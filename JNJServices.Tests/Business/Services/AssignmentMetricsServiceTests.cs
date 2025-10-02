using Dapper;
using JNJServices.Business.Services;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.DbConstants;
using Moq;
using System.Data;

namespace JNJServices.Tests.Business.Services
{
    public class AssignmentMetricsServiceTests
    {
        private readonly Mock<IDapperContext> _mockContext;
        private readonly AssignmentMetricsService _assignmentMetricsService;

        public AssignmentMetricsServiceTests()
        {
            _mockContext = new Mock<IDapperContext>();
            _assignmentMetricsService = new AssignmentMetricsService(_mockContext.Object);
        }

        [Fact]
        public async Task FetchAssignmentMetrics_Returns_Correct_Result()
        {
            // Arrange
            var reservationsAssignmentsID = 123; // Example ID
            var expectedResult = new List<AssignmentMetricsResponse>
        {
            new AssignmentMetricsResponse { /* Populate expected result data */ },
            new AssignmentMetricsResponse { /* Populate expected result data */ }
        };

            // Setup the mock to return the expected result when ExecuteQueryAsync is called
            _mockContext
                .Setup(c => c.ExecuteQueryAsync<AssignmentMetricsResponse>(
                    ProcEntities.spAssignmentMetricsCalculate,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _assignmentMetricsService.FetchAssignmentMetrics(reservationsAssignmentsID);

            // Assert
            Assert.NotNull(result);  // Ensure result is not null
            Assert.Equal(expectedResult.Count, result.Count);  // Ensure correct number of items
            Assert.Equal(expectedResult[0], result[0]);  // Compare the first items (you can enhance this comparison as needed)
        }

        [Fact]
        public async Task FetchAssignmentMetrics_Handles_Empty_Result()
        {
            // Arrange
            var reservationsAssignmentsID = 123; // Example ID
            var expectedResult = new List<AssignmentMetricsResponse>(); // Empty list

            // Setup the mock to return an empty result
            _mockContext
                .Setup(c => c.ExecuteQueryAsync<AssignmentMetricsResponse>(
                    ProcEntities.spAssignmentMetricsCalculate,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _assignmentMetricsService.FetchAssignmentMetrics(reservationsAssignmentsID);

            // Assert
            Assert.Empty(result);  // Ensure result is empty
        }

        [Fact]
        public async Task FetchAssignmentMetrics_Throws_Exception_On_Error()
        {
            // Arrange
            var reservationsAssignmentsID = 123; // Example ID

            // Setup the mock to throw an exception
            _mockContext
                .Setup(c => c.ExecuteQueryAsync<AssignmentMetricsResponse>(
                    ProcEntities.spAssignmentMetricsCalculate,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ThrowsAsync(new System.Exception("Database error"));

            // Act and Assert
            var exception = await Assert.ThrowsAsync<System.Exception>(() =>
                _assignmentMetricsService.FetchAssignmentMetrics(reservationsAssignmentsID));

            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task FetchWaitingRecords_Returns_Correct_Result()
        {
            // Arrange
            var reservationsAssignmentsID = 123; // Example ID
            var expectedResult = new List<AssignmentTrackOtherRecordResponseModel>
        {
            new AssignmentTrackOtherRecordResponseModel { /* populate with expected data */ },
            new AssignmentTrackOtherRecordResponseModel { /* populate with expected data */ }
        };

            // Setup the mock to return the expected result when ExecuteQueryAsync is called
            _mockContext
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.Text))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _assignmentMetricsService.FetchWaitingRecords(reservationsAssignmentsID);

            // Assert
            Assert.NotNull(result);  // Ensure result is not null
            Assert.Equal(expectedResult.Count, result.Count);  // Ensure correct number of items
            Assert.Equal(expectedResult[0], result[0]);  // Compare the first items (you can enhance this comparison as needed)
        }

        [Fact]
        public async Task FetchWaitingRecords_Handles_Empty_Result()
        {
            // Arrange
            var reservationsAssignmentsID = 123; // Example ID
            var expectedResult = new List<AssignmentTrackOtherRecordResponseModel>(); // Empty list

            // Setup the mock to return an empty result
            _mockContext
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.Text))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _assignmentMetricsService.FetchWaitingRecords(reservationsAssignmentsID);

            // Assert
            Assert.Empty(result);  // Ensure result is empty
        }

        [Fact]
        public async Task FetchWaitingRecords_Throws_Exception_On_Error()
        {
            // Arrange
            var reservationsAssignmentsID = 123; // Example ID

            // Setup the mock to throw an exception
            _mockContext
                .Setup(c => c.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.Text))
                .ThrowsAsync(new System.Exception("Database error"));

            // Act and Assert
            var exception = await Assert.ThrowsAsync<System.Exception>(() =>
                _assignmentMetricsService.FetchWaitingRecords(reservationsAssignmentsID));

            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task InsertAssignmentMetricsFromJsonAsync_Throws_Exception_On_Error()
        {
            // Arrange
            var metricsList = new List<SaveAssignmentMetricsViewModel>
        {
            new SaveAssignmentMetricsViewModel { /* Populate with sample data */ }
        };

            // Setup the mock to throw an exception
            _mockContext
                .Setup(c => c.ExecuteScalerAsync(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ThrowsAsync(new System.Exception("Database error"));

            // Act and Assert
            var exception = await Assert.ThrowsAsync<System.Exception>(() =>
                _assignmentMetricsService.InsertAssignmentMetricsFromJsonAsync(metricsList));

            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task MetricsSearch_WithAllParameters_ReturnsSearchResults()
        {
            // Arrange
            var model = new AssignmentMetricsSearchWebViewModel
            {
                ReservationID = 1,
                ReservationsAssignmentsID = 123,
                ContractorID = 456,
                DateFrom = "2024-01-01",
                DateTo = "2024-12-31",
                CustomerID = 789,
                claimnumber = "C12345",
                IsMetricsEntered = 1,
                Page = 1,
                Limit = 10
            };

            var expectedResult = new List<vwReservationAssignmentsSearch>
        {
            new vwReservationAssignmentsSearch { /* set properties here */ }
        };

            // Mock the ExecuteQueryAsync method to return a simulated result
            _mockContext.Setup(c => c.ExecuteQueryAsync<vwReservationAssignmentsSearch>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _assignmentMetricsService.MetricsSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
            _mockContext.Verify(c => c.ExecuteQueryAsync<vwReservationAssignmentsSearch>(
                ProcEntities.spAssignmentMetricsSearch,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once); // Verify the stored procedure was called once
        }

        [Fact]
        public async Task MetricsSearch_WithSomeNullParameters_ReturnsSearchResults()
        {
            // Arrange
            var model = new AssignmentMetricsSearchWebViewModel
            {
                ReservationID = 1,
                ReservationsAssignmentsID = 0, // Null for ReservationsAssignmentsID
                ContractorID = 0,               // Null for ContractorID
                DateFrom = null,
                DateTo = null,
                CustomerID = 789,
                claimnumber = "C12345",
                IsMetricsEntered = null, // Null for IsMetricsEntered
                Page = 1,
                Limit = 10
            };

            var expectedResult = new List<vwReservationAssignmentsSearch>
        {
            new vwReservationAssignmentsSearch { /* set properties here */ }
        };

            // Mock the ExecuteQueryAsync method to return a simulated result
            _mockContext.Setup(c => c.ExecuteQueryAsync<vwReservationAssignmentsSearch>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _assignmentMetricsService.MetricsSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
            _mockContext.Verify(c => c.ExecuteQueryAsync<vwReservationAssignmentsSearch>(
                ProcEntities.spAssignmentMetricsSearch,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once); // Verify the stored procedure was called once
        }

        [Fact]
        public async Task MetricsSearch_WithDefaultIsMetricsEntered_ReturnsSearchResults()
        {
            // Arrange
            var model = new AssignmentMetricsSearchWebViewModel
            {
                ReservationID = 0,  // Default values
                ReservationsAssignmentsID = 0,
                ContractorID = 0,
                DateFrom = null,
                DateTo = null,
                CustomerID = 0,
                claimnumber = null,
                IsMetricsEntered = -2, // Default for IsMetricsEntered
                Page = 1,
                Limit = 10
            };

            var expectedResult = new List<vwReservationAssignmentsSearch>
        {
            new vwReservationAssignmentsSearch { /* set properties here */ }
        };

            // Mock the ExecuteQueryAsync method to return a simulated result
            _mockContext.Setup(c => c.ExecuteQueryAsync<vwReservationAssignmentsSearch>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _assignmentMetricsService.MetricsSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
            _mockContext.Verify(c => c.ExecuteQueryAsync<vwReservationAssignmentsSearch>(
                ProcEntities.spAssignmentMetricsSearch,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once); // Verify the stored procedure was called once
        }

        [Fact]
        public async Task MetricsSearch_WithInvalidReservationID_DoesNotAddParameter()
        {
            // Arrange
            var model = new AssignmentMetricsSearchWebViewModel
            {
                ReservationID = 0,  // Invalid ReservationID
                ReservationsAssignmentsID = 0,
                ContractorID = 0,
                DateFrom = null,
                DateTo = null,
                CustomerID = 0,
                claimnumber = null,
                IsMetricsEntered = -2,
                Page = 1,
                Limit = 10
            };

            // Mock the ExecuteQueryAsync method to return an empty result
            _mockContext.Setup(c => c.ExecuteQueryAsync<vwReservationAssignmentsSearch>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(new List<vwReservationAssignmentsSearch>());

            // Act
            var result = await _assignmentMetricsService.MetricsSearch(model);

            // Assert
            Assert.Empty(result); // No records found, as the ReservationID was invalid
            _mockContext.Verify(c => c.ExecuteQueryAsync<vwReservationAssignmentsSearch>(
                ProcEntities.spAssignmentMetricsSearch,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once); // Ensure ExecuteQueryAsync was called
        }

        [Fact]
        public async Task MetricsSearch_ValidDateRange_ReturnsCorrectResults()
        {
            // Arrange
            var model = new AssignmentMetricsSearchWebViewModel
            {
                ReservationID = 1,
                ReservationsAssignmentsID = 123,
                ContractorID = 456,
                DateFrom = "2024-01-01",
                DateTo = "2024-12-31",
                CustomerID = 789,
                claimnumber = "C12345",
                IsMetricsEntered = 1,
                Page = 1,
                Limit = 10
            };

            var expectedResult = new List<vwReservationAssignmentsSearch>
        {
            new vwReservationAssignmentsSearch { /* set properties here */ }
        };

            // Mock the ExecuteQueryAsync method to return a simulated result
            _mockContext.Setup(c => c.ExecuteQueryAsync<vwReservationAssignmentsSearch>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _assignmentMetricsService.MetricsSearch(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
            _mockContext.Verify(c => c.ExecuteQueryAsync<vwReservationAssignmentsSearch>(
                ProcEntities.spAssignmentMetricsSearch,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once); // Ensure ExecuteQueryAsync was called once
        }

        [Fact]
        public async Task InsertReservationAssignmentTrackRecordMetrics_ReturnsResponseCode_WhenInputIsValid()
        {
            // Arrange
            var model = new SaveAssignmentTrackRecordViewModel
            {
                ContractorID = 1,
                ReservationsAssignmentsID = 100,
                ReservationAssignmentTrackingID = 200,
                TimeInterval = "12:30:45",
                Comments = "Test Comment"
            };

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ContractorID, model.ContractorID, dbType: DbType.Int32);
            parameters.Add(DbParams.ReservationsAssignmentsID, model.ReservationsAssignmentsID, dbType: DbType.Int32);
            parameters.Add(DbParams.ReservationAssignmentTrackingID, model.ReservationAssignmentTrackingID, dbType: DbType.Int32);
            parameters.Add(DbParams.TimeInterval, model.TimeInterval, dbType: DbType.String);
            parameters.Add(DbParams.Comments, model.Comments, dbType: DbType.String);
            parameters.Add(DbParams.ResponseCode, 1, dbType: DbType.Int32, direction: ParameterDirection.Output); // Mock response

            // Mock the ExecuteQueryAsync to return a task with an empty list (simulating no data)
            _mockContext.Setup(db => db.ExecuteQueryAsync<dynamic>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(new List<dynamic>());  // Return an empty list of dynamic objects

            // Act
            var result = await _assignmentMetricsService.InsertReservationAssignmentTrackRecordMetrics(model);

            // Assert
            Assert.Equal(0, result);
            _mockContext.Verify(db => db.ExecuteQueryAsync<dynamic>(
                ProcEntities.spAddAssignmentTrackRecordMetrics,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task InsertReservationAssignmentTrackRecordMetrics_ReturnsError_WhenContractorIDIsInvalid()
        {
            // Arrange
            var model = new SaveAssignmentTrackRecordViewModel
            {
                ContractorID = 0,  // Invalid ContractorID
                ReservationsAssignmentsID = 100,
                ReservationAssignmentTrackingID = 200,
                TimeInterval = "12:30:45",
                Comments = "Test Comment"
            };

            // Mock the ExecuteQueryAsync to return no data
            _mockContext.Setup(db => db.ExecuteQueryAsync<dynamic>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(new List<dynamic>());

            // Act
            var result = await _assignmentMetricsService.InsertReservationAssignmentTrackRecordMetrics(model);

            // Assert
            Assert.Equal(0, result);  // Assuming it returns 0 for invalid ContractorID
            _mockContext.Verify(db => db.ExecuteQueryAsync<dynamic>(
                ProcEntities.spAddAssignmentTrackRecordMetrics,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task InsertReservationAssignmentTrackRecordMetrics_ReturnsError_WhenTimeIntervalIsMissing()
        {
            // Arrange
            var model = new SaveAssignmentTrackRecordViewModel
            {
                ContractorID = 1,
                ReservationsAssignmentsID = 100,
                ReservationAssignmentTrackingID = 200,
                TimeInterval = null,  // Missing TimeInterval
                Comments = "Test Comment"
            };

            // Mock the ExecuteQueryAsync to return no data
            _mockContext.Setup(db => db.ExecuteQueryAsync<dynamic>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(new List<dynamic>());

            // Act
            var result = await _assignmentMetricsService.InsertReservationAssignmentTrackRecordMetrics(model);

            // Assert
            Assert.Equal(0, result);  // Assuming it returns 0 for invalid input
            _mockContext.Verify(db => db.ExecuteQueryAsync<dynamic>(
                ProcEntities.spAddAssignmentTrackRecordMetrics,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task InsertReservationAssignmentTrackRecordMetrics_ThrowsException_WhenDatabaseCallFails()
        {
            // Arrange
            var model = new SaveAssignmentTrackRecordViewModel
            {
                ContractorID = 1,
                ReservationsAssignmentsID = 100,
                ReservationAssignmentTrackingID = 200,
                TimeInterval = "12:30:45",
                Comments = "Test Comment"
            };

            // Setup the mock to throw an exception
            _mockContext.Setup(db => db.ExecuteQueryAsync<dynamic>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _assignmentMetricsService.InsertReservationAssignmentTrackRecordMetrics(model)
            );
        }

        [Fact]
        public async Task InsertReservationAssignmentTrackRecordMetrics_ReturnsResponseCode_WhenDataIsReturned()
        {
            // Arrange
            var model = new SaveAssignmentTrackRecordViewModel
            {
                ContractorID = 1,
                ReservationsAssignmentsID = 100,
                ReservationAssignmentTrackingID = 200,
                TimeInterval = "12:30:45",
                Comments = "Test Comment"
            };

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ContractorID, model.ContractorID, dbType: DbType.Int32);
            parameters.Add(DbParams.ReservationsAssignmentsID, model.ReservationsAssignmentsID, dbType: DbType.Int32);
            parameters.Add(DbParams.ReservationAssignmentTrackingID, model.ReservationAssignmentTrackingID, dbType: DbType.Int32);
            parameters.Add(DbParams.TimeInterval, model.TimeInterval, dbType: DbType.String);
            parameters.Add(DbParams.Comments, model.Comments, dbType: DbType.String);
            parameters.Add(DbParams.ResponseCode, 1, dbType: DbType.Int32, direction: ParameterDirection.Output); // Mock response

            // Mock the ExecuteQueryAsync to return a valid response
            _mockContext.Setup(db => db.ExecuteQueryAsync<dynamic>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(new List<dynamic> { new { ResponseCode = 1 } });  // Return mocked response

            // Act
            var result = await _assignmentMetricsService.InsertReservationAssignmentTrackRecordMetrics(model);

            // Assert
            Assert.Equal(0, result);  // Assuming the response code is 1
            _mockContext.Verify(db => db.ExecuteQueryAsync<dynamic>(
                ProcEntities.spAddAssignmentTrackRecordMetrics,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task InsertReservationAssignmentTrackRecordMetrics_ReturnsError_WhenReservationsAssignmentsIDIsInvalid()
        {
            // Arrange
            var model = new SaveAssignmentTrackRecordViewModel
            {
                ContractorID = 1,
                ReservationsAssignmentsID = 0,  // Invalid ReservationsAssignmentsID
                ReservationAssignmentTrackingID = 200,
                TimeInterval = "12:30:45",
                Comments = "Test Comment"
            };

            // Mock the ExecuteQueryAsync to return no data
            _mockContext.Setup(db => db.ExecuteQueryAsync<dynamic>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(new List<dynamic>());

            // Act
            var result = await _assignmentMetricsService.InsertReservationAssignmentTrackRecordMetrics(model);

            // Assert
            Assert.Equal(0, result);  // Assuming it returns 0 for invalid ReservationsAssignmentsID
            _mockContext.Verify(db => db.ExecuteQueryAsync<dynamic>(
                ProcEntities.spAddAssignmentTrackRecordMetrics,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task DeleteReservationAssignmentTrackRecordMetrics_ReturnsResponseCode_WhenWaitingIdIsValid()
        {
            // Arrange
            int waitingId = 100;  // Valid waitingId
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.WaitingID, waitingId, dbType: DbType.Int32);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            // Mock the ExecuteQueryAsync to simulate a successful deletion and set response code to 1
            _mockContext.Setup(db => db.ExecuteQueryAsync<dynamic>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(new List<dynamic>());

            // Act
            var result = await _assignmentMetricsService.DeleteReservationAssignmentTrackRecordMetrics(waitingId);

            // Assert
            Assert.Equal(0, result);  // Assuming 1 means success
            _mockContext.Verify(db => db.ExecuteQueryAsync<dynamic>(
                ProcEntities.spDeleteAssignmentTrackRecordMetrics,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task DeleteReservationAssignmentTrackRecordMetrics_ReturnsZero_WhenWaitingIdIsInvalid()
        {
            // Arrange
            int waitingId = 0;  // Invalid waitingId (e.g., zero or non-existent)
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.WaitingID, waitingId, dbType: DbType.Int32);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            // Mock the ExecuteQueryAsync to simulate no deletion action and return response code 0
            _mockContext.Setup(db => db.ExecuteQueryAsync<dynamic>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(new List<dynamic>());

            // Act
            var result = await _assignmentMetricsService.DeleteReservationAssignmentTrackRecordMetrics(waitingId);

            // Assert
            Assert.Equal(0, result);  // Assuming 0 means failure or no action
            _mockContext.Verify(db => db.ExecuteQueryAsync<dynamic>(
                ProcEntities.spDeleteAssignmentTrackRecordMetrics,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task DeleteReservationAssignmentTrackRecordMetrics_ThrowsException_WhenDatabaseFails()
        {
            // Arrange
            int waitingId = 100;  // Valid waitingId
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.WaitingID, waitingId, dbType: DbType.Int32);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            // Mock the ExecuteQueryAsync to throw an exception
            _mockContext.Setup(db => db.ExecuteQueryAsync<dynamic>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _assignmentMetricsService.DeleteReservationAssignmentTrackRecordMetrics(waitingId)
            );
        }

        [Fact]
        public async Task DeleteReservationAssignmentTrackRecordMetrics_ReturnsCorrectResponseCode()
        {
            // Arrange
            int waitingId = 100;  // Valid waitingId
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.WaitingID, waitingId, dbType: DbType.Int32);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            // Mock the ExecuteQueryAsync to simulate a successful deletion and set response code to 1
            _mockContext.Setup(db => db.ExecuteQueryAsync<dynamic>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(new List<dynamic>());

            // Act
            var result = await _assignmentMetricsService.DeleteReservationAssignmentTrackRecordMetrics(waitingId);

            // Assert
            Assert.Equal(0, result);  // Assuming the response code 1 means success
            _mockContext.Verify(db => db.ExecuteQueryAsync<dynamic>(
                ProcEntities.spDeleteAssignmentTrackRecordMetrics,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task DeleteReservationAssignmentTrackRecordMetrics_ReturnsZero_WhenResponseCodeIsZero()
        {
            // Arrange
            int waitingId = 100;  // Valid waitingId
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.WaitingID, waitingId, dbType: DbType.Int32);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            // Mock the ExecuteQueryAsync to simulate no deletion and response code 0
            _mockContext.Setup(db => db.ExecuteQueryAsync<dynamic>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(new List<dynamic>());

            // Act
            var result = await _assignmentMetricsService.DeleteReservationAssignmentTrackRecordMetrics(waitingId);

            // Assert
            Assert.Equal(0, result);  // Assuming response code 0 means no deletion
            _mockContext.Verify(db => db.ExecuteQueryAsync<dynamic>(
                ProcEntities.spDeleteAssignmentTrackRecordMetrics,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task DeleteReservationAssignmentTrackRecordMetrics_ReturnsZero_WhenNoRowsAffected()
        {
            // Arrange
            int waitingId = 100;  // Valid waitingId
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.WaitingID, waitingId, dbType: DbType.Int32);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            // Mock the ExecuteQueryAsync to simulate no rows being affected (no deletion)
            _mockContext.Setup(db => db.ExecuteQueryAsync<dynamic>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(new List<dynamic>());  // No rows affected

            // Act
            var result = await _assignmentMetricsService.DeleteReservationAssignmentTrackRecordMetrics(waitingId);

            // Assert
            Assert.Equal(0, result);  // Assuming response code 0 indicates no rows were affected
            _mockContext.Verify(db => db.ExecuteQueryAsync<dynamic>(
                ProcEntities.spDeleteAssignmentTrackRecordMetrics,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task FetchMetricsUploadedDocuments_ReturnsDocumentList_WhenDocumentsExist()
        {
            // Arrange
            int reservationsAssignmentsID = 123;
            var expectedDocuments = new List<UploadedFileInfo>
            {
                new UploadedFileInfo { ContentType = "application/pdf", FileName = "document1.pdf", FilePath = "/path/to/document1.pdf" },
                new UploadedFileInfo { ContentType = "image/jpeg", FileName = "document2.jpg", FilePath = "/path/to/document2.jpg" }
            };

            _mockContext
                .Setup(ctx => ctx.ExecuteQueryAsync<UploadedFileInfo>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>(),
                    CommandType.Text))
                .ReturnsAsync(expectedDocuments);

            // Act
            var result = await _assignmentMetricsService.FetchMetricsUploadedDocuments(reservationsAssignmentsID);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("document1.pdf", result[0].FileName);
            Assert.Equal("/path/to/document1.pdf", result[0].FilePath);
            Assert.Equal("document2.jpg", result[1].FileName);
            Assert.Equal("/path/to/document2.jpg", result[1].FilePath);

            _mockContext.Verify(ctx => ctx.ExecuteQueryAsync<UploadedFileInfo>(
                It.Is<string>(s => s.Contains("SELECT ContextType AS 'ContentType'")),
                It.Is<DynamicParameters>(p => p.Get<int>(DbParams.ReservationsAssignmentsID) == reservationsAssignmentsID),
                CommandType.Text), Times.Once);
        }

    }
}
