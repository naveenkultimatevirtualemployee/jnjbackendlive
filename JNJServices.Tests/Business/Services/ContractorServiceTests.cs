using Dapper;
using JNJServices.Business.Services;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.DbConstants;
using Moq;
using System.Data;

namespace JNJServices.Tests.Business.Services
{
    public class ContractorServiceTests
    {
        private readonly Mock<IDapperContext> _mockContext;
        private readonly ContractorService _contractorService;

        public ContractorServiceTests()
        {
            // Arrange
            _mockContext = new Mock<IDapperContext>();
            _contractorService = new ContractorService(_mockContext.Object);
        }

        [Fact]
        public async Task ContractorSearch_ShouldReturnResults_WhenValidModelIsProvided()
        {
            // Arrange
            var model = new ContractorSearchViewModel
            {
                ContractorID = 123,
                FirstName = "John",
                LastName = "Doe",
                Mobile = "1234567890",
                StatusCode = "Active",
                ServiceCode = "Plumbing",
                State = "NY",
                VehicleSize = "Large",
                LanguageCode = "EN",
                inactiveflag = 0,
                ZipCode = 10001,
                Miles = 10,
                Page = 1,
                Limit = 10
            };

            var expectedResults = new List<vwContractorSearch>
            {
                new vwContractorSearch { ContractorID = 123, FirstName = "John Doe", accountnumber = "1234567890" }
            };

            // Mock the database query result
            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorSearch>(
                    ProcEntities.spContractorsSearch,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _contractorService.ContractorSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);  // Verify that one record is returned
            Assert.Equal("John Doe", result.First().FirstName);  // Verify the content of the result
        }

        [Fact]
        public async Task ContractorSearch_ShouldReturnEmpty_WhenNoResultsFound()
        {
            // Arrange
            var model = new ContractorSearchViewModel
            {
                FirstName = "NonExistentName"
            };

            var expectedResults = new List<vwContractorSearch>();

            // Mock the database query result
            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorSearch>(
                    ProcEntities.spContractorsSearch,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _contractorService.ContractorSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);  // Verify that no records are returned
        }

        [Fact]
        public async Task ContractorSearch_ShouldHandleNullModelValues()
        {
            // Arrange
            var model = new ContractorSearchViewModel();  // Empty model

            var expectedResults = new List<vwContractorSearch>
            {
                new vwContractorSearch { ContractorID = 1, FirstName = "Default Contractor", accountnumber = "1112223333" }
            };

            // Mock the database query result
            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorSearch>(
                    ProcEntities.spContractorsSearch,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _contractorService.ContractorSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);  // Verify that default record is returned
            Assert.Equal("Default Contractor", result.First().FirstName);  // Verify the content of the result
        }

        [Fact]
        public async Task ContractorSearch_ShouldHandleInvalidZipCode()
        {
            // Arrange
            var model = new ContractorSearchViewModel
            {
                ZipCode = -1  // Invalid ZipCode
            };

            var expectedResults = new List<vwContractorSearch>();  // Assume no results for invalid input

            // Mock the database query result
            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorSearch>(
                    ProcEntities.spContractorsSearch,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _contractorService.ContractorSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);  // Verify that no records are returned for invalid input
        }

        [Fact]
        public async Task ContractorSearch_ShouldCallDatabaseWithCorrectParameters()
        {
            // Arrange
            var model = new ContractorSearchViewModel
            {
                ContractorID = 123,
                FirstName = "Jane"
            };

            // Act
            await _contractorService.ContractorSearch(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<vwContractorSearch>(
                ProcEntities.spContractorsSearch,
                It.Is<DynamicParameters>(p => p.Get<int>(DbParams.ContractorID) == 123 &&
                                              p.Get<string>(DbParams.FirstName) == "Jane"),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task GetContractorService_ShouldReturnActiveServices_WhenAvailable()
        {
            // Arrange
            var expectedResults = new List<ContractorServiceType>
        {
            new ContractorServiceType {code = "abc", description = "Plumbing"},
            new ContractorServiceType {code = "abcd", description = "Electrical"}
        };

            // Mock the database query result
            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorServiceType>(
                    It.Is<string>(q => q.Contains("codesCONTY")), // Verify the table name in the query
                    CommandType.Text))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _contractorService.GetContractorService();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count()); // Verify that two records are returned
            Assert.Equal("Plumbing", result.First().description); // Verify content of the first record
        }

        [Fact]
        public async Task GetContractorService_ShouldReturnEmptyList_WhenNoActiveServices()
        {
            // Arrange
            var expectedResults = new List<ContractorServiceType>(); // No active services

            // Mock the database query result
            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorServiceType>(
                    It.IsAny<string>(),
                    CommandType.Text))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _contractorService.GetContractorService();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Verify that the result is an empty list
        }

        [Fact]
        public async Task GetContractorService_ShouldExecuteQueryOnce_WithCorrectQuery()
        {
            // Arrange
            var expectedResults = new List<ContractorServiceType>
        {
            new ContractorServiceType { code = "abc", description = "Plumbing" }
        };

            // Mock the database query result
            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorServiceType>(
                    "Select * From codesCONTY where inactiveflag = 0 order by description",
                    CommandType.Text))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _contractorService.GetContractorService();

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorServiceType>(
                "Select * From codesCONTY where inactiveflag = 0 order by description",
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task GetContractorStatus_ShouldReturnActiveStatuses_WhenAvailable()
        {
            // Arrange
            var expectedResults = new List<ContractorStatus>
            {
                new ContractorStatus {code = "abc", description = "Active"},
                new ContractorStatus {code = "abcd", description = "Pending"}
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorStatus>(
                    It.Is<string>(q => q.Contains("codesCONST")), // Verifying the table in the query
                    CommandType.Text))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _contractorService.GetContractorStatus();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Active", result.First().description);
        }

        [Fact]
        public async Task GetContractorStatus_ShouldReturnEmptyList_WhenNoActiveStatuses()
        {
            // Arrange
            var expectedResults = new List<ContractorStatus>(); // Simulate no active statuses

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorStatus>(
                    It.IsAny<string>(),
                    CommandType.Text))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _contractorService.GetContractorStatus();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetContractorStatus_ShouldExecuteQueryOnce_WithCorrectQuery()
        {
            // Arrange
            var expectedResults = new List<ContractorStatus>
        {
            new ContractorStatus {code = "abc", description = "Active"}
        };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorStatus>(
                    "Select * From codesCONST where inactiveflag = 0 order by description",
                    CommandType.Text))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _contractorService.GetContractorStatus();

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorStatus>(
                "Select * From codesCONST where inactiveflag = 0 order by description",
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorServiceLoc_ShouldReturnAllServiceLocations_WhenContractorIDIsNull()
        {
            // Arrange
            var expectedResults = new List<ContractorServiceLocation>
        {
            new ContractorServiceLocation { ContractorID = 1, City = "Location1" },
            new ContractorServiceLocation { ContractorID = 2, City = "Location2" }
        };
            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorServiceLocation>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.Text))
                .ReturnsAsync(expectedResults);

            var model = new ContractorIDWebViewModel { ContractorID = null };

            // Act
            var result = await _contractorService.ContractorServiceLoc(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task ContractorServiceLoc_ShouldReturnEmptyList_WhenNoMatchingLocations()
        {
            // Arrange
            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorServiceLocation>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.Text))
                .ReturnsAsync(new List<ContractorServiceLocation>());

            var model = new ContractorIDWebViewModel { ContractorID = 999 }; // Non-existing ContractorID

            // Act
            var result = await _contractorService.ContractorServiceLoc(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ContractorAvlSearch_ShouldExecuteStoredProcedureWithCorrectParameters()
        {
            // Arrange
            var model = new ContractorAvailaleSearchWebViewModel
            {
                reservationid = 123,
                reservationassignmentsid = 456,
                zipCode = "12345",
                Miles = 10.0,  // Ensure this is a double (if required)
                vehsize = "Large",
                language = "EN",
                vehtype = "Truck",
                certified = "Yes",
                ContractorID = 1,
                status = "Active"
            };

            // Set up the mock context to return an empty list when ExecuteQueryAsync is called
            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorAvailableSearchWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .ReturnsAsync(new List<ContractorAvailableSearchWebResponseModel>());

            // Act
            await _contractorService.ContractorAvlSearch(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorAvailableSearchWebResponseModel>(
                It.Is<string>(s => s == ProcEntities.spRsvAssignContractorSearch),
                It.Is<DynamicParameters>(p =>
                    p.Get<int>(DbParams.ReservationID) == model.reservationid &&
                    p.Get<int>(DbParams.ReservationAssignmentsID) == model.reservationassignmentsid &&
                    p.Get<string>(DbParams.ZipCode) == model.zipCode &&
                    p.Get<double>(DbParams.Miles) == model.Miles &&  // Corrected to double
                    p.Get<string>(DbParams.VehSize) == model.vehsize &&
                    p.Get<string>(DbParams.Language) == model.language &&
                    p.Get<string>(DbParams.Type) == model.vehtype &&
                    p.Get<string>(DbParams.Certified) == model.certified &&
                    p.Get<int>(DbParams.ContractorID) == model.ContractorID &&  // Assuming ContractorID is an int
                    p.Get<string>(DbParams.Status) == model.status
                ),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task ApprovedContractorAvailableSearch_ShouldExecuteStoredProcedureWithCorrectParameters()
        {
            // Arrange
            var model = new ContractorAvailaleSearchWebViewModel
            {
                reservationid = 123,
                reservationassignmentsid = 456,
                zipCode = "12345",
                Miles = 10,  // Assuming Miles is an int
                vehsize = "Large",
                language = "EN",
                vehtype = "Truck",
                certified = "Yes",
                ContractorID = 1,
                status = "Active"
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorAvailableSearchWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .ReturnsAsync(new List<ContractorAvailableSearchWebResponseModel>());

            // Act
            await _contractorService.ApprovedContractorAvailableSearch(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorAvailableSearchWebResponseModel>(
                It.Is<string>(s => s == ProcEntities.spAssignmentApprovedContractorSearch),
                It.Is<DynamicParameters>(p =>
                    p.Get<int>(DbParams.ReservationID) == model.reservationid &&
                    p.Get<int>(DbParams.ReservationAssignmentsID) == model.reservationassignmentsid &&
                    p.Get<string>(DbParams.ZipCode) == model.zipCode &&
                    p.Get<double>(DbParams.Miles) == model.Miles &&  // Ensure Miles is treated as int
                    p.Get<string>(DbParams.VehSize) == model.vehsize &&
                    p.Get<string>(DbParams.Language) == model.language &&
                    p.Get<string>(DbParams.Type) == model.vehtype &&
                    p.Get<string>(DbParams.Certified) == model.certified &&
                    p.Get<string>(DbParams.Status) == model.status
                ),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task ApprovedContractorAvailableSearch_ShouldReturnEmptyListIfNoContractorsFound()
        {
            // Arrange
            var model = new ContractorAvailaleSearchWebViewModel
            {
                reservationid = 123,
                reservationassignmentsid = 456,
                zipCode = "12345",
                Miles = 10,
                vehsize = "Large",
                language = "EN",
                vehtype = "Truck",
                certified = "Yes",
                ContractorID = 1,
                status = "Active"
            };

            // Mocking the context to return an empty list
            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorAvailableSearchWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .ReturnsAsync(new List<ContractorAvailableSearchWebResponseModel>());

            // Act
            var result = await _contractorService.ApprovedContractorAvailableSearch(model);

            // Assert
            Assert.Empty(result); // Ensure the result is an empty list
        }

        [Fact]
        public async Task ApprovedContractorAvailableSearch_ShouldHandleExceptionGracefully()
        {
            // Arrange
            var model = new ContractorAvailaleSearchWebViewModel
            {
                reservationid = 123,
                reservationassignmentsid = 456,
                zipCode = "12345",
                Miles = 10,
                vehsize = "Large",
                language = "EN",
                vehtype = "Truck",
                certified = "Yes",
                ContractorID = 1,
                status = "Active"
            };

            // Mocking the context to throw an exception
            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorAvailableSearchWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _contractorService.ApprovedContractorAvailableSearch(model));
            Assert.Equal("Database error", exception.Message); // Ensure the exception message is correct
        }

        [Fact]
        public async Task ApprovedContractorAvailableSearch_ShouldReturnListOfContractorAvailableSearchWebResponseModel()
        {
            // Arrange
            var model = new ContractorAvailaleSearchWebViewModel
            {
                reservationid = 123,
                reservationassignmentsid = 456,
                zipCode = "12345",
                Miles = 10,
                vehsize = "Large",
                language = "EN",
                vehtype = "Truck",
                certified = "Yes",
                ContractorID = 1,
                status = "Active"
            };

            // Mocking the context to return a list with a sample contractor response
            var mockResponse = new List<ContractorAvailableSearchWebResponseModel>
            {
                new ContractorAvailableSearchWebResponseModel { contractorid = 1, contractorname = "Sample Contractor" }
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorAvailableSearchWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .ReturnsAsync(mockResponse);

            // Act
            var result = await _contractorService.ApprovedContractorAvailableSearch(model);

            // Assert
            Assert.NotEmpty(result);  // Ensure the list is not empty
            Assert.IsType<List<ContractorAvailableSearchWebResponseModel>>(result);  // Ensure the result is of the correct type
        }

        [Fact]
        public async Task ContractorRates_ShouldGenerateQueryWithAllParameters()
        {
            // Arrange
            var model = new ContractorRatesSearchViewModel
            {
                RateCode = "RC123",
                StateCode = "TX",
                EffectiveDate = "2024-11-15",
                Inactiveflag = -1
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorRates>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorRates>());

            // Act
            await _contractorService.ContractorRates(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorRates>(
                It.Is<string>(query =>
                    query.Contains("Select * From ContractorRates where 1=1") &&
                    query.Contains("and RATECTCode = @Ratecode") &&
                    query.Contains("and STATECode = @STATEcode") &&
                    query.Contains("and EffectiveDateFrom = @EffectiveDate") &&
                    query.Contains("and inactiveflag = @Inactiveflag")
                ),
                It.Is<DynamicParameters>(parameters =>
                    parameters.Get<string>(DbParams.RateCode) == model.RateCode &&
                    parameters.Get<string>(DbParams.StateCode) == model.StateCode &&
                    parameters.Get<string>(DbParams.EffectiveDate) == model.EffectiveDate &&
                    parameters.Get<int>(DbParams.InactiveFlag) == model.Inactiveflag
                ),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorRates_ShouldGenerateQueryWithPartialParameters()
        {
            // Arrange
            var model = new ContractorRatesSearchViewModel
            {
                RateCode = "RC123",
                StateCode = null,  // Missing parameter
                EffectiveDate = "2024-11-15",
                Inactiveflag = null // Missing parameter
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorRates>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorRates>());

            // Act
            await _contractorService.ContractorRates(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorRates>(
                It.Is<string>(query =>
                    query.Contains("Select * From ContractorRates where 1=1") &&
                    query.Contains("and RATECTCode = @Ratecode") &&
                    !query.Contains("and STATECode = @STATEcode") && // Ensure missing params are not included
                    query.Contains("and EffectiveDateFrom = @EffectiveDate") &&
                    !query.Contains("and inactiveflag = @Inactiveflag")
                ),
                It.Is<DynamicParameters>(parameters =>
                    parameters.Get<string>(DbParams.RateCode) == model.RateCode &&
                    !parameters.ParameterNames.Contains(DbParams.StateCode) &&
                    parameters.Get<string>(DbParams.EffectiveDate) == model.EffectiveDate &&
                    !parameters.ParameterNames.Contains(DbParams.InactiveFlag)
                ),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorRates_ShouldReturnMatchingResults()
        {
            // Arrange
            var model = new ContractorRatesSearchViewModel
            {
                RateCode = "RC123",
                StateCode = "TX",
                EffectiveDate = "2024-11-15",
                Inactiveflag = 1
            };

            var expectedResults = new List<ContractorRates>
            {
                new ContractorRates { RATECTCode = "RC123", STATECode = "TX" }
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorRates>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(expectedResults);

            // Act
            var results = await _contractorService.ContractorRates(model);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(expectedResults.Count, results.Count());
            Assert.Equal(expectedResults.First().RATECTCode, results.First().RATECTCode);
        }

        [Fact]
        public async Task ContractorRates_ShouldHandleExceptionGracefully()
        {
            // Arrange
            var model = new ContractorRatesSearchViewModel
            {
                RateCode = "RC123",
                StateCode = "TX",
                EffectiveDate = "2024-11-15",
                Inactiveflag = 1
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorRates>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _contractorService.ContractorRates(model));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task ContractorAvailablehours_ShouldIncludeContractorIDInQuery()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 123
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorsAvailableHours>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorsAvailableHours>());

            // Act
            await _contractorService.ContractorAvailablehours(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorsAvailableHours>(
                It.Is<string>(query =>
                    query.Contains("(@ContractorID IS NULL OR ContractorID = @ContractorID)")
                ),
                It.Is<DynamicParameters>(parameters =>
                    parameters.Get<int>(DbParams.ContractorID) == model.ContractorID
                ),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorAvailablehours_ShouldExcludeContractorIDWhenNullOrZero()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 0
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorsAvailableHours>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorsAvailableHours>());

            // Act
            await _contractorService.ContractorAvailablehours(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorsAvailableHours>(
                It.Is<string>(query =>
                    query.Contains("(@ContractorID IS NULL OR ContractorID = @ContractorID)")
                ),
                It.Is<DynamicParameters>(parameters =>
                    !parameters.ParameterNames.Contains(DbParams.ContractorID)
                ),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorAvailablehours_ShouldReturnMatchingResults()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 123
            };

            var expectedResults = new List<ContractorsAvailableHours>
            {
                new ContractorsAvailableHours { AvailableDayNum = 1, StartTime = TimeSpan.Parse("08:00:00"), EndTime = TimeSpan.Parse("17:00:00") }
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorsAvailableHours>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(expectedResults);

            // Act
            var results = await _contractorService.ContractorAvailablehours(model);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(expectedResults.Count, results.Count());
        }

        [Fact]
        public async Task ContractorAvailablehours_ShouldReturnEmptyListWhenNoResults()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 456 // ContractorID with no data
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorsAvailableHours>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorsAvailableHours>());

            // Act
            var results = await _contractorService.ContractorAvailablehours(model);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task ContractorAvailablehours_ShouldHandleExceptionGracefully()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 123
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorsAvailableHours>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _contractorService.ContractorAvailablehours(model));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task ContractorAvailablehours_ShouldReturnCorrectTimeFormat()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 123
            };

            var expectedResults = new List<ContractorsAvailableHours>
            {
                new ContractorsAvailableHours {  StartTime = TimeSpan.Parse("08:00:00"), EndTime = TimeSpan.Parse("17:00:00") }
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorsAvailableHours>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(expectedResults);

            // Act
            var results = await _contractorService.ContractorAvailablehours(model);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(TimeSpan.Parse("08:00:00"), results.First().StartTime);
            Assert.Equal(TimeSpan.Parse("17:00:00"), results.First().EndTime);
        }

        [Fact]
        public async Task ContractorDriver_ShouldIncludeIsPrimaryInQuery_WhenProvided()
        {
            // Arrange
            var model = new ContractorDriverSearchViewModel
            {
                ContractorID = 123,
                IsPrimary = 1
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorDriversSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<vwContractorDriversSearch>());

            // Act
            await _contractorService.ContractorDriver(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<vwContractorDriversSearch>(
                It.Is<string>(query => query.Contains("and IsPrimary = @IsPrimary")),
                It.Is<DynamicParameters>(parameters =>
                    parameters.Get<int>(DbParams.ContractorID) == model.ContractorID &&
                    parameters.Get<int>(DbParams.IsPrimary) == model.IsPrimary
                ),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorDriver_ShouldExcludeIsPrimaryInQuery_WhenNull()
        {
            // Arrange
            var model = new ContractorDriverSearchViewModel
            {
                ContractorID = 123,
                IsPrimary = null
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorDriversSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<vwContractorDriversSearch>());

            // Act
            await _contractorService.ContractorDriver(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<vwContractorDriversSearch>(
                It.Is<string>(query => !query.Contains("and IsPrimary = @IsPrimary")),
                It.Is<DynamicParameters>(parameters =>
                    parameters.Get<int>(DbParams.ContractorID) == model.ContractorID &&
                    !parameters.ParameterNames.Contains(DbParams.IsPrimary)
                ),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorDriver_ShouldReturnCorrectResults_WhenDataExists()
        {
            // Arrange
            var model = new ContractorDriverSearchViewModel
            {
                ContractorID = 123,
                IsPrimary = 1
            };

            var expectedResults = new List<vwContractorDriversSearch>
            {
                new vwContractorDriversSearch { ContractorID = 123, IsPrimary = 1 }
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorDriversSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(expectedResults);

            // Act
            var results = await _contractorService.ContractorDriver(model);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(expectedResults.Count, results.Count());
            Assert.Equal(expectedResults.First().ContractorID, results.First().ContractorID);
            Assert.Equal(expectedResults.First().IsPrimary, results.First().IsPrimary);
        }

        [Fact]
        public async Task ContractorDriver_ShouldReturnEmptyList_WhenNoDataExists()
        {
            // Arrange
            var model = new ContractorDriverSearchViewModel
            {
                ContractorID = 456, // Non-existent ContractorID
                IsPrimary = 1
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorDriversSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<vwContractorDriversSearch>());

            // Act
            var results = await _contractorService.ContractorDriver(model);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task ContractorDriver_ShouldHandleExceptionGracefully()
        {
            // Arrange
            var model = new ContractorDriverSearchViewModel
            {
                ContractorID = 123,
                IsPrimary = 1
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorDriversSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _contractorService.ContractorDriver(model));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task ContractorDriver_ShouldPreventSQLInjection()
        {
            // Arrange
            var model = new ContractorDriverSearchViewModel
            {
                ContractorID = 123,
                IsPrimary = 1 // Simulate possible injection in the input
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorDriversSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<vwContractorDriversSearch>());

            // Act
            await _contractorService.ContractorDriver(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<vwContractorDriversSearch>(
                It.IsAny<string>(),
                It.Is<DynamicParameters>(parameters =>
                    parameters.ParameterNames.Contains(DbParams.ContractorID) &&
                    parameters.ParameterNames.Contains(DbParams.IsPrimary)
                ),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorLang_ShouldIncludeContractorIDInQuery_WhenProvided()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 123
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorLanguageSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<vwContractorLanguageSearch>());

            // Act
            await _contractorService.ContractorLang(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<vwContractorLanguageSearch>(
                It.Is<string>(query => query.Contains("(@ContractorID IS NULL OR ContractorID = @ContractorID)")),
                It.Is<DynamicParameters>(parameters =>
                    parameters.Get<int>(DbParams.ContractorID) == model.ContractorID),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorLang_ShouldNotAddContractorIDParameter_WhenInvalid()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 0 // Invalid ContractorID
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorLanguageSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<vwContractorLanguageSearch>());

            // Act
            await _contractorService.ContractorLang(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<vwContractorLanguageSearch>(
                It.Is<string>(query => query.Contains("(@ContractorID IS NULL OR ContractorID = @ContractorID)")),
                It.Is<DynamicParameters>(parameters =>
                    !parameters.ParameterNames.Contains(DbParams.ContractorID)),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorLang_ShouldReturnCorrectResults_WhenDataExists()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 123
            };

            var expectedResults = new List<vwContractorLanguageSearch>
    {
        new vwContractorLanguageSearch { ContractorID = 123, LANGUCode = "English" },
        new vwContractorLanguageSearch { ContractorID = 123, LANGUCode = "Spanish" }
    };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorLanguageSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(expectedResults);

            // Act
            var results = await _contractorService.ContractorLang(model);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(expectedResults.Count, results.Count());
            Assert.Equal(expectedResults.First().ContractorID, results.First().ContractorID);
            Assert.Equal(expectedResults.First().LANGUCode, results.First().LANGUCode);
        }

        [Fact]
        public async Task ContractorLang_ShouldReturnEmptyList_WhenNoDataExists()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 999 // ID with no corresponding data
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorLanguageSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<vwContractorLanguageSearch>());

            // Act
            var results = await _contractorService.ContractorLang(model);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task ContractorLang_ShouldHandleExceptionsGracefully()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 123
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorLanguageSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _contractorService.ContractorLang(model));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task ContractorLang_ShouldPreventSQLInjection()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 123
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<vwContractorLanguageSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<vwContractorLanguageSearch>());

            // Act
            await _contractorService.ContractorLang(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<vwContractorLanguageSearch>(
                It.IsAny<string>(),
                It.Is<DynamicParameters>(parameters =>
                    parameters.ParameterNames.Contains(DbParams.ContractorID)),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorSelectiveDetails_ShouldIncludeContractorIDInQuery_WhenProvided()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 123
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorShowSelectiveWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorShowSelectiveWebResponseModel>());

            // Act
            await _contractorService.ContractorSelectiveDetails(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorShowSelectiveWebResponseModel>(
                It.Is<string>(query => query.Contains("(@ContractorID IS NULL OR ContractorID = @ContractorID)")),
                It.Is<DynamicParameters>(parameters =>
                    parameters.Get<int>(DbParams.ContractorID) == model.ContractorID),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorSelectiveDetails_ShouldNotAddContractorIDParameter_WhenInvalid()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 0 // Invalid ContractorID
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorShowSelectiveWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorShowSelectiveWebResponseModel>());

            // Act
            await _contractorService.ContractorSelectiveDetails(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorShowSelectiveWebResponseModel>(
                It.Is<string>(query => query.Contains("(@ContractorID IS NULL OR ContractorID = @ContractorID)")),
                It.Is<DynamicParameters>(parameters =>
                    !parameters.ParameterNames.Contains(DbParams.ContractorID)),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorSelectiveDetails_ShouldReturnCorrectResults_WhenDataExists()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 123
            };

            var expectedResults = new List<ContractorShowSelectiveWebResponseModel>
    {
        new ContractorShowSelectiveWebResponseModel { Company = "ABC Corp" },
        new ContractorShowSelectiveWebResponseModel { Company = "XYZ Ltd" }
    };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorShowSelectiveWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(expectedResults);

            // Act
            var results = await _contractorService.ContractorSelectiveDetails(model);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(expectedResults.Count, results.Count());
            Assert.Equal(expectedResults.First().Company, results.First().Company);
        }

        [Fact]
        public async Task ContractorSelectiveDetails_ShouldReturnEmptyList_WhenNoDataExists()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 999 // ID with no corresponding data
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorShowSelectiveWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorShowSelectiveWebResponseModel>());

            // Act
            var results = await _contractorService.ContractorSelectiveDetails(model);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task ContractorSelectiveDetails_ShouldHandleExceptionsGracefully()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 123
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorShowSelectiveWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _contractorService.ContractorSelectiveDetails(model));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task ContractorSelectiveDetails_ShouldPreventSQLInjection()
        {
            // Arrange
            var model = new ContractorIDWebViewModel
            {
                ContractorID = 123
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorShowSelectiveWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorShowSelectiveWebResponseModel>());

            // Act
            await _contractorService.ContractorSelectiveDetails(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorShowSelectiveWebResponseModel>(
                It.IsAny<string>(),
                It.Is<DynamicParameters>(parameters =>
                    parameters.ParameterNames.Contains(DbParams.ContractorID)),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorAssignmentJobStatus_ShouldReturnResults_WhenValidReservationsAssignmentsIDProvided()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel
            {
                ReservationsAssignmentsID = 12345
            };

            var expectedResults = new List<ContractorJobSearch>
            {
                new ContractorJobSearch { JobStatus = 0 },
                new ContractorJobSearch { JobStatus = 1 }
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorJobSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(expectedResults);

            // Act
            var results = await _contractorService.ContractorAssignmentJobStatus(model);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(expectedResults.Count, results.Count());
            Assert.Equal(expectedResults.First().JobStatus, results.First().JobStatus);
        }

        [Fact]
        public async Task ContractorAssignmentJobStatus_ShouldReturnEmptyResults_WhenInvalidReservationsAssignmentsIDProvided()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel
            {
                ReservationsAssignmentsID = 0 // Invalid ID
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorJobSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorJobSearch>());

            // Act
            var results = await _contractorService.ContractorAssignmentJobStatus(model);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task ContractorAssignmentJobStatus_ShouldExecuteQueryWithCorrectParameters()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel
            {
                ReservationsAssignmentsID = 12345
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorJobSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorJobSearch>());

            // Act
            await _contractorService.ContractorAssignmentJobStatus(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorJobSearch>(
                It.Is<string>(query => query.Contains("ReservationsAssignmentsID = @ReservationsAssignmentsID")),
                It.Is<DynamicParameters>(parameters =>
                    parameters.Get<int>(DbParams.ReservationsAssignmentsID) == model.ReservationsAssignmentsID),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorAssignmentJobStatus_ShouldHandleEmptyResultsGracefully()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel
            {
                ReservationsAssignmentsID = 99999 // ID with no corresponding data
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorJobSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorJobSearch>());

            // Act
            var results = await _contractorService.ContractorAssignmentJobStatus(model);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task ContractorAssignmentJobStatus_ShouldPreventSQLInjection()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel
            {
                ReservationsAssignmentsID = 12345
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorJobSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorJobSearch>());

            // Act
            await _contractorService.ContractorAssignmentJobStatus(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorJobSearch>(
                It.IsAny<string>(),
                It.Is<DynamicParameters>(parameters =>
                    parameters.ParameterNames.Contains(DbParams.ReservationsAssignmentsID)),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorAssignmentJobStatus_ShouldHandleExceptionsGracefully()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel
            {
                ReservationsAssignmentsID = 12345
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorJobSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _contractorService.ContractorAssignmentJobStatus(model));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task AllContractor_ShouldReturnResults_WhenValidContractorNameIsProvided()
        {
            // Arrange
            var model = new ContractorDynamicWebViewModel
            {
                ContractorName = "John"
            };

            var expectedResults = new List<ContractorDynamicSearchWebResponseModel>
    {
        new ContractorDynamicSearchWebResponseModel { ContractorID = 1, FullName = "John Doe - 1" },
        new ContractorDynamicSearchWebResponseModel { ContractorID = 2, FullName = "Johnny Bravo - 2" }
    };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorDynamicSearchWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(expectedResults);

            // Act
            var results = await _contractorService.AllContractor(model);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(expectedResults.Count, results.Count());
            Assert.Contains(results, r => r.FullName == "John Doe - 1");
            Assert.Contains(results, r => r.FullName == "Johnny Bravo - 2");
        }

        [Fact]
        public async Task AllContractor_ShouldReturnEmptyResults_WhenNoMatchingContractorName()
        {
            // Arrange
            var model = new ContractorDynamicWebViewModel
            {
                ContractorName = "NonExistentName"
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorDynamicSearchWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorDynamicSearchWebResponseModel>());

            // Act
            var results = await _contractorService.AllContractor(model);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task AllContractor_ShouldExecuteQueryWithCorrectParameters()
        {
            // Arrange
            var model = new ContractorDynamicWebViewModel
            {
                ContractorName = "John"
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorDynamicSearchWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorDynamicSearchWebResponseModel>());

            // Act
            await _contractorService.AllContractor(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorDynamicSearchWebResponseModel>(
                It.Is<string>(query => query.Contains("FullName LIKE @FullName")),
                It.Is<DynamicParameters>(parameters =>
                    parameters.Get<string>(DbParams.FullName) == "John%"),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task AllContractor_ShouldHandleNullContractorName_Gracefully()
        {
            // Arrange
            var model = new ContractorDynamicWebViewModel
            {

            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorDynamicSearchWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorDynamicSearchWebResponseModel>());

            // Act
            var results = await _contractorService.AllContractor(model);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task AllContractor_ShouldPreventSQLInjection()
        {
            // Arrange
            var model = new ContractorDynamicWebViewModel
            {
                ContractorName = "'; DROP TABLE vwContractorSearch; --"
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorDynamicSearchWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(new List<ContractorDynamicSearchWebResponseModel>());

            // Act
            var results = await _contractorService.AllContractor(model);

            // Assert
            Assert.NotNull(results);
            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<ContractorDynamicSearchWebResponseModel>(
                It.IsAny<string>(),
                It.Is<DynamicParameters>(parameters =>
                    parameters.ParameterNames.Contains(DbParams.FullName)),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task AllContractor_ShouldReturnFormattedFullName()
        {
            // Arrange
            var model = new ContractorDynamicWebViewModel
            {
                ContractorName = "John"
            };

            var expectedResults = new List<ContractorDynamicSearchWebResponseModel>
    {
        new ContractorDynamicSearchWebResponseModel { ContractorID = 1, FullName = "John Doe - 1" }
    };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorDynamicSearchWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(expectedResults);

            // Act
            var results = await _contractorService.AllContractor(model);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(expectedResults.First().FullName, results.First().FullName);
        }

        [Fact]
        public async Task AllContractor_ShouldHandleExceptionsGracefully()
        {
            // Arrange
            var model = new ContractorDynamicWebViewModel
            {
                ContractorName = "John"
            };

            _mockContext.Setup(ctx => ctx.ExecuteQueryAsync<ContractorDynamicSearchWebResponseModel>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _contractorService.AllContractor(model));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task ContractorWebJobSearch_ShouldReturnResponse_WhenInputsAreValid()
        {
            // Arrange
            var model = new ContractorJobSearchWebViewModel
            {
                ReservationID = 123,
                ReservationAssignmentsID = 456
            };

            _mockContext.Setup(ctx => ctx.ExecuteScalerAsync(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .Callback<string, DynamicParameters, CommandType>((_, parameters, _) =>
            {
                parameters.Add(DbParams.ResponseCode, 1, dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add(DbParams.Msg, "Success", dbType: DbType.String, direction: ParameterDirection.Output);
            });

            // Act
            var result = await _contractorService.ContractorWebJobSearch(model);

            // Assert
            Assert.Equal(1, result.Item1);
            Assert.Equal("Success", result.Item2);
        }

        [Fact]
        public async Task ContractorWebJobSearch_ShouldHandleMissingInputs()
        {
            // Arrange
            var model = new ContractorJobSearchWebViewModel
            {
                ReservationID = null,
                ReservationAssignmentsID = null
            };

            _mockContext.Setup(ctx => ctx.ExecuteScalerAsync(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .Callback<string, DynamicParameters, CommandType>((_, parameters, _) =>
            {
                parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add(DbParams.Msg, "Invalid Inputs", dbType: DbType.String, direction: ParameterDirection.Output);
            });

            // Act
            var result = await _contractorService.ContractorWebJobSearch(model);

            // Assert
            Assert.Equal(0, result.Item1);
            Assert.Equal("Invalid Inputs", result.Item2);
        }

        [Fact]
        public async Task ContractorWebJobSearch_ShouldReturnDefaultMessage_WhenMsgParameterIsNull()
        {
            // Arrange
            var model = new ContractorJobSearchWebViewModel
            {
                ReservationID = 123,
                ReservationAssignmentsID = 456
            };

            _mockContext.Setup(ctx => ctx.ExecuteScalerAsync(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .Callback<string, DynamicParameters, CommandType>((_, parameters, _) =>
            {
                parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add(DbParams.Msg, null, dbType: DbType.String, direction: ParameterDirection.Output);
            });

            // Act
            var result = await _contractorService.ContractorWebJobSearch(model);

            // Assert
            Assert.Equal(0, result.Item1);
            Assert.Equal("Contractor Entered Already", result.Item2);
        }

        [Fact]
        public async Task ContractorWebJobSearch_ShouldHandleDatabaseErrors_Gracefully()
        {
            // Arrange
            var model = new ContractorJobSearchWebViewModel
            {
                ReservationID = 123,
                ReservationAssignmentsID = 456
            };

            _mockContext.Setup(ctx => ctx.ExecuteScalerAsync(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _contractorService.ContractorWebJobSearch(model));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task ContractorWebJobSearch_ShouldExecuteStoredProcedureWithCorrectParameters()
        {
            // Arrange
            var model = new ContractorJobSearchWebViewModel
            {
                ReservationID = 123,
                ReservationAssignmentsID = 456
            };

            _mockContext.Setup(ctx => ctx.ExecuteScalerAsync(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .Verifiable();

            // Act
            await _contractorService.ContractorWebJobSearch(model);

            // Assert
            _mockContext.Verify(ctx => ctx.ExecuteScalerAsync(
                It.Is<string>(proc => proc == ProcEntities.spWebContractorJobSearch),
                It.Is<DynamicParameters>(parameters =>
                    parameters.Get<int?>(DbParams.ReservationID) == 123 &&
                    parameters.Get<int?>(DbParams.AssignmentID) == 456),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task ContractorWebJobSearch_ShouldHandleNullReservationID()
        {
            // Arrange
            var model = new ContractorJobSearchWebViewModel
            {
                ReservationID = null,
                ReservationAssignmentsID = 456
            };

            _mockContext.Setup(ctx => ctx.ExecuteScalerAsync(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .Callback<string, DynamicParameters, CommandType>((_, parameters, _) =>
            {
                parameters.Add(DbParams.ResponseCode, 1, dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add(DbParams.Msg, "Success with null ReservationID", dbType: DbType.String, direction: ParameterDirection.Output);
            });

            // Act
            var result = await _contractorService.ContractorWebJobSearch(model);

            // Assert
            Assert.Equal(1, result.Item1);
            Assert.Equal("Success with null ReservationID", result.Item2);
        }

        [Fact]
        public async Task ContractorWebJobSearch_ShouldReturnCorrectOutputParameters()
        {
            // Arrange
            var model = new ContractorJobSearchWebViewModel
            {
                ReservationID = 123,
                ReservationAssignmentsID = 456
            };

            _mockContext.Setup(ctx => ctx.ExecuteScalerAsync(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .Callback<string, DynamicParameters, CommandType>((_, parameters, _) =>
            {
                parameters.Add(DbParams.ResponseCode, 2, dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add(DbParams.Msg, "Duplicate Entry", dbType: DbType.String, direction: ParameterDirection.Output);
            });

            // Act
            var result = await _contractorService.ContractorWebJobSearch(model);

            // Assert
            Assert.Equal(2, result.Item1);
            Assert.Equal("Duplicate Entry", result.Item2);
        }

        [Fact]
        public async Task ContractorRateDetails_ReturnsData_WhenAllParametersProvided()
        {
            // Arrange
            var mockResult = new List<ContractorRatesDetails>
            {
                new ContractorRatesDetails { ContractorRatesID = 1, ACCTGCode = "ACC123", LANGUCode = "English" }
            };

            _mockContext.Setup(db => db.ExecuteQueryAsync<ContractorRatesDetails>(
                It.Is<string>(q => q.Contains("and crd.ContractorRatesID = @ContractorRatesID") &&
                                   q.Contains("and crd.ACCTGCode = @ACCTGCode") &&
                                   q.Contains("and crd.TRNTYCode = @TRNTYCode") &&
                                   q.Contains("and crd.LANGUCode = @LANGUCode") &&
                                   q.Contains("and ca.LOBCode = @LOBCode")),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(mockResult);

            var model = new ContractorRatesDetailSearchViewModel
            {
                ContractorRatesID = 1,
                ACCTGCode = "ACC123",
                TransType = "TRN456",
                Language = "ENG",
                LOB = "LOB001"
            };

            // Act
            var result = await _contractorService.ContractorRateDetails(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result.First().ContractorRatesID);
        }

        [Fact]
        public async Task ContractorRateDetails_ReturnsAllData_WhenNoParametersProvided()
        {
            // Arrange
            var mockResult = new List<ContractorRatesDetails>
    {
        new ContractorRatesDetails { ContractorRatesID = 1, ACCTGCode = "ACC123", LANGUCode = "English" },
        new ContractorRatesDetails { ContractorRatesID = 2, ACCTGCode = "ACC124", LANGUCode = "Spanish" }
    };

            _mockContext.Setup(db => db.ExecuteQueryAsync<ContractorRatesDetails>(
                It.Is<string>(q => !q.Contains("@ContractorRatesID") &&
                                   !q.Contains("@ACCTGCode") &&
                                   !q.Contains("@TRNTYCode") &&
                                   !q.Contains("@LANGUCode") &&
                                   !q.Contains("@LOBCode")),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(mockResult);

            var model = new ContractorRatesDetailSearchViewModel(); // No parameters

            // Act
            var result = await _contractorService.ContractorRateDetails(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task ContractorRateDetails_HandlesPartialParameters()
        {
            // Arrange
            var mockResult = new List<ContractorRatesDetails>
    {
        new ContractorRatesDetails { ContractorRatesID = 1, ACCTGCode = "ACC123", LANGUCode = "English" }
    };

            _mockContext.Setup(db => db.ExecuteQueryAsync<ContractorRatesDetails>(
                It.Is<string>(q => q.Contains("and crd.ContractorRatesID = @ContractorRatesID") &&
                                   !q.Contains("@ACCTGCode") &&
                                   !q.Contains("@TRNTYCode") &&
                                   !q.Contains("@LANGUCode") &&
                                   !q.Contains("@LOBCode")),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(mockResult);

            var model = new ContractorRatesDetailSearchViewModel
            {
                ContractorRatesID = 1
            };

            // Act
            var result = await _contractorService.ContractorRateDetails(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result.First().ContractorRatesID);
        }

        [Fact]
        public async Task ContractorRateDetails_ReturnsEmpty_WhenInvalidParametersProvided()
        {
            // Arrange
            var mockResult = new List<ContractorRatesDetails>(); // No results

            _mockContext.Setup(db => db.ExecuteQueryAsync<ContractorRatesDetails>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(mockResult);

            var model = new ContractorRatesDetailSearchViewModel
            {
                ContractorRatesID = 9999, // Invalid
                ACCTGCode = "INVALID"
            };

            // Act
            var result = await _contractorService.ContractorRateDetails(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ContractorRateDetails_VerifiesQueryAndParameters()
        {
            // Arrange
            var model = new ContractorRatesDetailSearchViewModel
            {
                ContractorRatesID = 1,
                ACCTGCode = "ACC123"
            };

            // Act
            await _contractorService.ContractorRateDetails(model);

            // Assert
            _mockContext.Verify(db => db.ExecuteQueryAsync<ContractorRatesDetails>(
                It.Is<string>(q => q.Contains("and crd.ContractorRatesID = @ContractorRatesID") &&
                                   q.Contains("and crd.ACCTGCode = @ACCTGCode")),
                It.Is<DynamicParameters>(p =>
                    p.Get<int>(DbParams.ContractorRatesID) == 1 &&
                    p.Get<string>(DbParams.ACCTGCode) == "ACC123"),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorVehicle_ReturnsData_WhenAllParametersProvided()
        {
            // Arrange
            var mockResult = new List<vwContractorVehicleSearch>
            {
                new vwContractorVehicleSearch { ContractorID = 1, IsPrimary = 1 }
            };

            _mockContext.Setup(db => db.ExecuteQueryAsync<vwContractorVehicleSearch>(
                It.Is<string>(q => q.Contains("SELECT * FROM vwContractorVehicleSearch where ContractorID=@ContractorID") &&
                                   q.Contains("and IsPrimary = @IsPrimary")),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(mockResult);

            var model = new ContractorVehicleSearchWebViewModel
            {
                ContractorID = 1,
                IsPrimary = 1
            };

            // Act
            var result = await _contractorService.ContractorVehicle(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result.First().ContractorID);
            Assert.Equal(1, result.First().IsPrimary);
        }

        [Fact]
        public async Task ContractorVehicle_ReturnsData_WhenOnlyContractorIDProvided()
        {
            // Arrange
            var mockResult = new List<vwContractorVehicleSearch>
            {
                new vwContractorVehicleSearch { ContractorID = 1, IsPrimary = 0 }
            };

            _mockContext.Setup(db => db.ExecuteQueryAsync<vwContractorVehicleSearch>(
                It.Is<string>(q => q.Contains("SELECT * FROM vwContractorVehicleSearch where ContractorID=@ContractorID") &&
                                   !q.Contains("and IsPrimary = @IsPrimary")),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(mockResult);

            var model = new ContractorVehicleSearchWebViewModel
            {
                ContractorID = 1
            };

            // Act
            var result = await _contractorService.ContractorVehicle(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result.First().ContractorID);
        }

        [Fact]
        public async Task ContractorVehicle_ReturnsEmpty_WhenInvalidContractorIDProvided()
        {
            // Arrange
            var mockResult = new List<vwContractorVehicleSearch>(); // No results

            _mockContext.Setup(db => db.ExecuteQueryAsync<vwContractorVehicleSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(mockResult);

            var model = new ContractorVehicleSearchWebViewModel
            {
                ContractorID = 9999, // Invalid ID
                IsPrimary = 1
            };

            // Act
            var result = await _contractorService.ContractorVehicle(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ContractorVehicle_VerifiesQueryAndParameters()
        {
            // Arrange
            var model = new ContractorVehicleSearchWebViewModel
            {
                ContractorID = 1,
                IsPrimary = 1
            };

            // Act
            await _contractorService.ContractorVehicle(model);

            // Assert
            _mockContext.Verify(db => db.ExecuteQueryAsync<vwContractorVehicleSearch>(
                It.Is<string>(q => q.Contains("SELECT * FROM vwContractorVehicleSearch where ContractorID=@ContractorID") &&
                                   q.Contains("and IsPrimary = @IsPrimary")),
                It.Is<DynamicParameters>(p =>
                    p.Get<int>(DbParams.ContractorID) == 1 &&
                    p.Get<int>(DbParams.IsPrimary) == 1),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ContractorVehicle_ReturnsEmpty_WhenIsPrimaryDoesNotMatch()
        {
            // Arrange
            var mockResult = new List<vwContractorVehicleSearch>(); // No results

            _mockContext.Setup(db => db.ExecuteQueryAsync<vwContractorVehicleSearch>(
                It.IsAny<string>(),
                It.IsAny<DynamicParameters>(),
                CommandType.Text))
            .ReturnsAsync(mockResult);

            var model = new ContractorVehicleSearchWebViewModel
            {
                ContractorID = 1,
                IsPrimary = 99 // Invalid value
            };

            // Act
            var result = await _contractorService.ContractorVehicle(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetContractorMediaAsync_ReturnsEmpty_WhenContractorNotFound()
        {
            // Arrange
            var mockResult = new ContractorMediaResponseModel(); // Simulate no contractor media found

            _mockContext.Setup(db => db.ExecuteQueryFirstOrDefaultAsync<ContractorMediaResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(mockResult);

            int contractorId = 9999; // Invalid contractor ID

            // Act
            var result = await _contractorService.GetContractorMediaAsync(contractorId);

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.IsType<ContractorMediaResponseModel>(result); // Ensure it's of expected type
            Assert.True(result.ContractorId == 0 && result.ContractorProfileImage == "");
        }

        [Fact]
        public async Task GetContractorMediaAsync_ReturnsContractorMedia_WhenContractorFound()
        {
            // Arrange
            var mockResult = new ContractorMediaResponseModel
            {
                ContractorProfileImage = "http://image.url", // Simulate valid contractor media data

            };

            _mockContext.Setup(db => db.ExecuteQueryFirstOrDefaultAsync<ContractorMediaResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(mockResult);

            int contractorId = 2; // Sample contractor ID

            // Act
            var result = await _contractorService.GetContractorMediaAsync(contractorId);

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.IsType<ContractorMediaResponseModel>(result); // Ensure it's of expected type
            Assert.Equal(mockResult.ContractorProfileImage, result.ContractorProfileImage); // Assert the ProfileImageUrl is correct

        }

        [Fact]
        public async Task GetContractorMediaAsync_ReturnsEmpty_WhenInvalidContractorId()
        {
            // Arrange
            var mockResult = new ContractorMediaResponseModel(); // Simulate no contractor media found

            _mockContext.Setup(db => db.ExecuteQueryFirstOrDefaultAsync<ContractorMediaResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(mockResult);

            int contractorId = 9999; // Invalid contractor ID

            // Act
            var result = await _contractorService.GetContractorMediaAsync(contractorId);

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.IsType<ContractorMediaResponseModel>(result); // Ensure it's of expected type
            Assert.Equal(0, result.ContractorId); // Assert the ContractorId is 0
            Assert.Equal("", result.ContractorProfileImage); // Assert the ProfileImage is empty string
        }

        [Fact]
        public async Task UpsertContractorMedia_ThrowsException_WhenDatabaseFails()
        {
            // Arrange
            var model = new ContractorMediaViewModel
            {
                ContractorID = 1,
                ProfileImageBase64 = "validBase64String",
                ProfileImageUrl = "http://image.url"
            };

            // Simulate a database failure by throwing an exception
            _mockContext.Setup(db => db.ExecuteScalerAsync(
                    ProcEntities.spUpdateContractorMedia,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _contractorService.UpsertContractorMedia(model));
            Assert.Equal("Database connection failed", exception.Message);  // Ensure the exception message matches
        }


    }
}
