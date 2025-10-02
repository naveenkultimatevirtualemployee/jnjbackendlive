using Dapper;
using JNJServices.Business.Services;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.ApiConstants;
using JNJServices.Utility.DbConstants;
using Moq;
using System.Data;

namespace JNJServices.Tests.Business.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<IDapperContext> _mockDapperContext;
        private readonly CustomerService _customerService;
        public CustomerServiceTests()
        {
            _mockDapperContext = new Mock<IDapperContext>();

            // Arrange - Set up any necessary method setups for the mock (if needed)
            _mockDapperContext.Setup(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(1); // Mocking ExecuteAsync to return a successful execution (1)

            // Initialize UserService with the mocked IDapperContext
            _customerService = new CustomerService(_mockDapperContext.Object);
        }

        [Fact]
        public async Task GetCategory_ReturnsCustomerCategories_Success()
        {
            // Arrange
            var mockCategories = new List<CustomerCategory>
            {
                new CustomerCategory { code = "", description = "Category 1" },
                new CustomerCategory { code = "", description = "Category 2" }
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<CustomerCategory>(It.IsAny<string>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockCategories);

            // Act
            var result = await _customerService.GetCategory();

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.IsType<List<CustomerCategory>>(result); // Ensure the result is of type List<CustomerCategory>
            Assert.Equal(2, result.Count()); // Ensure the correct number of categories are returned
            Assert.Equal("Category 1", result.First().description); // Check if the first category description is correct
        }

        [Fact]
        public async Task GetCategory_ReturnsEmptyList_WhenNoCategories()
        {
            // Arrange
            var mockCategories = new List<CustomerCategory>(); // No categories to return

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<CustomerCategory>(It.IsAny<string>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockCategories);

            // Act
            var result = await _customerService.GetCategory();

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.Empty(result); // Ensure the result list is empty
        }

        [Fact]
        public async Task GetCategory_ThrowsException_WhenDatabaseFails()
        {
            // Arrange
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<CustomerCategory>(It.IsAny<string>(), It.IsAny<CommandType>()))
                              .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _customerService.GetCategory());
            Assert.Equal("Database error", exception.Message); // Check the exception message
        }

        [Fact]
        public async Task GetCategory_ExecutesQueryWithCorrectSQL()
        {
            // Arrange
            var mockCategories = new List<CustomerCategory>
            {
                new CustomerCategory {code = "", description = "Category 1"}
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<CustomerCategory>(It.IsAny<string>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockCategories);

            // Act
            var result = await _customerService.GetCategory();

            // Assert
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<CustomerCategory>(
                "Select * From codesCUSCT where inactiveflag = 0 order by description",
                CommandType.Text), Times.Once); // Verify the query was executed with the correct SQL
        }

        [Fact]
        public async Task CustomerSearch_ReturnsResults_WhenValidParametersAreProvided()
        {
            // Arrange
            var model = new CustomerSearchWebViewModel
            {
                CustomerID = 123,
                CustomerName = "Test Customer",
                Email = "test@example.com",
                States = "CA",
                Category = "Category1",
                Inactive = 0,
                Page = 1,
                Limit = 10
            };

            var mockResult = new List<vwCustomerSearch>
            {
                new vwCustomerSearch { CustomerID = 123, CustomerName = "Test Customer" }
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<vwCustomerSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.CustomerSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<vwCustomerSearch>>(result);
            Assert.Single(result);
            Assert.Equal("Test Customer", result.First().CustomerName);
        }

        [Fact]
        public async Task CustomerSearch_ReturnsEmptyList_WhenNoResultsMatch()
        {
            // Arrange
            var model = new CustomerSearchWebViewModel
            {
                CustomerID = 9999, // No customer with this ID
                CustomerName = "Non Existent Customer"
            };

            var mockResult = new List<vwCustomerSearch>();

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<vwCustomerSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.CustomerSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Ensure the result list is empty
        }

        [Fact]
        public async Task CustomerSearch_DoesNotAddNullOrEmptyParameters_ToQuery()
        {
            // Arrange
            var model = new CustomerSearchWebViewModel
            {
                CustomerID = 123,
                CustomerName = string.Empty, // Empty string should not add to parameters
                Email = null, // Null should not add to parameters
                States = "all", // Should not add StateCode parameter
                Category = "all", // Should not add Category parameter
                Inactive = null,
                Page = 1,
                Limit = 10
            };

            var mockResult = new List<vwCustomerSearch>
                {
                    new vwCustomerSearch { CustomerID = 123, CustomerName = "Test Customer" }
                };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<vwCustomerSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.CustomerSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Customer", result.First().CustomerName);

            // Verify the parameters to check if empty or null values were not included
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<vwCustomerSearch>(
                It.Is<string>(s => s.Contains(ProcEntities.spCustomerSearch)), It.Is<DynamicParameters>(p => !p.ParameterNames.Contains(DbParams.CustomerName) &&
                                                                                                                      !p.ParameterNames.Contains(DbParams.Email) &&
                                                                                                                      !p.ParameterNames.Contains(DbParams.StateCode) &&
                                                                                                                      !p.ParameterNames.Contains(DbParams.Category) &&
                                                                                                                      !p.ParameterNames.Contains(DbParams.InActive)),
                            CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task CustomerSearch_UsesDefaultValues_ForPageAndLimit_WhenNotProvided()
        {
            // Arrange
            var model = new CustomerSearchWebViewModel
            {
                CustomerID = 123,
                CustomerName = "Test Customer",
                States = "CA",
                Category = "Category1",
                Page = null,  // Null should use the default value
                Limit = null  // Null should use the default value
            };

            var mockResult = new List<vwCustomerSearch>
    {
        new vwCustomerSearch { CustomerID = 123, CustomerName = "Test Customer" }
    };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<vwCustomerSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.CustomerSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Customer", result.First().CustomerName);

            // Verify that default values for Page and Limit are being used
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<vwCustomerSearch>(
                It.Is<string>(s => s.Contains(ProcEntities.spCustomerSearch)),
                It.Is<DynamicParameters>(p => p.Get<int>(DbParams.Page) == DefaultAppSettings.PageSize &&
                                              p.Get<int>(DbParams.Limit) == DefaultAppSettings.PageLimit),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task CustomerSearch_AddsCorrectParameters_WhenValidModelIsProvided()
        {
            // Arrange
            var model = new CustomerSearchWebViewModel
            {
                CustomerID = 123,
                CustomerName = "Test Customer",
                States = "CA",
                Category = "Category1",
                Inactive = 0,
                Page = 1,
                Limit = 10
            };

            var mockResult = new List<vwCustomerSearch>
            {
                new vwCustomerSearch { CustomerID = 123, CustomerName = "Test Customer" }
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<vwCustomerSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.CustomerSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Customer", result.First().CustomerName);

            // Verify the parameters being passed to the stored procedure
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<vwCustomerSearch>(
                It.Is<string>(s => s.Contains(ProcEntities.spCustomerSearch)),
                It.Is<DynamicParameters>(p => p.ParameterNames.Contains(DbParams.CustomerID) &&
                                              p.ParameterNames.Contains(DbParams.CustomerName) &&
                                              p.ParameterNames.Contains(DbParams.StateCode) &&
                                              p.ParameterNames.Contains(DbParams.Category) &&
                                              p.ParameterNames.Contains(DbParams.InActive)),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task CustomerSearch_HandlesInvalidCustomerIDOrInactiveValues()
        {
            // Arrange
            var model = new CustomerSearchWebViewModel
            {
                CustomerID = -1,  // Invalid CustomerID
                CustomerName = "Invalid Customer"
            };

            var mockResult = new List<vwCustomerSearch>();

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<vwCustomerSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.CustomerSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Ensure no result is returned
        }

        [Fact]
        public async Task AllCustomers_ReturnsCustomers_WhenValidCustomerNameIsProvided()
        {
            // Arrange
            var model = new CustomerDynamicSearchWebViewModel
            {
                CustomerName = "Test" // Partial match will be searched (e.g., "Test%" in SQL)
            };

            var mockResult = new List<CustomerDynamicSearchWebResponseModel>
            {
                new CustomerDynamicSearchWebResponseModel { CustomerID = 123, FullName = "Test Customer - 123" },
                new CustomerDynamicSearchWebResponseModel { CustomerID = 456, FullName = "Test Customer - 456" }
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<CustomerDynamicSearchWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.AllCustomers(model);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<CustomerDynamicSearchWebResponseModel>>(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Test Customer - 123", result.First().FullName);
        }

        [Fact]
        public async Task AllCustomers_ReturnsEmptyList_WhenNoCustomersMatch()
        {
            // Arrange
            var model = new CustomerDynamicSearchWebViewModel
            {
                CustomerName = "Non Existent Customer"
            };

            var mockResult = new List<CustomerDynamicSearchWebResponseModel>(); // No matches

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<CustomerDynamicSearchWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.AllCustomers(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Ensure the result list is empty
        }

        [Fact]
        public async Task AllCustomers_ReturnsAllCustomers_WhenCustomerNameIsEmpty()
        {
            // Arrange
            var model = new CustomerDynamicSearchWebViewModel
            {
                CustomerName = "" // Empty string should search for all customers
            };

            var mockResult = new List<CustomerDynamicSearchWebResponseModel>
            {
                new CustomerDynamicSearchWebResponseModel { CustomerID = 123, FullName = "Customer 123" },
                new CustomerDynamicSearchWebResponseModel { CustomerID = 456, FullName = "Customer 456" }
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<CustomerDynamicSearchWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.AllCustomers(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Customer 123", result.First().FullName);
        }

        [Fact]
        public async Task AllCustomers_CorrectlyFormatsFullNameField_WithCustomerID()
        {
            // Arrange
            var model = new CustomerDynamicSearchWebViewModel
            {
                CustomerName = "Test" // Will search for customers starting with "Test"
            };

            var mockResult = new List<CustomerDynamicSearchWebResponseModel>
            {
                new CustomerDynamicSearchWebResponseModel { CustomerID = 123, FullName = "Test Customer - 123" },
                new CustomerDynamicSearchWebResponseModel { CustomerID = 456, FullName = "Test Customer - 456" }
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<CustomerDynamicSearchWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.AllCustomers(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Customer - 123", result.First().FullName);
            Assert.Equal("Test Customer - 456", result.Last().FullName);
        }

        [Fact]
        public async Task AllCustomers_ReturnsEmptyList_WhenCustomerNameIsNull()
        {
            // Arrange
            var model = new CustomerDynamicSearchWebViewModel
            {
                CustomerName = null // Null should behave the same as an empty string
            };

            var mockResult = new List<CustomerDynamicSearchWebResponseModel>(); // No customers for null name

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<CustomerDynamicSearchWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.AllCustomers(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Ensure the result list is empty
        }

        [Fact]
        public async Task AllCustomers_ReturnsCustomers_WhenSpecialCharactersInCustomerName()
        {
            // Arrange
            var model = new CustomerDynamicSearchWebViewModel
            {
                CustomerName = "T$@t" // Searching for customers with special characters in their name
            };

            var mockResult = new List<CustomerDynamicSearchWebResponseModel>
            {
                new CustomerDynamicSearchWebResponseModel { CustomerID = 789, FullName = "T$@t Customer - 789" }
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<CustomerDynamicSearchWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.AllCustomers(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("T$@t Customer - 789", result.First().FullName);
        }

        [Fact]
        public async Task CustomerByClaimantSearch_ReturnsCustomers_WhenValidClaimantIDIsProvided()
        {
            // Arrange
            var model = new ClaimantCustomerSearchWebViewModel
            {
                ClaimantID = 1 // Valid ClaimantID
            };

            var mockResult = new List<vwCustomerSearch>
            {
                new vwCustomerSearch { CustomerID = 123, Firstname = "John", Lastname = "Doe", ClaimantName = "Jane Doe" },
                new vwCustomerSearch { CustomerID = 456, Firstname = "Alice", Lastname = "Smith", ClaimantName = "Jack Smith" }
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<vwCustomerSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.CustomerByClaimantSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, r => r.Firstname == "John" && r.Lastname == "Doe");
            Assert.Contains(result, r => r.Firstname == "Alice" && r.Lastname == "Smith");
        }

        [Fact]
        public async Task CustomerByClaimantSearch_ReturnsEmptyList_WhenNoCustomersMatch()
        {
            // Arrange
            var model = new ClaimantCustomerSearchWebViewModel
            {
                ClaimantID = 99 // Non-existing ClaimantID
            };

            var mockResult = new List<vwCustomerSearch>(); // No customers found for ClaimantID 99

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<vwCustomerSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.CustomerByClaimantSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Ensure the result list is empty
        }

        [Fact]
        public async Task CustomerByClaimantSearch_ReturnsEmptyList_WhenClaimantIDIsNullOrInvalid()
        {
            // Arrange
            var model = new ClaimantCustomerSearchWebViewModel
            {
                ClaimantID = 0 // Invalid ClaimantID (assuming ClaimantID cannot be 0)
            };

            var mockResult = new List<vwCustomerSearch>(); // No customers for invalid ClaimantID

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<vwCustomerSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.CustomerByClaimantSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Ensure the result list is empty
        }

        [Fact]
        public async Task CustomerByClaimantSearch_ReturnsMultipleCustomers_WhenClaimantIDMatchesMultipleCustomers()
        {
            // Arrange
            var model = new ClaimantCustomerSearchWebViewModel
            {
                ClaimantID = 1 // Valid ClaimantID, expecting multiple results
            };

            var mockResult = new List<vwCustomerSearch>
            {
                new vwCustomerSearch { CustomerID = 123, Firstname = "John", Lastname = "Doe", ClaimantName = "Jane Doe" },
                new vwCustomerSearch { CustomerID = 456, Firstname = "Alice", Lastname = "Smith", ClaimantName = "Jack Smith" }
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<vwCustomerSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.CustomerByClaimantSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, r => r.Firstname == "John" && r.Lastname == "Doe");
            Assert.Contains(result, r => r.Firstname == "Alice" && r.Lastname == "Smith");
        }

        [Fact]
        public async Task CustomerByClaimantSearch_ReturnsEmptyList_WhenNoMatchingCustomers()
        {
            // Arrange
            var model = new ClaimantCustomerSearchWebViewModel
            {
                ClaimantID = 1000 // A ClaimantID that does not match any customers
            };

            var mockResult = new List<vwCustomerSearch>(); // No matching results

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<vwCustomerSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(mockResult);

            // Act
            var result = await _customerService.CustomerByClaimantSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Ensure the result list is empty
        }

        [Fact]
        public async Task CustomerByClaimantSearch_ThrowsException_WhenDatabaseFails()
        {
            // Arrange
            var model = new ClaimantCustomerSearchWebViewModel
            {
                ClaimantID = 1 // Valid ClaimantID
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<vwCustomerSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _customerService.CustomerByClaimantSearch(model));
        }


    }
}
