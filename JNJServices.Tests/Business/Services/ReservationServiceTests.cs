using Dapper;
using JNJServices.Business.Services;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.DbConstants;
using Moq;
using System.Data;

namespace JNJServices.Tests.Business.Services
{
    public class ReservationServiceTests
    {
        private readonly Mock<IDapperContext> _mockDapperContext;
        private readonly ReservationService _reservationService;
        public ReservationServiceTests()
        {
            _mockDapperContext = new Mock<IDapperContext>();

            // Arrange - Set up any necessary method setups for the mock (if needed)
            _mockDapperContext.Setup(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                              .ReturnsAsync(1); // Mocking ExecuteAsync to return a successful execution (1)

            // Initialize UserService with the mocked IDapperContext
            _reservationService = new ReservationService(_mockDapperContext.Object);
        }

        [Fact]
        public async Task MobileReservationSearch_ShouldAddFilters_WhenAllPropertiesAreProvided()
        {
            // Arrange
            var model = new ReservationSearchViewModel
            {
                reservationid = 123,
                ActionCode = "ABC",
                IsTodayJobNotStarted = 1,
                ClaimantID = 456,
                DateFrom = "2025-01-01",
                DateTo = "2025-01-31",
                ReservationTimeFrom = DateTime.Parse("2025-01-01T08:00:00"),
                ReservationTimeTo = DateTime.Parse("2025-01-01T17:00:00"),
                ReservationTextSearch = "Sample",
                ClaimantConfirmation = "Confirmed",
                IsJobComplete = 1,
                inactiveflag = 0,
                Page = 1,
                Limit = 10,
                Type = 2
            };

            // Act
            var result = await _reservationService.MobileReservationSearch(model);

            // Assert
            Assert.NotNull(result);

            // Verify that the stored procedure is called once
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
                It.Is<string>(query => query == ProcEntities.spMobileReservationSearch),
                It.Is<DynamicParameters>(parameters =>
                    parameters.Get<int>(DbParams.ReservationID) == 123 &&
                    parameters.Get<string>(DbParams.RSVACCode) == "ABC" &&
                    parameters.Get<int>(DbParams.IsTodayJobNotStarted) == 1 &&
                    parameters.Get<int>(DbParams.ClaimantID) == 456 &&
                    parameters.Get<DateTime>(DbParams.DateFrom) == DateTime.Parse("2025-01-01").Date &&
                    parameters.Get<DateTime>(DbParams.DateTo) == DateTime.Parse("2025-01-31").Date &&
                    parameters.Get<DateTime>(DbParams.ReservationTimeFrom) == DateTime.Parse("2025-01-01T08:00:00") &&
                    parameters.Get<DateTime>(DbParams.ReservationTimeTo) == DateTime.Parse("2025-01-01T17:00:00") &&
                    parameters.Get<string>(DbParams.ReservationTextSearch) == "Sample" &&
                    parameters.Get<string>(DbParams.ClaimantConfirmation) == "Confirmed" &&
                    parameters.Get<int>(DbParams.IsJobComplete) == 1 &&
                    parameters.Get<int>(DbParams.InactiveFlag) == 0 &&
                    parameters.Get<int>(DbParams.Page) == 1 &&
                    parameters.Get<int>(DbParams.Limit) == 10 &&
                    parameters.Get<int>(DbParams.Type) == 2),
                CommandType.StoredProcedure), Times.Once);
        }

        //[Fact]
        //public async Task MobileReservationSearch_ShouldUseDefaultPageAndLimit_WhenInvalidValuesProvided()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        Page = 0,  // Invalid page number
        //        Limit = 0  // Invalid limit
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Empty(result);  // Because we mocked ExecuteQueryAsync to return an empty list
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);

        //    // Additional asserts can be added to check if default values were applied
        //}

        //[Fact]
        //public async Task MobileReservationSearch_ShouldBuildCorrectQuery_WhenReservationIdIsProvided()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        reservationid = 123
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("and ReservationID = @ReservationID")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationSearch_ShouldAddDateRange_WhenDateFromAndDateToAreProvided()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        DateFrom = "2023-01-01",
        //        DateTo = "2023-01-31"
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("and (ReservationDate Between @DateFrom and @DateTo)")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationSearch_ShouldAddActionCodeFilter_WhenActionCodeIsProvided()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        ActionCode = "ABC"
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("and RSVACCode = @RSVACCode")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        [Fact]
        public async Task ReservationActionCode_ShouldReturnListOfReservationActionCodes_WhenDataExists()
        {
            // Arrange
            var expectedActionCodes = new List<ReservationActionCode>
            {
                new ReservationActionCode {  code = "A1", description = "Action Code 1"},
                new ReservationActionCode { code = "A2", description = "Action Code 2" }
            };

            // Mock ExecuteQueryAsync to return the expected list of action codes
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<ReservationActionCode>(
                It.IsAny<string>(), CommandType.Text))
                .ReturnsAsync(expectedActionCodes);

            // Act
            var result = await _reservationService.ReservationActionCode();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedActionCodes.Count, result.Count());
            Assert.Equal(expectedActionCodes, result);
        }

        //[Fact]
        //public async Task MobileReservationSearch_ShouldAddIsTodayJobNotStarted()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        IsTodayJobNotStarted = 1
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("ReservationID in (Select ReservationID From vwReservationAssignSearch")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationSearch_ShouldAddClaimantID()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        ClaimantID = 1
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("Claimantid = @ClaimantID")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationSearch_ShouldAddDateToFilter()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        DateTo = "22/03/2024",
        //        Page = 1,
        //        Limit = 10
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("ReservationDate <= @DateTo")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationSearch_ShouldAddDateTo()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        DateFrom = "22/03/2022",
        //        Page = 1,
        //        Limit = 10
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("ReservationDate >= @DateFrom")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationSearch_ShouldAddReservationTimeFromTo()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        ReservationTimeFrom = DateTime.Now,
        //        ReservationTimeTo = DateTime.Now,
        //        Page = 1,
        //        Limit = 10
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("Cast(ReservationTime  as Time(7)) Between Cast(@ReservationTimeFrom  as Time(7))")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationSearch_ShoulReservationTimeFrom()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        ReservationTimeFrom = DateTime.Now
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("Cast(ReservationTime  as Time(7)) >= Cast(@ReservationTimeFrom  as Time(7))")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationSearch_ShouldAddReservationTimeTo()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        ReservationTimeTo = DateTime.Now
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("Cast(ReservationTime  as Time(7)) <= Cast(@ReservationTimeTo  as Time(7))")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationSearch_ShouldAddReservationTextSearch()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        ReservationTextSearch = "abc"
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("Cast(ReservationID as varchar(50))  like '%'+@ReservationTextSearch+'%'")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationSearch_ShouldAddClaimantConfirmation()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        ClaimantConfirmation = "abc"
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("@ClaimantConfirmation LIKE '%'+ ClaimantConfirmation +'%'")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationSearch_ShouldAddIsJobComplete()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        Type = 2,
        //        IsJobComplete = 2
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("ReservationID in (Select Distinct ras.ReservationID From vwReservationAssignSearch")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationSearch_ShouldAddinactiveflag()
        //{
        //    // Arrange
        //    var model = new ReservationSearchViewModel
        //    {
        //        inactiveflag = 0
        //    };

        //    // Act
        //    var result = await _reservationService.MobileReservationSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationAppResponseModel>(
        //        It.Is<string>(query => query.Contains("inactiveflag = @inactiveflag")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        [Fact]
        public async Task ReservationActionCode_ShouldReturnEmptyList_WhenNoDataExists()
        {
            // Arrange
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<ReservationActionCode>(
                It.IsAny<string>(), CommandType.Text))
                .ReturnsAsync(new List<ReservationActionCode>()); // No data returned

            // Act
            var result = await _reservationService.ReservationActionCode();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Ensure the result is an empty list
        }

        [Fact]
        public async Task ReservationActionCode_ShouldExecuteCorrectQuery()
        {
            // Arrange
            var expectedQuery = "Select * From codesRSVAC where inactiveflag = 0 and actype='R' order by code";

            // Act
            await _reservationService.ReservationActionCode();

            // Assert
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationActionCode>(
                It.Is<string>(query => query == expectedQuery),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ReservationActionCode_ShouldHandleCommandTypeText()
        {
            // Arrange
            var expectedCommandType = CommandType.Text;

            // Act
            await _reservationService.ReservationActionCode();

            // Assert
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationActionCode>(
                It.IsAny<string>(),
                It.Is<CommandType>(cmdType => cmdType == expectedCommandType)), Times.Once);
        }

        [Fact]
        public async Task ReservationServices_ShouldReturnListOfReservationServices_WhenDataExists()
        {
            // Arrange
            var expectedServices = new List<ReservationsServices>
            {
                new ReservationsServices { code = "S1", description = "Service 1" },
                new ReservationsServices { code = "S2", description = "Service 2" }
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<ReservationsServices>(
                It.IsAny<string>(), CommandType.Text))
                .ReturnsAsync(expectedServices);

            // Act
            var result = await _reservationService.ReservationServices();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedServices.Count, result.Count());
            Assert.Equal(expectedServices, result);
        }

        [Fact]
        public async Task ReservationServices_ShouldReturnEmptyList_WhenNoDataExists()
        {
            // Arrange
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<ReservationsServices>(
                It.IsAny<string>(), CommandType.Text))
                .ReturnsAsync(new List<ReservationsServices>()); // No data returned

            // Act
            var result = await _reservationService.ReservationServices();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Ensure the result is an empty list
        }

        [Fact]
        public async Task ReservationServices_ShouldExecuteCorrectQuery()
        {
            // Arrange
            var expectedQuery = "Select * From codesRSVSV where inactiveflag = 0 order by description";

            // Act
            await _reservationService.ReservationServices();

            // Assert
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationsServices>(
                It.Is<string>(query => query == expectedQuery),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ReservationServices_ShouldHandleCommandTypeText()
        {
            // Arrange
            var expectedCommandType = CommandType.Text;

            // Act
            await _reservationService.ReservationServices();

            // Assert
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationsServices>(
                It.IsAny<string>(),
                It.Is<CommandType>(cmdType => cmdType == expectedCommandType)), Times.Once);
        }

        [Fact]
        public async Task ReservationTripType_ShouldReturnListOfTripTypes_WhenDataExists()
        {
            // Arrange
            var expectedTripTypes = new List<ReservationTripType>
            {
                new ReservationTripType { code = "T1", description = "Trip Type 1" },
                new ReservationTripType { code = "T2", description = "Trip Type 2" }
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<ReservationTripType>(
                It.IsAny<string>(), CommandType.Text))
                .ReturnsAsync(expectedTripTypes);

            // Act
            var result = await _reservationService.ReservationTripType();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTripTypes.Count, result.Count());
            Assert.Equal(expectedTripTypes, result);
        }

        [Fact]
        public async Task ReservationTripType_ShouldReturnEmptyList_WhenNoDataExists()
        {
            // Arrange
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<ReservationTripType>(
                It.IsAny<string>(), CommandType.Text))
                .ReturnsAsync(new List<ReservationTripType>()); // No data returned

            // Act
            var result = await _reservationService.ReservationTripType();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Ensure the result is an empty list
        }

        [Fact]
        public async Task ReservationTripType_ShouldExecuteCorrectQuery()
        {
            // Arrange
            var expectedQuery = "Select * From codesRSVTT where inactiveflag = 0 order by description";

            // Act
            await _reservationService.ReservationTripType();

            // Assert
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationTripType>(
                It.Is<string>(query => query == expectedQuery),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ReservationTripType_ShouldHandleCommandTypeText()
        {
            // Arrange
            var expectedCommandType = CommandType.Text;

            // Act
            await _reservationService.ReservationTripType();

            // Assert
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationTripType>(
                It.IsAny<string>(),
                It.Is<CommandType>(cmdType => cmdType == expectedCommandType)), Times.Once);
        }

        [Fact]
        public async Task ReservationTransportType_ShouldReturnListOfTransportTypes_WhenDataExists()
        {
            // Arrange
            var expectedTransportTypes = new List<ReservationTransportType>
            {
                new ReservationTransportType { code = "T1", description = "Transport Type 1" } ,
                new ReservationTransportType { code = "T2", description = "Transport Type 2" }
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<ReservationTransportType>(
                It.IsAny<string>(), CommandType.Text))
                .ReturnsAsync(expectedTransportTypes);

            // Act
            var result = await _reservationService.ReservationTransportType();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTransportTypes.Count, result.Count());
            Assert.Equal(expectedTransportTypes, result);
        }

        [Fact]
        public async Task ReservationTransportType_ShouldReturnEmptyList_WhenNoDataExists()
        {
            // Arrange
            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<ReservationTransportType>(
                It.IsAny<string>(), CommandType.Text))
                .ReturnsAsync(new List<ReservationTransportType>()); // No data returned

            // Act
            var result = await _reservationService.ReservationTransportType();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Ensure the result is an empty list
        }

        [Fact]
        public async Task ReservationTransportType_ShouldExecuteCorrectQuery()
        {
            // Arrange
            var expectedQuery = "Select * From codesTRNTY where inactiveflag = 0 order by description";

            // Act
            await _reservationService.ReservationTransportType();

            // Assert
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationTransportType>(
                It.Is<string>(query => query == expectedQuery),
                CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task ReservationTransportType_ShouldHandleCommandTypeText()
        {
            // Arrange
            var expectedCommandType = CommandType.Text;

            // Act
            await _reservationService.ReservationTransportType();

            // Assert
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationTransportType>(
                It.IsAny<string>(),
                It.Is<CommandType>(cmdType => cmdType == expectedCommandType)), Times.Once);
        }

        [Fact]
        public async Task ReservationSearch_ShouldReturnListOfReservations_WhenDataExists()
        {
            // Arrange
            var model = new ReservationSearchWebViewModel
            {
                reservationid = 123,
                claimnumber = "CLM001",
                ClaimantID = 123,
                Contractorid = 123,
                Service = "abc",
                inactiveflag = -1,
                TransportType = "abc",
                TripType = "abc",
                ClaimantConfirmation = "abc",
                ConAssignStatus = 0,
                Page = 1,
                Limit = 10
            };

            var expectedResponse = new List<ReservationSearchResponseModel>
            {
                new ReservationSearchResponseModel {  reservationid = 123, claimnumber = "CLM001" }
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<ReservationSearchResponseModel>(
                It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _reservationService.ReservationSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task ReservationSearch_ShouldReturnEmptyList_WhenNoDataExists()
        {
            // Arrange
            var model = new ReservationSearchWebViewModel
            {
                reservationid = 9999,
                Page = 1,
                Limit = 10
            };

            _mockDapperContext.Setup(db => db.ExecuteQueryAsync<ReservationSearchResponseModel>(
                It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure))
                .ReturnsAsync(new List<ReservationSearchResponseModel>());

            // Act
            var result = await _reservationService.ReservationSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ReservationSearch_ShouldSetCorrectStoredProcedureName()
        {
            // Arrange
            var model = new ReservationSearchWebViewModel { Page = 1, Limit = 10 };
            var expectedProcedureName = ProcEntities.spReservationSearch;

            // Act
            await _reservationService.ReservationSearch(model);

            // Assert
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationSearchResponseModel>(
                It.Is<string>(name => name == expectedProcedureName),
                It.IsAny<DynamicParameters>(), CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task ReservationSearch_ShouldAddParametersCorrectly_BasedOnModel()
        {
            // Arrange
            var model = new ReservationSearchWebViewModel
            {
                reservationid = 123,
                claimnumber = "CLM001",
                ClaimID = 456,
                ActionCode = "A1",
                CustomerID = 789,
                DateFrom = "2023-01-01",
                DateTo = "2023-12-31",
                Page = 1,
                Limit = 10
            };

            // Act
            await _reservationService.ReservationSearch(model);

            // Assert
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationSearchResponseModel>(
                It.IsAny<string>(),
                It.Is<DynamicParameters>(p =>
                    p.Get<int>(DbParams.ReservationID) == model.reservationid &&
                    p.Get<string>(DbParams.ClaimNumber) == model.claimnumber &&
                    p.Get<int>(DbParams.ClaimID) == model.ClaimID &&
                    p.Get<string>(DbParams.ActionCode) == model.ActionCode &&
                    p.Get<int>(DbParams.CustomerID) == model.CustomerID &&
                    p.Get<DateTime>(DbParams.DateFrom) == Convert.ToDateTime(model.DateFrom) &&
                    p.Get<DateTime>(DbParams.DateTo) == Convert.ToDateTime(model.DateTo) &&
                    p.Get<int>(DbParams.Page) == model.Page &&
                    p.Get<int>(DbParams.Limit) == model.Limit),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task ReservationSearch_ShouldHandleNullOrEmptyOptionalParameters()
        {
            // Arrange
            var model = new ReservationSearchWebViewModel
            {
                reservationid = 0,  // Intentionally setting to 0 to test validation method
                claimnumber = null, // Testing null values for optional parameters
                ClaimID = 0,
                ActionCode = null,
                Page = 1,
                Limit = 10
            };

            // Act
            await _reservationService.ReservationSearch(model);

            // Assert
            _mockDapperContext.Verify(db => db.ExecuteQueryAsync<ReservationSearchResponseModel>(
                It.IsAny<string>(),
                It.Is<DynamicParameters>(p =>
                    !p.ParameterNames.Contains(DbParams.ReservationID) &&
                    !p.ParameterNames.Contains(DbParams.ClaimNumber) &&
                    !p.ParameterNames.Contains(DbParams.ClaimID) &&
                    !p.ParameterNames.Contains(DbParams.ActionCode)),
                CommandType.StoredProcedure), Times.Once);
        }


    }
}
