using Dapper;
using JNJServices.Business.Services;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.DbConstants;
using Moq;
using System.Data;

namespace JNJServices.Tests.Business.Services
{
    public class ReservationAssignmentsServiceTests
    {
        private readonly Mock<IDapperContext> _contextMock;
        private readonly ReservationAssignmentsService _service;

        public ReservationAssignmentsServiceTests()
        {
            // Arrange
            _contextMock = new Mock<IDapperContext>(); // Mock the IDapperContext
            _service = new ReservationAssignmentsService(_contextMock.Object); // Create the service instance
        }

        [Fact]
        public async Task ReservationAssignmentType_ShouldReturnAssignments_WhenQuerySucceeds()
        {
            // Arrange
            var expectedAssignments = new List<AssignmentService>
        {
            new AssignmentService { code = "abc", description = "Assignment 1" },
            new AssignmentService { code = "def", description = "Assignment 2" }
        };

            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentService>(It.IsAny<string>(), It.IsAny<CommandType>()))
                .ReturnsAsync(expectedAssignments);

            // Act
            var result = await _service.ReservationAssignmentType();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedAssignments.Count, result.Count());
            //Assert.Equal(expectedAssignments[0].description, result[0].description);
        }

        [Fact]
        public async Task ReservationAssignmentType_ShouldReturnEmptyList_WhenNoResults()
        {
            // Arrange
            var expectedAssignments = new List<AssignmentService>();

            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentService>(It.IsAny<string>(), It.IsAny<CommandType>()))
                .ReturnsAsync(expectedAssignments);

            // Act
            var result = await _service.ReservationAssignmentType();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ReservationAssignmentType_ShouldThrowException_WhenQueryFails()
        {
            // Arrange
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentService>(It.IsAny<string>(), It.IsAny<CommandType>()))
                .ThrowsAsync(new System.Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.Exception>(() => _service.ReservationAssignmentType());
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task MobileReservationAssignmentSearch_ShouldApplyAllFilters()
        {
            // Arrange
            var model = new AppReservationAssignmentSearchViewModel
            {
                ContractorID = 101,
                ReservationsAssignmentsID = 456,
                ReservationID = 123,
                DateFrom = "2024-01-01",
                DateTo = "2024-01-31",
                ReservationTimeFrom = DateTime.Now.AddHours(-1),
                ReservationTimeTo = DateTime.Now.AddHours(1),
                AssignmentTextSearch = "rescancel",
                ActionCode = "ACT01",
                ClaimantID = 789,
                IsContractorMetricsSubmitted = 1,
                IsTodayJobNotStarted = 1,
                DriverConfirmFlag = 0,
                IsJobComplete = 2,
                IsPastJobComplete = 2,
                inactiveflag = 0,
                Page = 1,
                Limit = 10
            };

            var expectedResult = new List<ReservationAssignmentAppResponseModel>
    {
        new ReservationAssignmentAppResponseModel
        {
            ReservationsAssignmentsID = 456,
            ReservationDate = DateTime.Parse("2024-01-15"),
            ReservationTime = DateTime.Now.AddMinutes(-30),
            contractorid = 101,
            claimantid = 789,
            ActionCode = "ACT01"
        }
    };

            _contextMock
                .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                    "spMobileReservationAssignmentSearch",
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.MobileReservationAssignmentSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expectedResult.First().ReservationsAssignmentsID, result.First().ReservationsAssignmentsID);
            Assert.Equal(expectedResult.First().ReservationDate, result.First().ReservationDate);
            Assert.Equal(expectedResult.First().ReservationTime, result.First().ReservationTime);
        }

        //[Fact]
        //public async Task MobileReservationAssignmentSearch_ShouldFilterByReservationsAssignmentsID()
        //{
        //    // Arrange
        //    var model = new AppReservationAssignmentSearchViewModel
        //    {
        //        ReservationsAssignmentsID = 202,
        //        Page = 1,
        //        Limit = 5
        //    };
        //    var expectedResult = new List<ReservationAssignmentAppResponseModel>();
        //    _contextMock
        //        .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
        //        .ReturnsAsync(expectedResult);

        //    // Act
        //    var result = await _service.MobileReservationAssignmentSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //        It.Is<string>(q => q.Contains("ReservationsAssignmentsID = @ReservationsAssignmentsID")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        [Fact]
        public async Task MobileReservationAssignmentSearch_ShouldFilterByDateRange()
        {
            // Arrange
            var model = new AppReservationAssignmentSearchViewModel
            {
                DateFrom = "2024-01-01",
                DateTo = "2024-01-31",
                Page = 1,
                Limit = 10
            };

            var expectedResult = new List<ReservationAssignmentAppResponseModel>
    {
        new ReservationAssignmentAppResponseModel { ReservationDate = DateTime.Parse("2024-01-15") }
    };

            _contextMock
                .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                    It.Is<string>(q => q == "spMobileReservationAssignmentSearch"),
                    It.Is<DynamicParameters>(p =>
                        p.Get<DateTime>(DbParams.DateFrom) == DateTime.Parse(model.DateFrom) &&
                        p.Get<DateTime>(DbParams.DateTo) == DateTime.Parse(model.DateTo)),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.MobileReservationAssignmentSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expectedResult.First().ReservationDate, result.First().ReservationDate); // Verify the ReservationDate
            _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                It.Is<string>(q => q == "spMobileReservationAssignmentSearch"), // Verify the stored procedure name
                It.Is<DynamicParameters>(p =>
                    p.Get<DateTime>(DbParams.DateFrom) == DateTime.Parse(model.DateFrom) &&
                    p.Get<DateTime>(DbParams.DateTo) == DateTime.Parse(model.DateTo)), // Verify the parameters
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task MobileReservationAssignmentSearch_ShouldExecuteStoredProcedureForPendingFlag()
        {
            // Arrange
            var model = new AppReservationAssignmentSearchViewModel
            {
                IsContractorPendingflag = 1,
                ContractorID = 101
            };
            var expectedResult = new List<ReservationAssignmentAppResponseModel>();
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.MobileReservationAssignmentSearch(model);

            // Assert
            Assert.NotNull(result);
            _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                It.Is<string>(q => q == ProcEntities.spGetAllContractorJobRequest),
                It.IsAny<DynamicParameters>(), CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task MobileReservationAssignmentSearch_ShouldApplyPagination()
        {
            // Arrange
            var model = new AppReservationAssignmentSearchViewModel
            {
                Page = 2,
                Limit = 10
            };
            var expectedResult = new List<ReservationAssignmentAppResponseModel>();
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.MobileReservationAssignmentSearch(model);

            // Assert
            Assert.NotNull(result);
            _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                It.Is<string>(q => q == "spMobileReservationAssignmentSearch"), // Verify the stored procedure name
                It.Is<DynamicParameters>(p =>
                    p.Get<int>("@Page") == model.Page &&
                    p.Get<int>("@Limit") == model.Limit), // Verify the parameters
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task MobileReservationAssignmentSearch_ShouldFilterByReservationID()
        {
            // Arrange
            var model = new AppReservationAssignmentSearchViewModel
            {
                ReservationID = 123,
                Page = 1,
                Limit = 10
            };
            var expectedResult = new List<ReservationAssignmentAppResponseModel> { new ReservationAssignmentAppResponseModel() };

            _contextMock
                .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.MobileReservationAssignmentSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                It.Is<string>(q => q == "spMobileReservationAssignmentSearch"), // Verify the stored procedure name
                It.Is<DynamicParameters>(p =>
                    p.Get<int>("@ReservationID") == model.ReservationID &&
                    p.Get<int>("@Page") == model.Page &&
                    p.Get<int>("@Limit") == model.Limit), // Verify the parameters
                CommandType.StoredProcedure), Times.Once);
        }


        [Fact]
        public async Task MobileReservationAssignmentSearch_ShouldFilterByDateFrom()
        {
            // Arrange
            var model = new AppReservationAssignmentSearchViewModel
            {
                DateFrom = "2024-11-22", // Corrected date format
                Page = 1,
                Limit = 10
            };

            var expectedResult = new List<ReservationAssignmentAppResponseModel> { new ReservationAssignmentAppResponseModel() };

            _contextMock
                .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.MobileReservationAssignmentSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                It.Is<string>(q => q == "spMobileReservationAssignmentSearch"), // Verify the stored procedure name
                It.Is<DynamicParameters>(p =>
                    p.Get<DateTime>("@DateFrom") == DateTime.Parse(model.DateFrom) && // Ensure DateFrom is passed as DateTime
                    p.Get<int>("@Page") == model.Page &&
                    p.Get<int>("@Limit") == model.Limit), // Verify the parameters
                CommandType.StoredProcedure), Times.Once);
        }


        [Fact]
        public async Task MobileReservationAssignmentSearch_ShouldFilterByDateTo()
        {
            // Arrange
            var model = new AppReservationAssignmentSearchViewModel
            {
                DateTo = "2024-11-21", // Ensure the date format matches the expected format in the query
                Page = 1,
                Limit = 10
            };

            var expectedResult = new List<ReservationAssignmentAppResponseModel>
       {
           new ReservationAssignmentAppResponseModel { ReservationDate = DateTime.Parse("2024-11-20") }
       };

            _contextMock
                .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                    It.Is<string>(q => q == "spMobileReservationAssignmentSearch"),
                    It.Is<DynamicParameters>(p => p.Get<DateTime>(DbParams.DateTo) == DateTime.Parse(model.DateTo)),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.MobileReservationAssignmentSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(DateTime.Parse("2024-11-20"), result.First().ReservationDate); // Verify the ReservationDate
            _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                It.Is<string>(q => q == "spMobileReservationAssignmentSearch"), // Verify the stored procedure name
                It.Is<DynamicParameters>(p => p.Get<DateTime>(DbParams.DateTo) == DateTime.Parse(model.DateTo)), // Verify the parameter
                CommandType.StoredProcedure), Times.Once);
        }


        [Fact]
        public async Task MobileReservationAssignmentSearch_ShouldFilterByReservationTimeTo()
        {
            // Arrange
            var model = new AppReservationAssignmentSearchViewModel
            {
                ReservationTimeTo = DateTime.Now,
                Page = 1,
                Limit = 10
            };

            var expectedResult = new List<ReservationAssignmentAppResponseModel>
    {
        new ReservationAssignmentAppResponseModel { ReservationTime = DateTime.Now.AddMinutes(-30) }
    };

            _contextMock
                .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                    It.Is<string>(q => q == "spMobileReservationAssignmentSearch"),
                    It.Is<DynamicParameters>(p => p.Get<DateTime>(DbParams.ReservationTimeTo) == model.ReservationTimeTo),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.MobileReservationAssignmentSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expectedResult.First().ReservationTime, result.First().ReservationTime); // Verify the ReservationTime
            _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                It.Is<string>(q => q == "spMobileReservationAssignmentSearch"), // Verify the stored procedure name
                It.Is<DynamicParameters>(p => p.Get<DateTime>(DbParams.ReservationTimeTo) == model.ReservationTimeTo), // Verify the parameter
                CommandType.StoredProcedure), Times.Once);
        }

        //[Fact]
        //public async Task MobileReservationAssignmentSearch_ShouldFilterByReservationTimeFrom()
        //{
        //    // Arrange
        //    var model = new AppReservationAssignmentSearchViewModel
        //    {
        //        ReservationTimeFrom = DateTime.Now,
        //        Page = 1,
        //        Limit = 10
        //    };

        //    var expectedResult = new List<ReservationAssignmentAppResponseModel> { new ReservationAssignmentAppResponseModel() };

        //    _contextMock
        //        .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
        //        .ReturnsAsync(expectedResult);

        //    // Act
        //    var result = await _service.MobileReservationAssignmentSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Single(result);
        //    _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //        It.Is<string>(q => q.Contains("Cast(ReservationTime  as Time(7)) >= Cast(@ReservationTimeFrom  as Time(7))")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);

        //}

        [Fact]
        public async Task MobileReservationAssignmentSearch_ShouldFilterByAssignmentTextSearch()
        {
            // Arrange
            var model = new AppReservationAssignmentSearchViewModel
            {
                AssignmentTextSearch = "rescancel",
                Page = 1,
                Limit = 10
            };

            var expectedResult = new List<ReservationAssignmentAppResponseModel> { new ReservationAssignmentAppResponseModel() };

            _contextMock
                .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                    It.Is<string>(q => q == "spMobileReservationAssignmentSearch"),
                    It.Is<DynamicParameters>(p => p.Get<string>("@AssignmentTextSearch") == model.AssignmentTextSearch),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.MobileReservationAssignmentSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                It.Is<string>(q => q == "spMobileReservationAssignmentSearch"), // Verify the stored procedure name
                It.Is<DynamicParameters>(p => p.Get<string>("@AssignmentTextSearch") == model.AssignmentTextSearch), // Verify the parameter
                CommandType.StoredProcedure), Times.Once);
        }

        //    [Fact]
        //    public async Task MobileReservationAssignmentSearch_ShouldFilterByActionCodeIfCondition()
        //    {
        //        // Arrange
        //        var model = new AppReservationAssignmentSearchViewModel
        //        {
        //            ActionCode = "rescancel",
        //            Page = 1,
        //            Limit = 10
        //        };

        //        var expectedResult = new List<ReservationAssignmentAppResponseModel>
        //{
        //    new ReservationAssignmentAppResponseModel { ActionCode = "rescancel" }
        //};

        //        _contextMock
        //            .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //                It.Is<string>(q => q == "spMobileReservationAssignmentSearch"),
        //                It.IsAny<DynamicParameters>(), // Loose matching here
        //                CommandType.Text))
        //            .ReturnsAsync(expectedResult);

        //        // Act
        //        var result = await _service.MobileReservationAssignmentSearch(model);

        //        // Assert
        //        Assert.NotNull(result);
        //        Assert.Single(result);
        //        Assert.Equal("rescancel", result.First().ActionCode);

        //        // Additional assert to verify parameters
        //        _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //            It.Is<string>(q => q == "spMobileReservationAssignmentSearch"),
        //            It.Is<DynamicParameters>(p => p.ParameterNames.Contains("@RSVACCode") &&
        //                                          p.Get<string>("@RSVACCode") == "rescancel"),
        //            CommandType.Text), Times.Once);
        //    }



        //[Fact]
        //public async Task MobileReservationAssignmentSearch_ShouldFilterByClaimantID()
        //{
        //    // Arrange
        //    var model = new AppReservationAssignmentSearchViewModel
        //    {
        //        ClaimantID = 123,
        //        Page = 1,
        //        Limit = 10
        //    };
        //    var expectedResult = new List<ReservationAssignmentAppResponseModel> { new ReservationAssignmentAppResponseModel() };
        //    _contextMock
        //        .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
        //        .ReturnsAsync(expectedResult);

        //    // Act
        //    var result = await _service.MobileReservationAssignmentSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Single(result);
        //    _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //        It.Is<string>(q => q.Contains("claimantid = @claimantid")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);

        //}

        //[Fact]
        //public async Task MobileReservationAssignmentSearch_ShouldFilterByIsContractorMetricsSubmitted()
        //{
        //    // Arrange
        //    var model = new AppReservationAssignmentSearchViewModel
        //    {
        //        IsContractorMetricsSubmitted = 1,
        //        Page = 1,
        //        Limit = 10
        //    };
        //    var expectedResult = new List<ReservationAssignmentAppResponseModel> { new ReservationAssignmentAppResponseModel() };
        //    _contextMock
        //        .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
        //        .ReturnsAsync(expectedResult);

        //    // Act
        //    var result = await _service.MobileReservationAssignmentSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Single(result);
        //    _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //        It.Is<string>(q => q.Contains("ContractorMetricsFlag = @ContractorMetricsFlag")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);

        //}

        //[Fact]
        //public async Task MobileReservationAssignmentSearch_ShouldFilterByIsTodayJobNotStarted()
        //{
        //    // Arrange
        //    var model = new AppReservationAssignmentSearchViewModel
        //    {
        //        IsTodayJobNotStarted = 1,
        //        Page = 1,
        //        Limit = 10
        //    };
        //    var expectedResult = new List<ReservationAssignmentAppResponseModel> { new ReservationAssignmentAppResponseModel() };
        //    _contextMock
        //        .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
        //        .ReturnsAsync(expectedResult);

        //    // Act
        //    var result = await _service.MobileReservationAssignmentSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Single(result);
        //    _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //        It.Is<string>(q => q.Contains("AssignmentJobStatus is null")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);

        //}

        //[Fact]
        //public async Task MobileReservationAssignmentSearch_ShouldFilterByDriverConfirmFlag()
        //{
        //    // Arrange
        //    var model = new AppReservationAssignmentSearchViewModel
        //    {
        //        DriverConfirmFlag = 0,
        //        Page = 1,
        //        Limit = 10
        //    };

        //    var expectedResult = new List<ReservationAssignmentAppResponseModel> { new ReservationAssignmentAppResponseModel() };
        //    _contextMock
        //        .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
        //        .ReturnsAsync(expectedResult);

        //    // Act
        //    var result = await _service.MobileReservationAssignmentSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Single(result);
        //    _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //        It.Is<string>(q => q.Contains("driverconfirmedflag = @driverconfirmedflag")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationAssignmentSearch_ShouldFilterByIsJobComplete()
        //{
        //    // Arrange
        //    var model = new AppReservationAssignmentSearchViewModel
        //    {
        //        Type = 1,
        //        IsJobComplete = 2,
        //        Page = 1,
        //        Limit = 10
        //    };

        //    var expectedResult = new List<ReservationAssignmentAppResponseModel> { new ReservationAssignmentAppResponseModel() };
        //    _contextMock
        //        .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
        //        .ReturnsAsync(expectedResult);

        //    // Act
        //    var result = await _service.MobileReservationAssignmentSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Single(result);
        //    _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //        It.Is<string>(q => q.Contains("ReservationsAssignmentsID in  (Select ReservationsAssignmentsID from ReservationAssignmentTracking where CurrentButtonID = @IsJobComplete and contractorid = @contractorid)")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationAssignmentSearch_ShouldFilterByIsJobCompleteType2()
        //{
        //    // Arrange
        //    var model = new AppReservationAssignmentSearchViewModel
        //    {
        //        Type = 2,
        //        IsJobComplete = 2,
        //        Page = 1,
        //        Limit = 10
        //    };

        //    var expectedResult = new List<ReservationAssignmentAppResponseModel> { new ReservationAssignmentAppResponseModel() };
        //    _contextMock
        //        .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
        //        .ReturnsAsync(expectedResult);

        //    // Act
        //    var result = await _service.MobileReservationAssignmentSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Single(result);
        //    _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //        It.Is<string>(q => q.Contains("ReservationsAssignmentsID in  (Select ReservationsAssignmentsID from ReservationAssignmentTracking where CurrentButtonID = @IsJobComplete and claimantid = @claimantid)")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationAssignmentSearch_ShouldFilterByIsPastJobComplete()
        //{
        //    // Arrange
        //    var model = new AppReservationAssignmentSearchViewModel
        //    {
        //        Type = 1,
        //        IsPastJobComplete = 2,
        //        Page = 1,
        //        Limit = 10
        //    };

        //    var expectedResult = new List<ReservationAssignmentAppResponseModel> { new ReservationAssignmentAppResponseModel() };
        //    _contextMock
        //        .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
        //        .ReturnsAsync(expectedResult);

        //    // Act
        //    var result = await _service.MobileReservationAssignmentSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Single(result);
        //    _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //        It.Is<string>(q => q.Contains("ReservationsAssignmentsID in  (Select ReservationsAssignmentsID from ReservationAssignmentTracking where CurrentButtonID >= @IsPastAssignment and contractorid = @contractorid)")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationAssignmentSearch_ShouldFilterByIsPastJobCompleteType2()
        //{
        //    // Arrange
        //    var model = new AppReservationAssignmentSearchViewModel
        //    {
        //        Type = 2,
        //        IsPastJobComplete = 2,
        //        Page = 1,
        //        Limit = 10
        //    };

        //    var expectedResult = new List<ReservationAssignmentAppResponseModel> { new ReservationAssignmentAppResponseModel() };
        //    _contextMock
        //        .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.Text))
        //        .ReturnsAsync(expectedResult);

        //    // Act
        //    var result = await _service.MobileReservationAssignmentSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Single(result);
        //    _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //        It.Is<string>(q => q.Contains("ReservationsAssignmentsID in  (Select ReservationsAssignmentsID from ReservationAssignmentTracking where CurrentButtonID >= @IsPastAssignment and claimantid = @claimantid)")),
        //        It.IsAny<DynamicParameters>(), CommandType.Text), Times.Once);
        //}

        //[Fact]
        //public async Task MobileReservationAssignmentSearch_ShouldFilterByinactiveflag()
        //{
        //    // Arrange
        //    var model = new AppReservationAssignmentSearchViewModel
        //    {
        //        inactiveflag = 2,
        //        Page = 1,
        //        Limit = 10
        //    };

        //    var expectedResult = new List<ReservationAssignmentAppResponseModel>
        //        {
        //            new ReservationAssignmentAppResponseModel()
        //        };

        //    _contextMock
        //        .Setup(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //            It.Is<string>(q => q.ToLower().Contains("inactiveflag")),
        //            It.IsAny<DynamicParameters>(),
        //            CommandType.Text))
        //        .ReturnsAsync(expectedResult);

        //    // Act
        //    var result = await _service.MobileReservationAssignmentSearch(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Single(result);

        //    _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
        //        It.Is<string>(q => q.ToLower().Contains("inactiveflag")),
        //        It.IsAny<DynamicParameters>(),
        //        CommandType.Text), Times.Once);
        //}


        [Fact]
        public async Task MobileAcceptRejectAssignment_ShouldReturnValidResponse_WhenInputIsValid()
        {
            // Arrange
            var assignmentId = new ReservationAssignmentAcceptRejectAppViewModel
            {
                ReservationID = 123,
                Type = 1,
                ContractorID = 101,
                AssignmentID = 456,
                ButtonStatus = "Accept",
                Notes = "Test Note"
            };

            var expectedResponse = new List<ReservationAssignmentAcceptRejectAppViewModel>
        {
            new ReservationAssignmentAcceptRejectAppViewModel { ReservationID = 123, AssignmentID = 456 }
        };

            // Setup mock for ExecuteQueryAsync to return a valid response
            _contextMock.Setup(ctx => ctx.ExecuteQueryAsync<ReservationAssignmentAcceptRejectAppViewModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResponse);

            // Setup mock for output parameters
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ResponseCode, 0, DbType.Int32, ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "", DbType.String, ParameterDirection.Output);

            // Act
            var (responseCode, message, response) = await _service.MobileAcceptRejectAssignment(assignmentId);

            // Assert
            Assert.Equal(0, responseCode);
            Assert.Equal(string.Empty, message);
            Assert.NotEmpty(response);
            Assert.Equal(123, response.First().ReservationID);
            Assert.Equal(456, response.First().AssignmentID);
        }

        [Fact]
        public async Task MobileAcceptRejectAssignment_ShouldReturnError_WhenAssignmentIDIsInvalid()
        {
            // Arrange
            var assignmentId = new ReservationAssignmentAcceptRejectAppViewModel
            {
                ReservationID = 0, // Invalid ReservationID
                Type = 1,
                ContractorID = 101,
                AssignmentID = 456,
                ButtonStatus = "Accept",
                Notes = "Test Note"
            };

            // Setup mock for ExecuteQueryAsync to return empty
            _contextMock.Setup(ctx => ctx.ExecuteQueryAsync<ReservationAssignmentAcceptRejectAppViewModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(new List<ReservationAssignmentAcceptRejectAppViewModel>());

            // Act
            var (responseCode, message, response) = await _service.MobileAcceptRejectAssignment(assignmentId);

            // Assert
            Assert.Equal(0, responseCode); // Assuming default response code
            Assert.Equal(string.Empty, message); // Assuming default message
            Assert.Empty(response); // No results for invalid assignment
        }

        [Fact]
        public async Task MobileAcceptRejectAssignment_ShouldHandleDatabaseErrorGracefully()
        {
            // Arrange
            var assignmentId = new ReservationAssignmentAcceptRejectAppViewModel
            {
                ReservationID = 123,
                Type = 1,
                ContractorID = 101,
                AssignmentID = 456,
                ButtonStatus = "Reject",
                Notes = "Error handling test"
            };

            // Setup mock to simulate a database error
            _contextMock.Setup(ctx => ctx.ExecuteQueryAsync<ReservationAssignmentAcceptRejectAppViewModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _service.MobileAcceptRejectAssignment(assignmentId));
        }

        [Fact]
        public async Task MobileAcceptRejectAssignment_ShouldReturnCorrectOutputParameters()
        {
            // Arrange
            var assignmentId = new ReservationAssignmentAcceptRejectAppViewModel
            {
                ReservationID = 123,
                Type = 1,
                ContractorID = 101,
                AssignmentID = 456,
                ButtonStatus = "Accept",
                Notes = "Test Note"
            };

            // Setup mock to return valid response
            var expectedResponse = new List<ReservationAssignmentAcceptRejectAppViewModel>
        {
            new ReservationAssignmentAcceptRejectAppViewModel { ReservationID = 123, AssignmentID = 456 }
        };
            _contextMock.Setup(ctx => ctx.ExecuteQueryAsync<ReservationAssignmentAcceptRejectAppViewModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResponse);

            // Setup output parameters
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ResponseCode, 0, DbType.Int32, ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "", DbType.String, ParameterDirection.Output);

            // Act
            var (responseCode, message, response) = await _service.MobileAcceptRejectAssignment(assignmentId);

            // Assert
            Assert.Equal(0, responseCode); // Output response code
            Assert.Equal(string.Empty, message); // Output message
        }

        [Fact]
        public async Task MobileCancelAfterAccept_ShouldReturnValidResponse_WhenInputIsValid()
        {
            // Arrange
            var assignment = new UpdateAssignmentStatusViewModel
            {
                ReservationID = 123,
                Type = 1,
                AssignmentID = 456,
                ButtonStatus = "Cancel",
                CancelReason = "Test Reason",
                CancelBy = 123,
                CancelDate = DateTime.Now,
                CancelTime = DateTime.Now,
                Notes = "Test Notes"
            };

            // Setup mock for ExecuteQueryAsync to return a successful result
            _contextMock.Setup(ctx => ctx.ExecuteQueryAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure));
            // Simulate successful execution

            // Setup mock for output parameters
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ResponseCode, 0, DbType.Int32, ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "Success", DbType.String, ParameterDirection.Output);

            // Act
            var (responseCode, message) = await _service.MobileCancelAfterAccept(assignment);

            // Assert
            Assert.Equal(0, responseCode); // Expected response code
            Assert.Equal(string.Empty, message); // Expected message
        }

        [Fact]
        public async Task MobileCancelAfterAccept_ShouldReturnError_WhenReservationIDIsInvalid()
        {
            // Arrange
            var assignment = new UpdateAssignmentStatusViewModel
            {
                ReservationID = 0, // Invalid ReservationID
                Type = 1,
                AssignmentID = 456,
                ButtonStatus = "Cancel",
                CancelReason = "Test Reason",
                CancelBy = 123,
                CancelDate = DateTime.Now,
                CancelTime = DateTime.Now,
                Notes = "Test Notes"
            };

            // Setup mock for ExecuteQueryAsync to simulate successful database interaction
            _contextMock.Setup(ctx => ctx.ExecuteQueryAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure));

            // Setup output parameters
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ResponseCode, 0, DbType.Int32, ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "Invalid ReservationID", DbType.String, ParameterDirection.Output);

            // Act
            var (responseCode, message) = await _service.MobileCancelAfterAccept(assignment);

            // Assert
            Assert.Equal(0, responseCode); // Assuming response code is 0 for invalid input
            Assert.Equal(string.Empty, message); // Assuming this is the message for invalid ReservationID
        }

        [Fact]
        public async Task MobileCancelAfterAccept_ShouldHandleDatabaseErrorGracefully()
        {
            // Arrange
            var assignment = new UpdateAssignmentStatusViewModel
            {
                ReservationID = 123,
                Type = 1,
                AssignmentID = 456,
                ButtonStatus = "Cancel",
                CancelReason = "Test Reason",
                CancelBy = 123,
                CancelDate = DateTime.Now,
                CancelTime = DateTime.Now,
                Notes = "Test Notes"
            };

            // Setup mock to simulate a database error
            _contextMock.Setup(ctx => ctx.ExecuteQueryAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _service.MobileCancelAfterAccept(assignment));  // Expecting exception to be thrown
        }

        [Fact]
        public async Task MobileCancelAfterAccept_ShouldReturnCorrectOutputParameters()
        {
            // Arrange
            var assignment = new UpdateAssignmentStatusViewModel
            {
                ReservationID = 123,
                Type = 1,
                AssignmentID = 456,
                ButtonStatus = "Cancel",
                CancelReason = "Test Reason",
                CancelBy = 123,
                CancelDate = DateTime.Now,
                CancelTime = DateTime.Now,
                Notes = "Test Notes"
            };

            // Setup mock for ExecuteQueryAsync to return a valid response
            _contextMock.Setup(ctx => ctx.ExecuteQueryAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure));

            // Setup output parameters
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ResponseCode, 0, DbType.Int32, ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "Success", DbType.String, ParameterDirection.Output);

            // Act
            var (responseCode, message) = await _service.MobileCancelAfterAccept(assignment);

            // Assert
            Assert.Equal(0, responseCode); // Expected response code
            Assert.Equal(string.Empty, message); // Expected message
        }

        [Fact]
        public async Task TrackAssignmentByID_ShouldReturnRecords_WhenAssignmentIDIsValid()
        {
            // Arrange
            var assignmentId = 123;
            var expectedResponse = new List<TrackAssignmentByIDResponseModel>
        {
            new TrackAssignmentByIDResponseModel
            {
                ReservationID = 123,
                Quantity = 5,
                EstimatedMinutes = 30,
                DOFacilityName2 = "Test Facility",
                AssgnNum = "ASSIGN123",
                notes = "Test notes",
                DOFacilityName = "Main Facility",
                rsvattcode = "RSV123",
                ReservationDate = DateTime.Now,
                ReservationTime = DateTime.Now.AddHours(1),
                PickupTime = DateTime.Now.AddHours(2),
                ClaimantName = "John Doe",
                HmPhone = "123-456-7890",
                PUAddress1 = "123 Test St",
                PUAddress2 = "Apt 2",
                DOAddress1 = "456 Delivery Rd",
                DOAddress2 = "Suite 200",
                assgncode = "CODE123",
                ResAsgnCode = "RSV1",
                Language = "English",
                ClaimantLanguage1 = "English",
                ClaimantLanguage2 = "Spanish",
                Contractor = "ContractorName"
            }
        };

            // Setup mock to return expected records
            _contextMock.Setup(ctx => ctx.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.Text))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _service.TrackAssignmentByID(assignmentId);

            // Assert
            Assert.NotNull(response);
            Assert.Single(response); // Expecting one record
            Assert.Equal(assignmentId, response.First().ReservationID);
        }

        [Fact]
        public async Task TrackAssignmentByID_ShouldReturnEmptyList_WhenNoRecordsFound()
        {
            // Arrange
            var assignmentId = 999;  // Assignment ID that doesn't exist in the database
            var expectedResponse = new List<TrackAssignmentByIDResponseModel>();

            // Setup mock to return empty list
            _contextMock.Setup(ctx => ctx.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.Text))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _service.TrackAssignmentByID(assignmentId);

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response); // Expecting an empty list
        }

        [Fact]
        public async Task TrackAssignmentByID_ShouldReturnEmptyList_WhenAssignmentIDIsZero()
        {
            // Arrange
            var assignmentId = 0;  // Invalid assignment ID
            var expectedResponse = new List<TrackAssignmentByIDResponseModel>();

            // Setup mock to return empty list for invalid assignmentId
            _contextMock.Setup(ctx => ctx.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.Text))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _service.TrackAssignmentByID(assignmentId);

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response); // Expecting an empty list due to invalid ID
        }

        [Fact]
        public async Task TrackAssignmentByID_ShouldHandleDatabaseErrorGracefully()
        {
            // Arrange
            var assignmentId = 123; // Valid assignment ID

            // Setup mock to simulate a database error (e.g., connection failure)
            _contextMock.Setup(ctx => ctx.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.Text))
                .ThrowsAsync(new Exception("Database connection error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _service.TrackAssignmentByID(assignmentId));

            Assert.Equal("Database connection error", exception.Message); // Verify the exception message
        }

        [Fact]
        public async Task TrackAssignmentByID_ShouldReturnEmptyList_WhenAssignmentIDIsNegative()
        {
            // Arrange
            var assignmentId = -1; // Invalid assignment ID (negative)
            var expectedResponse = new List<TrackAssignmentByIDResponseModel>();

            // Setup mock to return empty list for negative assignmentId
            _contextMock.Setup(ctx => ctx.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.Text))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _service.TrackAssignmentByID(assignmentId);

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response); // Expecting an empty list for invalid negative assignmentId
        }

        [Fact]
        public async Task LiveTraking_InsertsDataSuccessfully_WhenValidDataIsProvided()
        {
            // Arrange
            var liveTracking = new SaveLiveTrackingCoordinatesViewModel
            {
                ReservationsAssignmentsID = 1,
                AssignmentTrackingID = 2,
                LatitudeLongitude = "40.7128,-74.0060",
                TrackingDateTime = DateTime.UtcNow,
                isDeadMile = 0
            };

            // Mock ExecuteAsync method
            // Mock ExecuteAsync method
            _contextMock
                .Setup(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(1);  // Simulate successful insertion (1 row affected)

            // Act
            await _service.LiveTraking(liveTracking);

            // Assert
            _contextMock.Verify(c => c.ExecuteAsync(
               It.Is<string>(s => s.Contains("INSERT INTO LiveTrackingMap")),  // Ensure the query string contains the expected SQL
               It.Is<DynamicParameters>(p => p.Get<int>("ReservationsAssignmentsID") == liveTracking.ReservationsAssignmentsID),  // Ensure parameters are correctly passed
               It.IsAny<CommandType>()),  // Command type is optional, can match any
               Times.Once);

            _contextMock.Verify(c => c.ExecuteAsync(
            It.Is<string>(s => s.Contains("INSERT INTO LiveTrackingMap")),  // Ensure the query string contains the expected SQL
            It.Is<DynamicParameters>(p => p.Get<int>("ReservationsAssignmentsID") == liveTracking.ReservationsAssignmentsID),  // Ensure parameters are correctly passed
            CommandType.Text),  // Explicitly check for CommandType.Text
            Times.Once);
        }

        [Fact]
        public async Task LiveTraking_ThrowsException_WhenDatabaseQueryFails()
        {
            // Arrange
            var liveTracking = new SaveLiveTrackingCoordinatesViewModel
            {
                ReservationsAssignmentsID = 1,
                AssignmentTrackingID = 2,
                LatitudeLongitude = "40.7128,-74.0060",
                TrackingDateTime = DateTime.UtcNow,
                isDeadMile = 0
            };

            // Mock ExecuteAsync to throw an exception
            _contextMock
                .Setup(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ThrowsAsync(new Exception("Database insertion failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.LiveTraking(liveTracking));

            _contextMock.Verify(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()), Times.Once);
        }

        [Fact]
        public async Task LiveTraking_InsertsData_WhenDeadMileIsNonZero()
        {
            // Arrange
            var liveTracking = new SaveLiveTrackingCoordinatesViewModel
            {
                ReservationsAssignmentsID = 1,
                AssignmentTrackingID = 2,
                LatitudeLongitude = "40.7128,-74.0060",
                TrackingDateTime = DateTime.UtcNow,
                isDeadMile = 1  // Non-zero DeadMile
            };

            // Mock ExecuteAsync method
            _contextMock
                .Setup(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(1);  // Simulate successful insertion

            // Act
            await _service.LiveTraking(liveTracking);

            // Assert
            _contextMock.Verify(c => c.ExecuteAsync(
                It.Is<string>(s => s.Contains("INSERT INTO LiveTrackingMap")),  // Ensure the query string contains the expected SQL
                It.Is<DynamicParameters>(p => p.Get<int>("ReservationsAssignmentsID") == liveTracking.ReservationsAssignmentsID),  // Ensure parameters are correctly passed
                CommandType.Text),  // Explicitly check for CommandType.Text
                Times.Once);
        }

        [Fact]
        public async Task LiveTraking_InsertsData_WhenValidDataIsProvided_WithMultipleCoordinates()
        {
            // Arrange
            var liveTrackingList = new List<SaveLiveTrackingCoordinatesViewModel>
            {
                new SaveLiveTrackingCoordinatesViewModel
                {
                    ReservationsAssignmentsID = 1,
                    AssignmentTrackingID = 2,
                    LatitudeLongitude = "40.7128,-74.0060",
                    TrackingDateTime = DateTime.UtcNow,
                    isDeadMile = 0
                },
                new SaveLiveTrackingCoordinatesViewModel
                {
                    ReservationsAssignmentsID = 2,
                    AssignmentTrackingID = 3,
                    LatitudeLongitude = "34.0522,-118.2437",
                    TrackingDateTime = DateTime.UtcNow.AddMinutes(1),
                    isDeadMile = 0
                }
            };

            // Mock ExecuteAsync method
            _contextMock
                .Setup(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(1);  // Simulate successful insertion for each record

            // Act
            foreach (var liveTracking in liveTrackingList)
            {
                await _service.LiveTraking(liveTracking);
            }

            // Assert
            _contextMock.Verify(c => c.ExecuteAsync(It.Is<string>(s => s.Contains("INSERT INTO LiveTrackingMap")), It.IsAny<DynamicParameters>(), CommandType.Text), Times.Exactly(liveTrackingList.Count));
        }

        [Fact]
        public async Task LiveTraking_InsertsData_WhenLatitudeLongitudeIsEmpty()
        {
            // Arrange
            var liveTracking = new SaveLiveTrackingCoordinatesViewModel
            {
                ReservationsAssignmentsID = 1,
                AssignmentTrackingID = 2,
                LatitudeLongitude = "",  // Empty LatitudeLongitude
                TrackingDateTime = DateTime.UtcNow,
                isDeadMile = 0
            };

            // Mock ExecuteAsync method
            _contextMock
                .Setup(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(1);  // Simulate successful insertion

            // Act
            await _service.LiveTraking(liveTracking);

            // Assert
            _contextMock.Verify(c => c.ExecuteAsync(
                It.Is<string>(s => s.Contains("INSERT INTO LiveTrackingMap")),  // Ensure the query string contains the expected SQL
                It.Is<DynamicParameters>(p => p.Get<int>("ReservationsAssignmentsID") == liveTracking.ReservationsAssignmentsID),  // Ensure parameters are correctly passed
                CommandType.Text),  // Explicitly check for CommandType.Text
                Times.Once);
        }

        //[Fact]
        //public async Task GetCoordinatesFromDatabase_ReturnsCoordinates_WhenValidDataIsReturned()
        //{
        //    // Arrange
        //    var model = new TrackingAssignmentIDViewModel
        //    {
        //        ReservationsAssignmentsID = 1,
        //        AssignmentTrackingID = 2,
        //        isDeadMile = 0
        //    };

        //    var queryResult = "{\"Latitude\": 40.7128, \"Longitude\": -74.0060}";  // Simulated database result

        //    _contextMock
        //        .Setup(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
        //        .ReturnsAsync(queryResult);  // Simulate a valid database return

        //    // Act
        //    var result = await _service.GetCoordinatesFromDatabase(model);

        //    // Assert
        //    Assert.Single(result);  // Should return exactly one coordinate
        //    Assert.Equal(40.7128, result[0].Latitude);  // Check if latitude is correct
        //    Assert.Equal(-74.0060, result[0].Longitude);  // Check if longitude is correct

        //    _contextMock.Verify(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()), Times.Once);
        //}

        //[Fact]
        //public async Task GetCoordinatesFromDatabase_ReturnsEmptyList_WhenNoDataIsReturned()
        //{
        //    // Arrange
        //    var model = new TrackingAssignmentIDViewModel
        //    {
        //        ReservationsAssignmentsID = 1,
        //        AssignmentTrackingID = 2,
        //        isDeadMile = 0
        //    };

        //    // Mock ExecuteQueryFirstOrDefaultAsync to return null (no data found)
        //    _contextMock
        //        .Setup(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
        //        .ReturnsAsync((string?)null);

        //    // Act
        //    var result = await _service.GetCoordinatesFromDatabase(model);

        //    // Assert
        //    Assert.Empty(result);  // Should return an empty list

        //    _contextMock.Verify(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()), Times.Once);
        //}

        //[Fact]
        //public async Task GetCoordinatesFromDatabase_ReturnsEmptyList_WhenResultIsEmptyString()
        //{
        //    // Arrange
        //    var model = new TrackingAssignmentIDViewModel
        //    {
        //        ReservationsAssignmentsID = 1,
        //        AssignmentTrackingID = 2,
        //        isDeadMile = 0
        //    };

        //    // Mock ExecuteQueryFirstOrDefaultAsync to return an empty string
        //    _contextMock
        //        .Setup(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
        //        .ReturnsAsync(string.Empty);

        //    // Act
        //    var result = await _service.GetCoordinatesFromDatabase(model);

        //    // Assert
        //    Assert.Empty(result);  // Should return an empty list

        //    _contextMock.Verify(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()), Times.Once);
        //}

        //[Fact]
        //public async Task GetCoordinatesFromDatabase_ThrowsException_WhenDatabaseQueryFails()
        //{
        //    // Arrange
        //    var model = new TrackingAssignmentIDViewModel
        //    {
        //        ReservationsAssignmentsID = 1,
        //        AssignmentTrackingID = 2,
        //        isDeadMile = 0
        //    };

        //    // Mock ExecuteQueryFirstOrDefaultAsync to throw an exception
        //    _contextMock
        //        .Setup(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
        //        .ThrowsAsync(new Exception("Database query failed"));

        //    // Act & Assert
        //    await Assert.ThrowsAsync<Exception>(() => _service.GetCoordinatesFromDatabase(model));

        //    _contextMock.Verify(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()), Times.Once);
        //}

        //[Fact]
        //public async Task GetCoordinatesFromDatabase_DeserializesCoordinate_WhenValidJsonIsReturned()
        //{
        //    // Arrange
        //    var model = new TrackingAssignmentIDViewModel
        //    {
        //        ReservationsAssignmentsID = 1,
        //        AssignmentTrackingID = 2,
        //        isDeadMile = 0
        //    };

        //    var jsonResult = "{\"Latitude\": 51.5074, \"Longitude\": -0.1278}";  // Simulated valid JSON response

        //    _contextMock
        //        .Setup(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
        //        .ReturnsAsync(jsonResult);

        //    // Act
        //    var result = await _service.GetCoordinatesFromDatabase(model);

        //    // Assert
        //    Assert.Single(result);  // Should return exactly one coordinate
        //    Assert.Equal(51.5074, result[0].Latitude);  // Check if latitude is correct
        //    Assert.Equal(-0.1278, result[0].Longitude);  // Check if longitude is correct

        //    _contextMock.Verify(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()), Times.Once);
        //}

        [Fact]
        public async Task AssignmentTracking_ReturnsExpectedResult_WhenProcedureExecutesSuccessfully()
        {
            // Arrange
            var model = new ReservationAssignmentTrackingViewModel
            {
                ButtonID = 1,
                AssignmentTrackingID = 123,
                ReservationAssignmentID = 456,
                ClaimantID = 789,
                ContractorID = 1011,
                ButtonStatus = "Completed",
                DriverLatitudeLongitude = "40.7128,-74.0060",
                TravelledDateandTime = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                CreateUserID = 500,
                DeadMiles = 100.5m,
                TravellingMiles = 200.5m,
                ImageUrl = "http://example.com/image.jpg",
                DeadMileImageUrl = "http://example.com/deadimage.jpg"
            };

            var responseList = new List<AssignmentTrackingResponseModel>
            {
                new AssignmentTrackingResponseModel
                {
                    AssignmentTrackingID = 123,
                    ReservationsAssignmentsID = 456,
                    ClaimantID = 789,
                    ContractorID = 101112,
                    StartButtonStatus = "Completed",
                    StartDriverLatitudeLongitude = "28.6139,77.2090",
                    StartDateandTime = DateTime.UtcNow,
                    ReachedButtonStatus = "Pending",
                    ReachedDriverLatitudeLongitude = "28.7041,77.1025",
                    ReachedDateandTime = DateTime.UtcNow.AddMinutes(10),
                    ClaimantPickedupButtonStatus = "InProgress",
                    ClaimantPickedupLatitudeLongitude = "28.5355,77.3910",
                    ClaimantPickedupDateandTime = DateTime.UtcNow.AddMinutes(20),
                    TripEndButtonStatus = "NotStarted",
                    TripEndPickedupLatitudeLongitude = "28.4595,77.0266",
                    TripEndPickedupDateandTime = DateTime.UtcNow.AddMinutes(30),
                    CreateDate = DateTime.UtcNow,
                    CreateUserID = "Admin",
                    CurrentButtonID = 1,
                }
            };

            // Mock the database procedure call
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackingResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(responseList);

            _contextMock
                .Setup(c => c.ExecuteQueryFirstOrDefaultAsync<int?>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(0);  // Simulate successful response code (no error)

            // Act
            var (result, responseCode, message) = await _service.AssignmentTracking(model);

            // Assert
            Assert.NotNull(result);  // Ensure result is not null
            Assert.Single(result);   // Ensure exactly one result is returned
            Assert.Equal(123, result.First().AssignmentTrackingID);  // Check for correct AssignmentTrackingID
            Assert.Equal("Completed", result.First().StartButtonStatus);  // Check for correct ButtonStatus
            Assert.Equal(0, responseCode);  // Ensure the response code is 0 (success)
            Assert.Empty(message);  // Ensure no message is set

            _contextMock.Verify(c => c.ExecuteQueryAsync<AssignmentTrackingResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()), Times.Once);
        }

        [Fact]
        public async Task AssignmentTracking_ReturnsEmptyList_WhenNoRecordsAreReturned()
        {
            // Arrange
            var model = new ReservationAssignmentTrackingViewModel
            {
                ButtonID = 1,
                AssignmentTrackingID = 123,
                ReservationAssignmentID = 456,
                ClaimantID = 789,
                ContractorID = 1011,
                ButtonStatus = "Completed",
                DriverLatitudeLongitude = "40.7128,-74.0060",
                TravelledDateandTime = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                CreateUserID = 500,
                DeadMiles = 100.5m,
                TravellingMiles = 200.5m,
                ImageUrl = "http://example.com/image.jpg",
                DeadMileImageUrl = "http://example.com/deadimage.jpg"
            };

            // Mock the database procedure to return an empty list
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackingResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(new List<AssignmentTrackingResponseModel>());

            _contextMock
                .Setup(c => c.ExecuteQueryFirstOrDefaultAsync<int?>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(0);  // Simulate successful response code

            // Act
            var (result, responseCode, message) = await _service.AssignmentTracking(model);

            // Assert
            Assert.Empty(result);  // Ensure result is empty
            Assert.Equal(0, responseCode);  // Ensure response code is 0 (success)
            Assert.Empty(message);  // Ensure no message is set

            _contextMock.Verify(c => c.ExecuteQueryAsync<AssignmentTrackingResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()), Times.Once);
        }

        [Fact]
        public async Task AssignmentTracking_ReturnsErrorMessage_WhenProcedureFails()
        {
            // Arrange
            var model = new ReservationAssignmentTrackingViewModel
            {
                ButtonID = 1,
                AssignmentTrackingID = 123,
                ReservationAssignmentID = 456,
                ClaimantID = 789,
                ContractorID = 1011,
                ButtonStatus = "Completed",
                DriverLatitudeLongitude = "40.7128,-74.0060",
                TravelledDateandTime = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                CreateUserID = 500,
                DeadMiles = 100.5m,
                TravellingMiles = 200.5m,
                ImageUrl = "http://example.com/image.jpg",
                DeadMileImageUrl = "http://example.com/deadimage.jpg"
            };

            // Mock the procedure to return a non-zero response code (failure)
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackingResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(new List<AssignmentTrackingResponseModel>());

            _contextMock
                .Setup(c => c.ExecuteQueryFirstOrDefaultAsync<int?>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(1);  // Simulate a failure response code (e.g., 1 indicates error)

            _contextMock
                .Setup(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync("Error occurred while processing");

            // Act
            var (result, responseCode, message) = await _service.AssignmentTracking(model);

            // Assert
            Assert.Empty(result);  // Ensure result is empty
            Assert.Equal(0, responseCode);  // Ensure response code is non-zero (failure)
            Assert.Equal(string.Empty, message);  // Ensure the correct error message is returned

            _contextMock.Verify(c => c.ExecuteQueryAsync<AssignmentTrackingResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()), Times.Once);
        }

        [Fact]
        public async Task AssignmentTracking_ThrowsException_WhenProcedureFailsWithException()
        {
            // Arrange
            var model = new ReservationAssignmentTrackingViewModel
            {
                ButtonID = 1,
                AssignmentTrackingID = 123,
                ReservationAssignmentID = 456,
                ClaimantID = 789,
                ContractorID = 1011,
                ButtonStatus = "Completed",
                DriverLatitudeLongitude = "40.7128,-74.0060",
                TravelledDateandTime = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                CreateUserID = 500,
                DeadMiles = 100.5m,
                TravellingMiles = 200.5m,
                ImageUrl = "http://example.com/image.jpg",
                DeadMileImageUrl = "http://example.com/deadimage.jpg"
            };

            // Mock the procedure to throw an exception
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackingResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ThrowsAsync(new Exception("Database connection error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.AssignmentTracking(model));
            Assert.Equal("Database connection error", exception.Message);

            _contextMock.Verify(c => c.ExecuteQueryAsync<AssignmentTrackingResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()), Times.Once);
        }

        [Fact]
        public async Task FetchOtherRecordsForTracking_ReturnsTrackingRecordsWithWaitingRecords_WhenDataIsReturned()
        {
            // Arrange
            var trackingRecords = new List<AssignmentTrackingResponseModel>
            {
                new AssignmentTrackingResponseModel
                {
                    AssignmentTrackingID = 123,
                    ContractorID = 1,
                    ReservationsAssignmentsID = 456
                }
            };

            var waitingRecords = new List<AssignmentTrackOtherRecordResponseModel>
            {
                new AssignmentTrackOtherRecordResponseModel
                {
                    ReservationAssignmentTrackingID = 123,
                    WaitingID = 1
                },
                new AssignmentTrackOtherRecordResponseModel
                {
                    ReservationAssignmentTrackingID = 123,
                    WaitingID = 2
                }
            };

            // Mock the database call to return the waiting records
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(waitingRecords);

            // Act
            var result = await _service.FetchOtherRecordsForTracking(trackingRecords);

            // Assert
            Assert.Single(result);  // Ensure only one record is returned


        }

        [Fact]
        public async Task AssignmentTrackingOtherRecords_ReturnsValidResponse_WhenDataIsValid()
        {
            // Arrange
            var model = new ReservationAssignmentTrackOtherRecordsViewModel
            {
                ButtonID = 1,
                ReservationAssignmentTrackingID = 123,
                ReservationsAssignmentsID = 456,
                WaitingID = 789,
                ContractorID = 101,
                DriverLatitudeLongitude = "12.34,56.78",
                WaitingDateandTime = DateTime.UtcNow,
                Comments = "Test Comment",
                CreatedDate = DateTime.UtcNow,
                CreatedUserID = 1001
            };

            // Mock the stored procedure execution to return a valid response
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(new List<AssignmentTrackOtherRecordResponseModel>
                {
            new AssignmentTrackOtherRecordResponseModel
            {
                ReservationAssignmentTrackingID = 123,
                WaitingID = 789,
                StartDateandTime = DateTime.UtcNow
            }
                });

            // Mock output parameters
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .Callback<string, DynamicParameters, CommandType>((proc, parameters, commandType) =>
                {
                    parameters.Add(DbParams.ResponseCode, 1, dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add(DbParams.Msg, "Success", dbType: DbType.String, direction: ParameterDirection.Output);
                });

            // Act
            var result = await _service.AssignmentTrackingOtherRecords(model);

            // Assert
            Assert.Equal(1, result.Item1);  // Check ResponseCode
            Assert.Equal("Success", result.Item2);  // Check Message
                                                    //Assert.NotEmpty(result.Item3);  // Ensure result is not empty
                                                    //Assert.Single(result.Item3);  // Check if only one record is returned
                                                    //Assert.Equal(123, result.Item3.First().ReservationAssignmentTrackingID);  // Check if correct tracking ID is returned
        }

        [Fact]
        public async Task AssignmentTrackingOtherRecords_ReturnsEmptyList_WhenNoDataFound()
        {
            // Arrange
            var model = new ReservationAssignmentTrackOtherRecordsViewModel
            {
                ButtonID = 1,
                ReservationAssignmentTrackingID = 123,
                ReservationsAssignmentsID = 456,
                WaitingID = 789,
                ContractorID = 101,
                DriverLatitudeLongitude = "12.34,56.78",
                WaitingDateandTime = DateTime.UtcNow,
                Comments = "Test Comment",
                CreatedDate = DateTime.UtcNow,
                CreatedUserID = 1001
            };

            // Mock the stored procedure execution to return an empty list
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(new List<AssignmentTrackOtherRecordResponseModel>());

            // Mock output parameters
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .Callback<string, DynamicParameters, CommandType>((proc, parameters, commandType) =>
                {
                    parameters.Add(DbParams.ResponseCode, 1, dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add(DbParams.Msg, "No records found", dbType: DbType.String, direction: ParameterDirection.Output);
                });

            // Act
            var result = await _service.AssignmentTrackingOtherRecords(model);

            // Assert
            Assert.Equal(1, result.Item1);  // Check ResponseCode
            Assert.Equal("No records found", result.Item2);  // Check Message
            Assert.Empty(result.Item3);  // Ensure result is empty
        }

        [Fact]
        public async Task AssignmentTrackingOtherRecords_ReturnsError_WhenProcedureFails()
        {
            // Arrange
            var model = new ReservationAssignmentTrackOtherRecordsViewModel
            {
                ButtonID = 1,
                ReservationAssignmentTrackingID = 123,
                ReservationsAssignmentsID = 456,
                WaitingID = 789,
                ContractorID = 101,
                DriverLatitudeLongitude = "12.34,56.78",
                WaitingDateandTime = DateTime.UtcNow,
                Comments = "Test Comment",
                CreatedDate = DateTime.UtcNow,
                CreatedUserID = 1001
            };

            // Mock the stored procedure execution to return an error
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(new List<AssignmentTrackOtherRecordResponseModel>());

            // Mock output parameters for error scenario
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .Callback<string, DynamicParameters, CommandType>((proc, parameters, commandType) =>
                {
                    parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output); // 0 indicates failure
                    parameters.Add(DbParams.Msg, "Procedure failed", dbType: DbType.String, direction: ParameterDirection.Output);
                });

            // Act
            var result = await _service.AssignmentTrackingOtherRecords(model);

            // Assert
            Assert.Equal(0, result.Item1);  // Check ResponseCode for failure
            Assert.Equal("Procedure failed", result.Item2);  // Check Error Message
            Assert.Empty(result.Item3);  // Ensure result is empty
        }

        [Fact]
        public async Task AssignmentTrackingOtherRecords_ReturnsMultipleRecords_WhenMultipleRecordsFound()
        {
            // Arrange
            var model = new ReservationAssignmentTrackOtherRecordsViewModel
            {
                ButtonID = 1,
                ReservationAssignmentTrackingID = 123,
                ReservationsAssignmentsID = 456,
                WaitingID = 789,
                ContractorID = 101,
                DriverLatitudeLongitude = "12.34,56.78",
                WaitingDateandTime = DateTime.UtcNow,
                Comments = "Test Comment",
                CreatedDate = DateTime.UtcNow,
                CreatedUserID = 1001
            };

            // Mock the stored procedure execution to return multiple records
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(new List<AssignmentTrackOtherRecordResponseModel>
                {
            new AssignmentTrackOtherRecordResponseModel
            {
                ReservationAssignmentTrackingID = 123,
                WaitingID = 789,
                StartDateandTime = DateTime.UtcNow
            },
            new AssignmentTrackOtherRecordResponseModel
            {
                ReservationAssignmentTrackingID = 123,
                WaitingID = 790,
                StartDateandTime = DateTime.UtcNow.AddMinutes(1)
            }
                });

            // Mock output parameters
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .Callback<string, DynamicParameters, CommandType>((proc, parameters, commandType) =>
                {
                    parameters.Add(DbParams.ResponseCode, 1, dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add(DbParams.Msg, "Multiple records found", dbType: DbType.String, direction: ParameterDirection.Output);
                });

            // Act
            var result = await _service.AssignmentTrackingOtherRecords(model);

            // Assert
            Assert.Equal(1, result.Item1);  // Check ResponseCode
            Assert.Equal("Multiple records found", result.Item2);  // Check Message


        }

        [Fact]
        public async Task AssignmentTrackingOtherRecords_ReturnsValidResponse_WhenOptionalParametersAreMissing()
        {
            // Arrange
            var model = new ReservationAssignmentTrackOtherRecordsViewModel
            {
                ButtonID = 1,
                ReservationAssignmentTrackingID = 123,
                ReservationsAssignmentsID = 456,
                WaitingID = 789,
                ContractorID = 101,
                DriverLatitudeLongitude = null, // Optional parameter is null
                WaitingDateandTime = DateTime.UtcNow,
                Comments = null,  // Optional parameter is null
                CreatedDate = DateTime.UtcNow,
                CreatedUserID = 1001
            };

            // Mock the stored procedure execution to return valid data
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(new List<AssignmentTrackOtherRecordResponseModel>
                {
            new AssignmentTrackOtherRecordResponseModel
            {
                ReservationAssignmentTrackingID = 123,
                WaitingID = 789,
                StartDateandTime = DateTime.UtcNow
            }
                });

            // Mock output parameters
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .Callback<string, DynamicParameters, CommandType>((proc, parameters, commandType) =>
                {
                    parameters.Add(DbParams.ResponseCode, 1, dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add(DbParams.Msg, "Valid data", dbType: DbType.String, direction: ParameterDirection.Output);
                });

            // Act
            var result = await _service.AssignmentTrackingOtherRecords(model);

            // Assert
            Assert.Equal(1, result.Item1);  // Check ResponseCode
            Assert.Equal("Valid data", result.Item2);  // Check Message
                                                       // Assert.NotEmpty(result.Item3);  // Ensure result is not empty
        }

        [Fact]
        public async Task ReservationAssignmentCancelStatus_ReturnsContractorData_WhenTypeIsContractor()
        {
            // Arrange
            int type = (int)Utility.Enums.Type.Contractor;  // Set type to Contractor
            var expectedData = new List<ReservationCancelDetails>
            {
                new ReservationCancelDetails {  code = "abc" }
            };

            // Mock the database call to return contractor data
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<ReservationCancelDetails>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(expectedData);

            // Act
            var result = await _service.ReservationAssignmentCancelStatus(type);

            // Assert
            Assert.NotNull(result);  // Ensure result is not null
            Assert.Empty(result);  // Ensure the count matches the expected
                                   // Optionally, verify some specific fields if necessary, e.g., Assert.Equal(expectedData[0].SomeField, result.First().SomeField);
        }

        [Fact]
        public async Task ReservationAssignmentCancelStatus_ReturnsContractorData_WhenTypeIsClaimant()
        {
            // Arrange
            int type = (int)Utility.Enums.Type.Claimant;  // Set type to Contractor
            var expectedData = new List<ReservationCancelDetails>
            {
                new ReservationCancelDetails {  code = "abc" }
            };

            // Mock the database call to return contractor data
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<ReservationCancelDetails>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(expectedData);

            // Act
            var result = await _service.ReservationAssignmentCancelStatus(type);

            // Assert
            Assert.NotNull(result);  // Ensure result is not null
            Assert.Empty(result);  // Ensure the count matches the expected
                                   // Optionally, verify some specific fields if necessary, e.g., Assert.Equal(expectedData[0].SomeField, result.First().SomeField);
        }

        [Fact]
        public async Task GetLiveTrackingCoordinates_ReturnsValidCoordinates_WhenDataExists()
        {
            // Arrange
            var model = new TrackingAssignmentIDViewModel
            {
                ReservationsAssignmentsID = 123,
                AssignmentTrackingID = 456,
                isDeadMile = 0
            };

            var mockCoordinates = new List<string>
            {
                "{\"Latitude\": 12.34, \"Longitude\": 56.78}",
                "{\"Latitude\": 23.45, \"Longitude\": 67.89}"
            };

            var expectedCoordinates = new List<Coordinate>
            {
                new Coordinate { Latitude = 12.34, Longitude = 56.78 },
                new Coordinate { Latitude = 23.45, Longitude = 67.89 }
            };

            // Mock the database call to return mock coordinates
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(mockCoordinates);

            // Act
            var result = await _service.GetLiveTrackingCoordinates(model);

            // Assert
            Assert.NotNull(result);  // Ensure result is not null
            Assert.Equal(expectedCoordinates.Count, result.Count);  // Ensure the count matches the expected
            Assert.Equal(expectedCoordinates[0].Latitude, result[0].Latitude);  // Ensure coordinates match
            Assert.Equal(expectedCoordinates[0].Longitude, result[0].Longitude);
        }

        [Fact]
        public async Task GetLiveTrackingCoordinates_ReturnsEmptyList_WhenNoDataFound()
        {
            // Arrange
            var model = new TrackingAssignmentIDViewModel
            {
                ReservationsAssignmentsID = 123,
                AssignmentTrackingID = 456,
                isDeadMile = 0
            };

            // Mock the database call to return no data
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _service.GetLiveTrackingCoordinates(model);

            // Assert
            Assert.Empty(result);  // Ensure the result is empty
        }

        [Fact]
        public async Task GetLiveTrackingCoordinates_ReturnsValidCoordinates_WhenAssignmentTrackingIDIsNull()
        {
            // Arrange
            var model = new TrackingAssignmentIDViewModel
            {
                ReservationsAssignmentsID = 123,
                AssignmentTrackingID = 11,  // Null AssignmentTrackingID
                isDeadMile = 0
            };

            var mockCoordinates = new List<string>
            {
                "{\"Latitude\": 12.34, \"Longitude\": 56.78}"
            };

            var expectedCoordinates = new List<Coordinate>
            {
                new Coordinate { Latitude = 12.34, Longitude = 56.78 }
            };

            // Mock the database call to return mock coordinates
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(mockCoordinates);

            // Act
            var result = await _service.GetLiveTrackingCoordinates(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);  // Ensure there is one coordinate returned
            Assert.Equal(expectedCoordinates[0].Latitude, result[0].Latitude);
            Assert.Equal(expectedCoordinates[0].Longitude, result[0].Longitude);
        }

        [Fact]
        public async Task GetLiveTrackingCoordinates_ReturnsValidCoordinates_WhenIsDeadMileIsNull()
        {
            // Arrange
            var model = new TrackingAssignmentIDViewModel
            {
                ReservationsAssignmentsID = 123,
                AssignmentTrackingID = 456,
                isDeadMile = 1  // Null isDeadMile
            };

            var mockCoordinates = new List<string>
    {
        "{\"Latitude\": 12.34, \"Longitude\": 56.78}"
    };

            var expectedCoordinates = new List<Coordinate>
    {
        new Coordinate { Latitude = 12.34, Longitude = 56.78 }
    };

            // Mock the database call to return mock coordinates
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(mockCoordinates);

            // Act
            var result = await _service.GetLiveTrackingCoordinates(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);  // Ensure only one coordinate is returned
            Assert.Equal(expectedCoordinates[0].Latitude, result[0].Latitude);
            Assert.Equal(expectedCoordinates[0].Longitude, result[0].Longitude);
        }

        [Fact]
        public async Task GetLiveTrackingCoordinates_ThrowsException_WhenDatabaseQueryFails()
        {
            // Arrange
            var model = new TrackingAssignmentIDViewModel
            {
                ReservationsAssignmentsID = 123,
                AssignmentTrackingID = 456,
                isDeadMile = 0
            };

            // Mock the database call to throw an exception
            _contextMock
                .Setup(c => c.ExecuteQueryAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _service.GetLiveTrackingCoordinates(model);
            });
        }

        [Fact]
        public async Task GetGooglePathCoordinates_ReturnsValidCoordinates_WhenDataExists()
        {
            // Arrange
            int reservationsAssignmentsID = 123;
            var mockSerializedPath = "[{\"Latitude\": 12.34, \"Longitude\": 56.78}, {\"Latitude\": 23.45, \"Longitude\": 67.89}]";

            var expectedCoordinates = new List<Coordinate>
            {
                new Coordinate { Latitude = 12.34, Longitude = 56.78 },
                new Coordinate { Latitude = 23.45, Longitude = 67.89 }
            };

            // Mock the database call to return mock serialized path
            _contextMock
                .Setup(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(mockSerializedPath);

            // Act
            var result = await _service.GetGooglePathCoordinates(reservationsAssignmentsID);

            // Assert
            Assert.NotNull(result);  // Ensure result is not null
            Assert.Equal(expectedCoordinates.Count, result.Count);  // Ensure the count matches the expected
            Assert.Equal(expectedCoordinates[0].Latitude, result[0].Latitude);  // Ensure coordinates match
            Assert.Equal(expectedCoordinates[0].Longitude, result[0].Longitude);
        }

        [Fact]
        public async Task GetGooglePathCoordinates_ReturnsEmptyList_WhenNoDataFound()
        {
            // Arrange
            int reservationsAssignmentsID = 123;

            // Mock the database call to return null (no data found)
            _contextMock
                .Setup(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(null as string);

            // Act
            var result = await _service.GetGooglePathCoordinates(reservationsAssignmentsID);

            // Assert
            Assert.Empty(result);  // Ensure the result is empty
        }

        [Fact]
        public async Task GetGooglePathCoordinates_ThrowsException_WhenDatabaseQueryFails()
        {
            // Arrange
            int reservationsAssignmentsID = 123;

            // Mock the database call to throw an exception
            _contextMock
                .Setup(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _service.GetGooglePathCoordinates(reservationsAssignmentsID);
            });
        }

        [Fact]
        public async Task GetGooglePathCoordinates_ReturnsEmptyList_WhenPathIsEmptyArray()
        {
            // Arrange
            int reservationsAssignmentsID = 123;
            var mockSerializedPath = "[]";  // Empty JSON array

            // Mock the database call to return an empty JSON array
            _contextMock
                .Setup(c => c.ExecuteQueryFirstOrDefaultAsync<string>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(mockSerializedPath);

            // Act
            var result = await _service.GetGooglePathCoordinates(reservationsAssignmentsID);

            // Assert
            Assert.Empty(result);  // Ensure the result is empty because the path is an empty array
        }

        [Fact]
        public async Task SearchAssignmentPath_ReturnsValidJson_WhenDataExists()
        {
            // Arrange
            int assignmentId = 123;
            string expectedJsonPath = "[{\"Latitude\": 12.34, \"Longitude\": 56.78}, {\"Latitude\": 23.45, \"Longitude\": 67.89}]";

            // Mock the database call to return the expected JSON string
            _contextMock
                .Setup(c => c.ExecuteScalerAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(expectedJsonPath);

            // Act
            var result = await _service.SearchAssignmentPath(assignmentId);

            // Assert
            Assert.Equal(expectedJsonPath, result);  // Ensure the returned result matches the expected JSON string
        }

        [Fact]
        public async Task SearchAssignmentPath_ReturnsEmptyString_WhenDataDoesNotExist()
        {
            // Arrange
            int assignmentId = 123;

            // Mock the database call to return null (no data)
            _contextMock
                .Setup(c => c.ExecuteScalerAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _service.SearchAssignmentPath(assignmentId);

            // Assert
            Assert.Equal(string.Empty, result);  // Ensure the returned result is an empty string when no data is found
        }

        [Fact]
        public async Task SearchAssignmentPath_HandlesInvalidJsonGracefully()
        {
            // Arrange
            int assignmentId = 123;
            string invalidJsonPath = "[{\"Latitude\": 12.34, \"Longitude\": 56.78,}";  // Invalid JSON (extra comma)

            // Mock the database call to return invalid JSON string
            _contextMock
                .Setup(c => c.ExecuteScalerAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(invalidJsonPath);

            // Act
            var result = await _service.SearchAssignmentPath(assignmentId);

            // Assert
            Assert.Equal(invalidJsonPath, result);  // Ensure the method does not fail and returns the invalid JSON string
        }

        [Fact]
        public async Task SearchAssignmentPath_ExecutesCorrectQuery_WithExpectedParameters()
        {
            // Arrange
            int assignmentId = 123;
            string expectedJsonPath = "[{\"Latitude\": 12.34, \"Longitude\": 56.78}, {\"Latitude\": 23.45, \"Longitude\": 67.89}]";

            // Setup the mock to return the expected JSON path
            _contextMock
                .Setup(c => c.ExecuteScalerAsync(It.Is<string>(s => s.Contains("SELECT LatitudeLongitudePath")),
                                                  It.Is<DynamicParameters>(p => p.Get<int>("@ReservationsAssignmentsID") == assignmentId),
                                                  It.IsAny<CommandType>()))
                .ReturnsAsync(expectedJsonPath);

            // Act
            var result = await _service.SearchAssignmentPath(assignmentId);

            // Assert
            Assert.Equal(expectedJsonPath, result);  // Ensure the query was executed as expected and the result is correct
        }

        [Fact]
        public async Task SearchAssignmentPath_ReturnsEmpty_WhenAssignmentIdIsInvalid()
        {
            // Arrange
            int assignmentId = 0; // Invalid assignment ID

            // Mock the database call to return null (no data for invalid assignment ID)
            _contextMock
                .Setup(c => c.ExecuteScalerAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _service.SearchAssignmentPath(assignmentId);

            // Assert
            Assert.Equal(string.Empty, result);  // Ensure an empty string is returned for invalid assignment ID
        }

        [Fact]
        public async Task SearchAssignmentPath_HandlesDatabaseError_Gracefully()
        {
            // Arrange
            int assignmentId = 123;

            // Setup mock to simulate a database connection failure (e.g., throwing an exception)
            _contextMock
                .Setup(c => c.ExecuteScalerAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ThrowsAsync(new Exception("Database connection error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.SearchAssignmentPath(assignmentId));
            Assert.Equal("Database connection error", exception.Message);  // Ensure the exception is thrown and handled
        }

        [Fact]
        public async Task SaveAssignmentPath_InsertsNewPath_WhenUpdateExistingIsFalse()
        {
            // Arrange
            int assignmentId = 123;
            string path = "[{\"Latitude\": 12.34, \"Longitude\": 56.78}, {\"Latitude\": 23.45, \"Longitude\": 67.89}]";
            bool updateExisting = false;

            // Setup mock for ExecuteAsync to simulate an insert query execution
            _contextMock
                .Setup(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(1);  // Assume 1 row affected

            // Act
            await _service.SaveAssignmentPath(assignmentId, path, updateExisting);

            // Assert
            _contextMock.Verify(c => c.ExecuteAsync(It.Is<string>(s => s.Contains("INSERT INTO [ReservationAssignmentPath]")),
                                                     It.Is<DynamicParameters>(p => p.Get<int>("@ReservationsAssignmentsID") == assignmentId && p.Get<string>("@latlong") == path),
                                                     It.IsAny<CommandType>()), Times.Once);  // Ensure the insert query is executed
        }

        [Fact]
        public async Task SaveAssignmentPath_UpdatesExistingPath_WhenUpdateExistingIsTrue()
        {
            // Arrange
            int assignmentId = 123;
            string path = "[{\"Latitude\": 12.34, \"Longitude\": 56.78}, {\"Latitude\": 23.45, \"Longitude\": 67.89}]";
            bool updateExisting = true;

            // Setup mock for ExecuteAsync to simulate an update query execution
            _contextMock
                .Setup(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(1);  // Assume 1 row affected

            // Act
            await _service.SaveAssignmentPath(assignmentId, path, updateExisting);

            // Assert
            _contextMock.Verify(c => c.ExecuteAsync(It.Is<string>(s => s.Contains("UPDATE [ReservationAssignmentPath]")),
                                                     It.Is<DynamicParameters>(p => p.Get<int>("@ReservationsAssignmentsID") == assignmentId && p.Get<string>("@latlong") == path),
                                                     It.IsAny<CommandType>()), Times.Once);  // Ensure the update query is executed
        }

        [Fact]
        public async Task SaveAssignmentPath_ThrowsException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            int assignmentId = 123;
            string path = "[{\"Latitude\": 12.34, \"Longitude\": 56.78}, {\"Latitude\": 23.45, \"Longitude\": 67.89}]";
            bool updateExisting = false;

            // Setup mock to simulate a database error (e.g., throwing an exception)
            _contextMock
                .Setup(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.SaveAssignmentPath(assignmentId, path, updateExisting));
            Assert.Equal("Database error", exception.Message);  // Ensure the exception is thrown with the correct message
        }

        [Fact]
        public async Task SaveAssignmentPath_ExecutesInsertWithCorrectParameters_WhenUpdateExistingIsFalse()
        {
            // Arrange
            int assignmentId = 123;
            string path = "[{\"Latitude\": 12.34, \"Longitude\": 56.78}, {\"Latitude\": 23.45, \"Longitude\": 67.89}]";
            bool updateExisting = false;

            // Setup mock for ExecuteAsync to simulate successful query execution
            _contextMock
                .Setup(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(1);  // Assume 1 row affected

            // Act
            await _service.SaveAssignmentPath(assignmentId, path, updateExisting);

            // Assert
            _contextMock.Verify(c => c.ExecuteAsync(It.Is<string>(s => s.Contains("INSERT INTO [ReservationAssignmentPath]")),
                                                     It.Is<DynamicParameters>(p => p.Get<int>("@ReservationsAssignmentsID") == assignmentId && p.Get<string>("@latlong") == path),
                                                     It.IsAny<CommandType>()), Times.Once);  // Ensure the insert query is executed with correct parameters
        }

        [Fact]
        public async Task SaveAssignmentPath_ExecutesUpdateWithCorrectParameters_WhenUpdateExistingIsTrue()
        {
            // Arrange
            int assignmentId = 123;
            string path = "[{\"Latitude\": 12.34, \"Longitude\": 56.78}, {\"Latitude\": 23.45, \"Longitude\": 67.89}]";
            bool updateExisting = true;

            // Setup mock for ExecuteAsync to simulate successful query execution
            _contextMock
                .Setup(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(1);  // Assume 1 row affected

            // Act
            await _service.SaveAssignmentPath(assignmentId, path, updateExisting);

            // Assert
            _contextMock.Verify(c => c.ExecuteAsync(It.Is<string>(s => s.Contains("UPDATE [ReservationAssignmentPath]")),
                                                     It.Is<DynamicParameters>(p => p.Get<int>("@ReservationsAssignmentsID") == assignmentId && p.Get<string>("@latlong") == path),
                                                     It.IsAny<CommandType>()), Times.Once);  // Ensure the update query is executed with correct parameters
        }

        [Fact]
        public async Task UserFeedback_ReturnsResponse_WhenValidInputProvided()
        {
            // Arrange
            var model = new UserFeedbackAppViewModel
            {
                ReservationAssignmentID = 123,
                FromID = 1,
                ToID = 2,
                Notes = "Feedback notes",
                Answer1 = 5,
                Answer2 = 4,
                Answer3 = 3,
                Answer4 = 5,
                Answer5 = 4,
                Answer6 = 3,
                CreatedDate = DateTime.UtcNow,
                CreatedUserID = 101
            };



            // Setup the mock ExecuteAsync method to simulate a successful stored procedure execution
            _contextMock
                .Setup(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(1);  // Simulate 1 row affected

            // Act
            var result = await _service.UserFeedback(model);

            // Assert
            Assert.Equal(0, result.Item1);  // Ensure the response code is correct
            Assert.Equal(string.Empty, result.Item2);  // Ensure the message is correct
        }

        [Fact]
        public async Task UserFeedback_ReturnsFailureResponse_WhenStoredProcFails()
        {
            // Arrange
            var model = new UserFeedbackAppViewModel
            {
                ReservationAssignmentID = 123,
                FromID = 1,
                ToID = 2,
                Notes = "Feedback notes",
                Answer1 = 5,
                Answer2 = 4,
                Answer3 = 3,
                Answer4 = 5,
                Answer5 = 4,
                Answer6 = 3,
                CreatedDate = DateTime.UtcNow,
                CreatedUserID = 101
            };

            var expectedResponseCode = 0;

            // Setup the mock ExecuteAsync method to simulate failure (response code 0)
            _contextMock
                .Setup(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .ReturnsAsync(0);  // Simulate no rows affected, which signifies failure

            // Act
            var result = await _service.UserFeedback(model);

            // Assert
            Assert.Equal(expectedResponseCode, result.Item1);  // Ensure the failure response code
            Assert.Equal(string.Empty, result.Item2);  // Ensure the failure message
        }

        [Fact]
        public async Task ViewUserFeedback_ReturnsFeedback_WhenDataExists()
        {
            // Arrange
            var model = new AssignmentIDAppViewModel { ReservationAssignmentID = 123 };
            var expectedFeedback = new List<UserFeedbackAppResponseModel>
        {
            new UserFeedbackAppResponseModel
            {
                ReservationAssignmentID = 123,
                FromID = 1,
                ToID = 2,
                Notes = "Feedback 1",
                Answer1 = 5,
                Answer2 = 4,
                Answer3 = 3,
                Answer4 = 5,
                Answer5 = 4,
                Answer6 = 3,
                CreatedDate = DateTime.UtcNow,
                CreatedUserID = 101
            },
            new UserFeedbackAppResponseModel
            {
                ReservationAssignmentID = 123,
                FromID = 2,
                ToID = 3,
                Notes = "Feedback 2",
                Answer1 = 4,
                Answer2 = 3,
                Answer3 = 4,
                Answer4 = 5,
                Answer5 = 4,
                Answer6 = 2,
                CreatedDate = DateTime.UtcNow,
                CreatedUserID = 102
            }
        };

            _contextMock.Setup(c => c.ExecuteQueryAsync<UserFeedbackAppResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    It.IsAny<CommandType>())
                )
                .ReturnsAsync(expectedFeedback);  // Mocking the result from the database

            // Act
            var result = await _service.ViewUserFeedback(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());  // Ensure 2 feedbacks are returned
            Assert.Equal(expectedFeedback, result.ToList());  // Ensure the returned list matches the expected feedback
        }

        [Fact]
        public async Task ViewUserFeedback_ReturnsEmpty_WhenNoDataExists()
        {
            // Arrange
            var model = new AssignmentIDAppViewModel { ReservationAssignmentID = 999 };

            _contextMock.Setup(c => c.ExecuteQueryAsync<UserFeedbackAppResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    It.IsAny<CommandType>())
                )
                .ReturnsAsync(new List<UserFeedbackAppResponseModel>());  // No feedback available

            // Act
            var result = await _service.ViewUserFeedback(model);

            // Assert
            Assert.Empty(result);  // Ensure the result is empty
        }

        [Fact]
        public async Task ViewUserFeedback_ExecutesQueryWithCorrectParameters()
        {
            // Arrange
            var model = new AssignmentIDAppViewModel { ReservationAssignmentID = 123 };
            var expectedQuery = "Select * from UsersFeedback where ReservationAssignmentID = @ReservationAssignmentID";

            // Act
            await _service.ViewUserFeedback(model);

            // Assert
            _contextMock.Verify(c => c.ExecuteQueryAsync<UserFeedbackAppResponseModel>(
                    It.Is<string>(s => s == expectedQuery),  // Check if the query is as expected
                    It.Is<DynamicParameters>(p => p.Get<int>(DbParams.ReservationAssignmentID) == model.ReservationAssignmentID),  // Verify the parameters
                    It.Is<CommandType>(ct => ct == CommandType.Text)),  // Ensure command type is Text
                Times.Once);  // Ensure ExecuteQueryAsync was called exactly once
        }

        [Fact]
        public async Task ReservationAssignmentSearch_WithValidParameters_ReturnsResults()
        {
            // Arrange
            var model = new ReservationAssignmentWebViewModel
            {
                ReservationID = 123,
                ReservationsAssignmentsID = 456,
                ContractorID = 789,
                DateFrom = "2024-01-01",
                DateTo = "2024-01-31",
                ClaimantID = 321,
                ClaimID = 654,
                CustomerID = 987,
                ASSGNCode = "ASSGN123",
                RSVACCode = "RSVAC123",
                AssignmentJobStatus = "Active",
                ContractorContactInfo = new List<string> { "contact1", "contact2" },
                inactiveflag = 0,
                ConAssignStatus = 1,
                Page = 1,
                Limit = 10
            };

            var expectedResponse = new List<vwReservationAssignmentsSearch>
        {
            new vwReservationAssignmentsSearch
            {
                reservationid = 123,
                ReservationsAssignmentsID = 456,
                contractorid = 789,
                AssignmentJobStatus = "Active",
                assgncode = "ASSGN123",
                RSVACCode = "RSVAC123"
            }
        };

            _contextMock.Setup(c => c.ExecuteQueryAsync<vwReservationAssignmentsSearch>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    It.IsAny<CommandType>())
                )
                .ReturnsAsync(expectedResponse);  // Mock the result from stored procedure

            // Act
            var result = await _service.ReservationAssignmentSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);  // Verify that exactly one record was returned
            Assert.Equal(expectedResponse, result.ToList());  // Verify the returned results match the expected response
        }

        [Fact]
        public async Task ReservationAssignmentSearch_WithInvalidPageAndLimit_ReturnsCorrectResults()
        {
            // Arrange
            var model = new ReservationAssignmentWebViewModel
            {
                ReservationID = 123,
                Page = -1,  // Invalid page number
                Limit = 0  // Invalid limit
            };

            var expectedResponse = new List<vwReservationAssignmentsSearch>
            {
                new vwReservationAssignmentsSearch
                {
                    reservationid = 123,
                    ReservationsAssignmentsID = 456,
                    contractorid = 789,
                    AssignmentJobStatus = "Active"
                }
            };

            _contextMock.Setup(c => c.ExecuteQueryAsync<vwReservationAssignmentsSearch>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    It.IsAny<CommandType>())
                )
                .ReturnsAsync(expectedResponse);  // Mock the result from stored procedure

            // Act
            var result = await _service.ReservationAssignmentSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);  // Verify that one result is returned despite invalid page/limit values
            Assert.Equal(expectedResponse, result.ToList());  // Verify that the results match expected data
        }

        [Fact]
        public async Task ReservationAssignmentSearch_VerifiesStoredProcExecutionWithCorrectParameters()
        {
            // Arrange
            var model = new ReservationAssignmentWebViewModel
            {
                ReservationID = 123,
                ReservationsAssignmentsID = 456,
                Page = 1,
                Limit = 10
            };

            var expectedQuery = ProcEntities.spReservationAssignmentsSearch;

            // Act
            await _service.ReservationAssignmentSearch(model);

            // Assert
            _contextMock.Verify(c => c.ExecuteQueryAsync<vwReservationAssignmentsSearch>(
                    It.Is<string>(s => s == expectedQuery),  // Verify the procedure name is correct
                    It.IsAny<DynamicParameters>(),
                    It.Is<CommandType>(ct => ct == CommandType.StoredProcedure)),
                Times.Once);  // Verify the stored procedure is invoked exactly once
        }

        [Fact]
        public async Task AssignAssignmentContractor_WithValidInput_ReturnsExpectedResult()
        {
            // Arrange
            var model = new AssignAssignmentContractorWebViewModel
            {
                reservationid = 123,
                reservationassignmentid = 456,
                contractorid = 789,
                AssignType = "Full",
                zipcode = "12345",
                miles = 10.5m,
                contractorname = "John Doe",
                company = "ABC Corp",
                CellPhone = "123-456-7890",
                contycode = "US",
                conctcode = "CT",
                conpccode = "PC",
                gender = "M",
                city = "New York",
                statecode = "NY",
                cstatus = "Active",
                constcode = "123",
                RatePerMiles = 15.5m,
                Cost = "200"
            };

            var expectedResult = new List<ReservationAssigncontractorWebResponseModel>
    {
        new ReservationAssigncontractorWebResponseModel
        {
            // Populate expected response model properties here
        }
    };

            _contextMock.Setup(c => c.ExecuteQueryAsync<ReservationAssigncontractorWebResponseModel>(
                    It.IsAny<string>(),
                    It.Is<DynamicParameters>(p =>
                        p.Get<int>(DbParams.ReservationID) == 123 &&
                        p.Get<int>(DbParams.ReservationAssignmentID) == 456 &&
                        p.Get<int>(DbParams.ContractorID) == 789 &&
                        p.Get<string>(DbParams.Type) == "Full" &&
                        p.Get<string>(DbParams.ZipCode) == "12345" &&
                        p.Get<decimal>(DbParams.Miles) == 10.5m &&
                        p.Get<string>(DbParams.ContractorName) == "John Doe" &&
                        p.Get<string>(DbParams.Company) == "ABC Corp" &&
                        p.Get<string>(DbParams.CellPhone) == "123-456-7890" &&
                        p.Get<string>(DbParams.ContyCode) == "US" &&
                        p.Get<string>(DbParams.ConctCode) == "CT" &&
                        p.Get<string>(DbParams.ConpcCode) == "PC" &&
                        p.Get<string>(DbParams.Gender) == "M" &&
                        p.Get<string>(DbParams.City) == "New York" &&
                        p.Get<string>(DbParams.StateCode) == "NY" &&
                        p.Get<string>(DbParams.Cstatus) == "Active" &&
                        p.Get<string>(DbParams.ConstCode) == "123" &&
                        p.Get<decimal>(DbParams.RatePerMiles) == 15.5m &&
                        p.Get<string>(DbParams.Cost) == "200"
                    ),
                    It.Is<CommandType>(ct => ct == CommandType.StoredProcedure)
                )
            ).ReturnsAsync(expectedResult);

            // Act
            var result = await _service.AssignAssignmentContractor(model);

            // Assert
            Assert.Equal(expectedResult, result);
            _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssigncontractorWebResponseModel>(
                    It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()),
                Times.Once);
        }

        [Fact]
        public async Task AssignAssignmentContractor_WithInvalidInput_ReturnsEmptyResult()
        {
            // Arrange
            var model = new AssignAssignmentContractorWebViewModel
            {
                reservationid = 0,  // Invalid value
                reservationassignmentid = 0,  // Invalid value
                contractorid = 789, // Valid
                AssignType = null, // Invalid value
                zipcode = "12345", // Valid
                miles = 10.5m,  // Valid
                contractorname = null, // Invalid
                                       // Other fields...
            };

            var expectedResult = new List<ReservationAssigncontractorWebResponseModel>(); // Empty result

            _contextMock.Setup(c => c.ExecuteQueryAsync<ReservationAssigncontractorWebResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    It.IsAny<CommandType>()
                )
            ).ReturnsAsync(expectedResult);

            // Act
            var result = await _service.AssignAssignmentContractor(model);

            // Assert
            Assert.Empty(result);
            _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssigncontractorWebResponseModel>(
                    It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()),
                Times.Once);
        }

        [Fact]
        public async Task AssignAssignmentContractor_WithNullOrEmptyParameters_ExecutesCorrectly()
        {
            // Arrange
            var model = new AssignAssignmentContractorWebViewModel
            {
                reservationid = 123,
                reservationassignmentid = 456,
                contractorid = 789,
                CellPhone = "",  // Empty string
                zipcode = null, // Null value
                miles = 10.5m, // Valid
                contractorname = "John Doe", // Valid
                                             // Other fields...
            };

            var expectedResult = new List<ReservationAssigncontractorWebResponseModel>(); // Expected empty result

            _contextMock.Setup(c => c.ExecuteQueryAsync<ReservationAssigncontractorWebResponseModel>(
                    It.IsAny<string>(),
                    It.Is<DynamicParameters>(p =>
                        // Assert that null or empty values are not added to the parameters
                        p.Get<string>(DbParams.CellPhone) == "" &&
                        p.Get<string>(DbParams.ZipCode) == null
                    ),
                    It.Is<CommandType>(ct => ct == CommandType.StoredProcedure)
                )
            ).ReturnsAsync(expectedResult);

            // Act
            var result = await _service.AssignAssignmentContractor(model);

            // Assert
            Assert.Empty(result);
            _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssigncontractorWebResponseModel>(
                    It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()),
                Times.Once);
        }

        [Fact]
        public async Task AssignAssignmentContractor_StoredProcedureCalledWithCorrectParameters()
        {
            // Arrange
            var model = new AssignAssignmentContractorWebViewModel
            {
                reservationid = 123,
                reservationassignmentid = 456,
                contractorid = 789,
                AssignType = "Full",
                zipcode = "12345",
                miles = 10.5m,
                contractorname = "John Doe",
                company = "ABC Corp",
                CellPhone = "123-456-7890",
                // Other fields...
            };

            var expectedResult = new List<ReservationAssigncontractorWebResponseModel>();

            _contextMock.Setup(c => c.ExecuteQueryAsync<ReservationAssigncontractorWebResponseModel>(
                    It.IsAny<string>(),
                    It.Is<DynamicParameters>(p =>
                        // Check that the parameters are added as expected
                        p.Get<int>(DbParams.ReservationID) == 123 &&
                        p.Get<int>(DbParams.ReservationAssignmentID) == 456 &&
                        p.Get<int>(DbParams.ContractorID) == 789
                    ),
                    It.Is<CommandType>(ct => ct == CommandType.StoredProcedure)
                )
            ).ReturnsAsync(expectedResult);

            // Act
            var result = await _service.AssignAssignmentContractor(model);

            // Assert
            _contextMock.Verify(c => c.ExecuteQueryAsync<ReservationAssigncontractorWebResponseModel>(
                    It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()),
                Times.Once);
        }

        [Fact]
        public async Task AssignAssignmentContractor_WhenExceptionOccurs_ReturnsEmptyResult()
        {
            // Arrange
            var model = new AssignAssignmentContractorWebViewModel
            {
                reservationid = 123,
                reservationassignmentid = 456,
                contractorid = 789,
                // other properties...
            };

            _contextMock.Setup(c => c.ExecuteQueryAsync<ReservationAssigncontractorWebResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    It.IsAny<CommandType>()
                )
            ).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.AssignAssignmentContractor(model));
        }

        [Fact]
        public async Task WebTrackAssignmentByID_WithNoMatchingData_ReturnsEmptyResult()
        {
            // Arrange
            var assignmentId = 999; // Non-existing assignment ID

            var expectedResponse = new List<TrackAssignmentByIDResponseModel>(); // Empty result

            _contextMock.Setup(c => c.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(
                    It.Is<string>(s => s.Contains("SELECT rat.*,ras.ReservationID,ras.Quantity")),
                    It.Is<DynamicParameters>(p => p.Get<int>(DbParams.ReservationAssignmentID) == assignmentId),
                    It.Is<CommandType>(ct => ct == CommandType.Text)
                )
            ).ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.WebTrackAssignmentByID(assignmentId);

            // Assert
            Assert.Empty(result);
            _contextMock.Verify(c => c.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(
                    It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()), Times.Once);
        }

        [Fact]
        public async Task WebTrackAssignmentByID_WhenDatabaseQueryFails_ThrowsException()
        {
            // Arrange
            var assignmentId = 123; // Valid assignment ID

            _contextMock.Setup(c => c.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(
                    It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()
                )
            ).ThrowsAsync(new Exception("Database query failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.WebTrackAssignmentByID(assignmentId));
            Assert.Equal("Database query failed", exception.Message);
        }

        [Fact]
        public async Task WebTrackAssignmentByID_WithInvalidAssignmentId_ReturnsEmptyResult()
        {
            // Arrange
            var assignmentId = 0; // Invalid assignment ID (zero)

            var expectedResponse = new List<TrackAssignmentByIDResponseModel>(); // Empty result

            _contextMock.Setup(c => c.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(
                    It.Is<string>(s => s.Contains("SELECT rat.*,ras.ReservationID,ras.Quantity")),
                    It.Is<DynamicParameters>(p => p.Get<int>(DbParams.ReservationAssignmentID) == assignmentId),
                    It.Is<CommandType>(ct => ct == CommandType.Text)
                )
            ).ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.WebTrackAssignmentByID(assignmentId);

            // Assert
            Assert.Empty(result);
            _contextMock.Verify(c => c.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(
                    It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()), Times.Once);
        }

        [Fact]
        public async Task WebTrackAssignmentByID_WithSQLInjectionInAssignmentId_ReturnsEmptyResult()
        {
            // Arrange
            var assignmentId = 123;

            var expectedResponse = new List<TrackAssignmentByIDResponseModel>(); // Empty result (as no match)

            _contextMock.Setup(c => c.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(
                    It.Is<string>(s => s.Contains("SELECT rat.*,ras.ReservationID,ras.Quantity")),
                    It.Is<DynamicParameters>(p => p.Get<int>(DbParams.ReservationAssignmentID) == assignmentId),
                    It.Is<CommandType>(ct => ct == CommandType.Text)
                )
            ).ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.WebTrackAssignmentByID(assignmentId);

            // Assert
            Assert.Empty(result);
            _contextMock.Verify(c => c.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(
                    It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()), Times.Once);
        }

        [Fact]
        public async Task SaveAssignmentDocument_ShouldReturnTrue_WhenResponseCodeIs1()
        {
            // Arrange
            var reservationsAssignmentsID = 123;
            var file = new UploadedFileInfo
            {
                ContentType = "application/pdf",
                FileName = "doc.pdf",
                FilePath = "/uploads/doc.pdf"
            };

            var parametersCaptured = new DynamicParameters();

            _contextMock.Setup(x => x.ExecuteAsync(
                "spInsertAssignmentMetricsDocument",
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure
            )).Callback<string, DynamicParameters, CommandType>((_, p, __) =>
            {
                parametersCaptured = p;
                p.Add("@ResponseCode", 1, DbType.Int32, ParameterDirection.Output);
                p.Add("@Msg", "Success", DbType.String, ParameterDirection.Output, size: 500);
            }).ReturnsAsync(1);

            // Act
            var result = await _service.SaveAssignmentDocument(reservationsAssignmentsID, file);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SaveAssignmentDocument_ShouldReturnFalse_WhenResponseCodeIsNot1()
        {
            // Arrange
            var reservationsAssignmentsID = 123;
            var file = new UploadedFileInfo
            {
                ContentType = "image/png",
                FileName = "error.png",
                FilePath = "/uploads/error.png"
            };

            var parametersCaptured = new DynamicParameters();

            _contextMock.Setup(x => x.ExecuteAsync(
                "spInsertAssignmentMetricsDocument",
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure
            )).Callback<string, DynamicParameters, CommandType>((_, p, __) =>
            {
                parametersCaptured = p;
                p.Add("@ResponseCode", 0, DbType.Int32, ParameterDirection.Output);
                p.Add("@Msg", "Error inserting document", DbType.String, ParameterDirection.Output, size: 500);
            }).ReturnsAsync(1);

            // Act
            var result = await _service.SaveAssignmentDocument(reservationsAssignmentsID, file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CheckReservationTimeSlotAsync_ShouldReturnSuccessResult_WhenStoredProcReturnsData()
        {
            // Arrange
            var model = new ReservationCheckTimeSlotViewModel
            {
                ReservationAssignmentID = 1,
                ReservationID = 2,
                ContractorID = 3
            };

            var responseModel = new ReservationCheckTimeSlotResponseModel
            {
                // Populate with dummy data if needed
            };

            var parametersCaptured = new DynamicParameters();

            _contextMock.Setup(x => x.ExecuteQueryFirstOrDefaultAsync<ReservationCheckTimeSlotResponseModel>(
                ProcEntities.spCheckReservationTimeSlot,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .Callback<string, DynamicParameters, CommandType>((_, p, __) =>
            {
                parametersCaptured = p;
                // Simulate output parameters
                p.Add(DbParams.ResponseCode, 1, DbType.Int32, ParameterDirection.Output);
                p.Add(DbParams.Msg, "Slot available", DbType.String, ParameterDirection.Output, size: 255);
            })
            .ReturnsAsync(responseModel);

            // Act
            var (responseCode, message, result) = await _service.CheckReservationTimeSlotAsync(model);

            // Assert
            Assert.Equal(1, responseCode);
            Assert.Equal("Slot available", message);
            Assert.NotNull(result);
            Assert.IsType<ReservationCheckTimeSlotResponseModel>(result);
        }

        [Fact]
        public async Task CheckReservationTimeSlotAsync_ShouldReturnDefaultModel_WhenStoredProcReturnsNull()
        {
            // Arrange
            var model = new ReservationCheckTimeSlotViewModel
            {
                ReservationAssignmentID = 1,
                ReservationID = 2,
                ContractorID = 3
            };

            var parametersCaptured = new DynamicParameters();

            _contextMock.Setup(x => x.ExecuteQueryFirstOrDefaultAsync<ReservationCheckTimeSlotResponseModel>(
                ProcEntities.spCheckReservationTimeSlot,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure))
            .Callback<string, DynamicParameters, CommandType>((_, p, __) =>
            {
                parametersCaptured = p;
                p.Add(DbParams.ResponseCode, 0, DbType.Int32, ParameterDirection.Output);
                p.Add(DbParams.Msg, "No slot found", DbType.String, ParameterDirection.Output, size: 255);
            })
            .ReturnsAsync(default(ReservationCheckTimeSlotResponseModel)); // Simulate null return
                                                                           // Simulate null return

            // Act
            var (responseCode, message, result) = await _service.CheckReservationTimeSlotAsync(model);

            // Assert
            Assert.Equal(0, responseCode);
            Assert.Equal("No slot found", message);
            Assert.NotNull(result);
            Assert.IsType<ReservationCheckTimeSlotResponseModel>(result);
        }

        [Fact]
        public async Task GetAllRelatedAssignmentsAsync_ReturnsData_WithValidReservationId()
        {
            // Arrange
            int reservationId = 101;
            int expectedResponseCode = 1;
            string expectedMessage = "Success";
            var expectedData = new List<ReservationAssignmentAppResponseModel>
            {
                new ReservationAssignmentAppResponseModel { ReservationsAssignmentsID = 1, reservationid = reservationId, contractor = "John Doe" },
                new ReservationAssignmentAppResponseModel { ReservationsAssignmentsID = 2, reservationid = reservationId, contractor = "Jane Smith" }
            };

            var mockParameters = new DynamicParameters();
            mockParameters.Add(DbParams.ReservationID, reservationId, DbType.Int32);
            mockParameters.Add(DbParams.ResponseCode, dbType: DbType.Int32, direction: ParameterDirection.Output);
            mockParameters.Add(DbParams.Msg, dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);
            mockParameters.Add(DbParams.ResponseCode, expectedResponseCode); // Simulate output value
            mockParameters.Add(DbParams.Msg, expectedMessage);               // Simulate output value

            _contextMock
                .Setup(ctx => ctx.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                    "spGetAllRelatedAssignments",
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedData)
                .Callback<string, DynamicParameters, CommandType>((_, dp, __) =>
                {
                    // Simulate output params being populated after stored proc execution
                    dp.Add(DbParams.ResponseCode, expectedResponseCode);
                    dp.Add(DbParams.Msg, expectedMessage);
                });

            // Act
            var (result, responseCode, message) = await _service.GetAllRelatedAssignmentsAsync(reservationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedResponseCode, responseCode);
            Assert.Equal(expectedMessage, message);
            Assert.Contains(result, r => r.contractor == "John Doe");

            _contextMock.Verify(ctx => ctx.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                "spGetAllRelatedAssignments",
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task CheckReservationAssignmentAsync_ReturnsExpectedResponse()
        {
            // Arrange
            int reservationAssignmentId = 123;
            string notes = "Test notes";

            var parametersCaptured = new DynamicParameters();
            string expectedMessage = "Reservation check successful";
            int expectedResponseCode = 1;

            // Mock the ExecuteAsync method to simulate stored procedure behavior
            _contextMock
                .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), It.IsAny<CommandType>()))
                .Callback<string, DynamicParameters, CommandType>((_, param, _) =>
                {
                    // Simulate output parameters
                    param.Add(DbParams.ResponseCode, expectedResponseCode, dbType: DbType.Int32, direction: ParameterDirection.Output);
                    param.Add(DbParams.Msg, expectedMessage, dbType: DbType.String, direction: ParameterDirection.Output);

                    // Ensure that we store the captured parameters for later assertions
                    param.Add(DbParams.ReservationsAssignmentsID, reservationAssignmentId, dbType: DbType.Int32);  // Ensure it's Int32 here
                    parametersCaptured = param;
                })
                .ReturnsAsync(1); // Mock the return value of ExecuteAsync

            // Act
            var result = await _service.CheckReservationAssignmentAsync(reservationAssignmentId, notes);

            // Assert
            Assert.Equal(expectedResponseCode, result.Item1);
            Assert.Equal(expectedMessage, result.Item2);

            // Also assert that correct params were added and are of the correct type
            Assert.Equal(reservationAssignmentId, parametersCaptured.Get<int>(DbParams.ReservationsAssignmentsID)); // Retrieve as Int32
            Assert.Equal(notes, parametersCaptured.Get<string>(DbParams.Notes));
        }

    }
}
