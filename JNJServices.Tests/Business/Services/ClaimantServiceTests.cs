using Dapper;
using JNJServices.Business.Services;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.ApiConstants;
using JNJServices.Utility.DbConstants;
using Moq;
using System.Data;

namespace JNJServices.Tests.Business.Services
{
    public class ClaimantServiceTests
    {
        private readonly Mock<IDapperContext> _mockContext;
        private readonly ClaimantService _claimantService;

        public ClaimantServiceTests()
        {
            _mockContext = new Mock<IDapperContext>();
            _claimantService = new ClaimantService(_mockContext.Object);
        }

        [Fact]
        public async Task ClaimantSearch_ShouldCallExecuteQueryAsync_WithAllValidParameters()
        {
            // Arrange
            var model = new ClaimantSearchViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Mobile = "1234567890",
                LanguageCode = "EN",
                ClaimantID = 1,
                CustomerID = 2,
                Inactive = 0,
                ZipCode = 12345,
                Miles = 10,
                Page = 1,
                Limit = 10
            };

            var expectedResult = new List<vwClaimantSearch> { new vwClaimantSearch() };

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<vwClaimantSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _claimantService.ClaimantSearch(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<vwClaimantSearch>(
                It.Is<string>(proc => proc == ProcEntities.spClaimantSearch),
                It.Is<DynamicParameters>(p =>
                    p.Get<string>(DbParams.FirstName) == "John" &&
                    p.Get<string>(DbParams.LastName) == "Doe" &&
                    p.Get<string>(DbParams.Email) == "john.doe@example.com" &&
                    p.Get<string>(DbParams.Mobile) == "1234567890" &&
                    p.Get<string>(DbParams.Language) == "EN" &&
                    p.Get<int>(DbParams.ClaimantID) == 1 &&
                    p.Get<int>(DbParams.CustomerID) == 2 &&
                    p.Get<int>(DbParams.InActive) == 0 &&
                    p.Get<int>(DbParams.ZipCode) == 12345 &&
                    p.Get<int>(DbParams.Miles) == 10 &&
                    p.Get<int>(DbParams.Page) == 1 &&
                    p.Get<int>(DbParams.Limit) == 10
                ),
                CommandType.StoredProcedure), Times.Once);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ClaimantSearch_ShouldCallExecuteQueryAsync_WithDefaultPageAndLimit_WhenOptionalParametersAreMissing()
        {
            // Arrange
            var model = new ClaimantSearchViewModel
            {
                Page = null,
                Limit = null
            };
            var expectedResult = new List<vwClaimantSearch> { new vwClaimantSearch() };

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<vwClaimantSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _claimantService.ClaimantSearch(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<vwClaimantSearch>(
                It.Is<string>(proc => proc == ProcEntities.spClaimantSearch),
                It.Is<DynamicParameters>(p =>
                    p.Get<int>(DbParams.Page) == DefaultAppSettings.PageSize &&
                    p.Get<int>(DbParams.Limit) == DefaultAppSettings.PageLimit
                ),
                CommandType.StoredProcedure), Times.Once);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ClaimantSearch_ShouldReturnEmptyList_WhenNoRecordsFound()
        {
            // Arrange
            var model = new ClaimantSearchViewModel
            {
                FirstName = "NonExistent",
                LastName = "User"
            };

            var expectedResult = new List<vwClaimantSearch>(); // Expected empty result

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<vwClaimantSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _claimantService.ClaimantSearch(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<vwClaimantSearch>(
                It.Is<string>(proc => proc == ProcEntities.spClaimantSearch),
                It.Is<DynamicParameters>(p =>
                    p.Get<string>(DbParams.FirstName) == "NonExistent" &&
                    p.Get<string>(DbParams.LastName) == "User"
                ),
                CommandType.StoredProcedure), Times.Once);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AssignCustomerInfoToClaimants_ShouldAssignCustomerInfo_WhenCustomerExists()
        {
            // Arrange
            var claimants = new List<vwClaimantSearch>
        {
            new vwClaimantSearch { ClaimantID = 1 },
            new vwClaimantSearch { ClaimantID = 2 }
        };
            int customerId = 1;

            var customerInfo = new CustomerClaimantsInfoWebResponseModel
            {
                CustomerName = "Acme Corp",
                CompanyName = "Acme Services"
            };

            _mockContext
                .Setup(m => m.ExecuteQueryFirstOrDefaultAsync<CustomerClaimantsInfoWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync(customerInfo);

            // Act
            var result = await _claimantService.AssignCustomerInfoToClaimants(claimants, customerId);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryFirstOrDefaultAsync<CustomerClaimantsInfoWebResponseModel>(
                It.Is<string>(query => query.Contains("vwCustomerSearch")),
                It.Is<DynamicParameters>(p => p.Get<int>(DbParams.CustomerID) == customerId),
                CommandType.Text), Times.Once);

            Assert.All(result, claimant =>
            {
                Assert.Equal("Acme Corp", claimant.CustomerName);
                Assert.Equal("Acme Services", claimant.companyName);
            });
        }

        [Fact]
        public async Task AssignCustomerInfoToClaimants_ShouldSetEmptyFields_WhenCustomerNotFound()
        {
            // Arrange
            var claimants = new List<vwClaimantSearch>
        {
            new vwClaimantSearch { ClaimantID = 1 },
            new vwClaimantSearch { ClaimantID = 2 }
        };
            int customerId = 1;

            _mockContext
                .Setup(m => m.ExecuteQueryFirstOrDefaultAsync<CustomerClaimantsInfoWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync((CustomerClaimantsInfoWebResponseModel?)null); // No customer found

            // Act
            var result = await _claimantService.AssignCustomerInfoToClaimants(claimants, customerId);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryFirstOrDefaultAsync<CustomerClaimantsInfoWebResponseModel>(
                It.Is<string>(query => query.Contains("vwCustomerSearch")),
                It.Is<DynamicParameters>(p => p.Get<int>(DbParams.CustomerID) == customerId),
                CommandType.Text), Times.Once);

            Assert.All(result, claimant =>
            {
                Assert.Equal(string.Empty, claimant.CustomerName);
                Assert.Equal(string.Empty, claimant.companyName);
            });
        }

        [Fact]
        public async Task ClaimantByCustomerSearch_ShouldReturnClaimants_WhenCustomerIDIsValid()
        {
            // Arrange
            var model = new ClaimantCustomerSearchWebViewModel { CustomerID = 1 };
            var claimants = new List<vwClaimantSearch>
        {
            new vwClaimantSearch { ClaimantID = 1, CustomerName = "Acme Corp" },
            new vwClaimantSearch { ClaimantID = 2, CustomerName = "Acme Corp" }
        };

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<vwClaimantSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync(claimants);

            // Act
            var result = await _claimantService.ClaimantByCustomerSearch(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<vwClaimantSearch>(
                It.Is<string>(query => query.Contains("vwClaimantCustomerSearch")),
                It.Is<DynamicParameters>(p => p.Get<int>(DbParams.CustomerID) == model.CustomerID),
                CommandType.Text), Times.Once);

            Assert.Equal(claimants.Count, result.Count());
            Assert.All(result, claimant => Assert.Equal("Acme Corp", claimant.CustomerName));
        }

        [Fact]
        public async Task ClaimantByCustomerSearch_ShouldReturnEmpty_WhenCustomerIDNotFound()
        {
            // Arrange
            var model = new ClaimantCustomerSearchWebViewModel { CustomerID = 99 };
            var emptyList = new List<vwClaimantSearch>();

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<vwClaimantSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _claimantService.ClaimantByCustomerSearch(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<vwClaimantSearch>(
                It.Is<string>(query => query.Contains("vwClaimantCustomerSearch")),
                It.Is<DynamicParameters>(p => p.Get<int>(DbParams.CustomerID) == model.CustomerID),
                CommandType.Text), Times.Once);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ClaimantByCustomerSearch_ShouldReturnEmpty_WhenCustomerIDIsZeroOrNull()
        {
            // Arrange
            var model = new ClaimantCustomerSearchWebViewModel { CustomerID = 0 };
            var emptyList = new List<vwClaimantSearch>();

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<vwClaimantSearch>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _claimantService.ClaimantByCustomerSearch(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<vwClaimantSearch>(
                It.Is<string>(query => query.Contains("vwClaimantCustomerSearch")),
                It.Is<DynamicParameters>(p => p.Get<int>(DbParams.CustomerID) == model.CustomerID),
                CommandType.Text), Times.Once);

            Assert.Empty(result);
        }

        [Fact]
        public async Task AllClaimant_ShouldReturnClaimants_WhenClaimantNameIsValid()
        {
            // Arrange
            var model = new ClaimantDynamicSearchWebViewModel { ClaimantName = "John" };
            var claimants = new List<ClaimantDynamicSearchWebResponseModel>
        {
            new ClaimantDynamicSearchWebResponseModel { ClaimantID = 1, FullName = "John Doe - 1" },
            new ClaimantDynamicSearchWebResponseModel { ClaimantID = 2, FullName = "John Smith - 2" }
        };

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<ClaimantDynamicSearchWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync(claimants);

            // Act
            var result = await _claimantService.AllClaimant(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<ClaimantDynamicSearchWebResponseModel>(
                It.Is<string>(query => query.Contains("vwClaimantSearch")),
                It.Is<DynamicParameters>(p => p.Get<string>(DbParams.FullName) == $"{model.ClaimantName}%"),
                CommandType.Text), Times.Once);

            Assert.Equal(claimants.Count, result.Count());
            Assert.All(result, claimant => Assert.Contains(model.ClaimantName, claimant.FullName));
        }

        [Fact]
        public async Task AllClaimant_ShouldReturnEmpty_WhenClaimantNameIsEmpty()
        {
            // Arrange
            var model = new ClaimantDynamicSearchWebViewModel { ClaimantName = string.Empty };
            var emptyList = new List<ClaimantDynamicSearchWebResponseModel>();

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<ClaimantDynamicSearchWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _claimantService.AllClaimant(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<ClaimantDynamicSearchWebResponseModel>(
                It.Is<string>(query => query.Contains("vwClaimantSearch")),
                It.Is<DynamicParameters>(p => p.Get<string>(DbParams.FullName) == $"{model.ClaimantName}%"),
                CommandType.Text), Times.Once);

            Assert.Empty(result);
        }

        [Fact]
        public async Task AllClaimant_ShouldReturnEmpty_WhenNoClaimantsMatchName()
        {
            // Arrange
            var model = new ClaimantDynamicSearchWebViewModel { ClaimantName = "NonExistentName" };
            var emptyList = new List<ClaimantDynamicSearchWebResponseModel>();

            _mockContext
                .Setup(m => m.ExecuteQueryAsync<ClaimantDynamicSearchWebResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _claimantService.AllClaimant(model);

            // Assert
            _mockContext.Verify(m => m.ExecuteQueryAsync<ClaimantDynamicSearchWebResponseModel>(
                It.Is<string>(query => query.Contains("vwClaimantSearch")),
                It.Is<DynamicParameters>(p => p.Get<string>(DbParams.FullName) == $"{model.ClaimantName}%"),
                CommandType.Text), Times.Once);

            Assert.Empty(result);
        }
    }
}
