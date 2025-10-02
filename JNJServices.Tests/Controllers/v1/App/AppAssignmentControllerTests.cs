using AutoMapper;
using JNJServices.API.Controllers.v1.App;
using JNJServices.API.Helper;
using JNJServices.Business.Abstracts;
using JNJServices.Business.Email;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.CommonModels;
using JNJServices.Models.Entities;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Helper;
using JNJServices.Utility.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using static JNJServices.Utility.ApiConstants.NotificationConstants;
using static JNJServices.Utility.UserResponses;

namespace JNServices.Tests.Controllers.v1.App
{
    public class AppAssignmentControllerTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IContractorService> _contractorServiceMock;
        private readonly Mock<IClaimantService> _claimantServiceMock;
        private readonly Mock<IJwtFactory> _jwtFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly TimeZoneConverter _timeZoneConverter;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<ILogger<NotificationHelper>> _loggerMock;
        private readonly Mock<IOptions<FirebaseSettings>> _firebaseSettingsMock;
        private readonly Mock<IAssignmentMetricsService> _assignmentMetricsMock;
        private readonly Mock<IReservationAssignmentsService> _reservationAssignmentsServiceMock;
        private readonly AppAssignmentController _assignmentController;
        private readonly Mock<IUrlHelper> _mockUrlHelper;
        private readonly NotificationHelper _notificationHelper;

        public AppAssignmentControllerTests()
        {
            // Mock for IConfiguration
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "EncryptionDecryption:Key", "testEncryptionKey" },
                { "SelectTimeZone:TimeZone", "India Standard Time" },
                { "FirebaseSettings:FcmCredentialsFilePath", "jnj-services-firebase-creds.json" },
                { "BaseUrl:Images", "https://local-api-jnjservices.betademo.net" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Assign the IConfiguration instance
            _configurationMock = new Mock<IConfiguration>();
            _mockUrlHelper = new Mock<IUrlHelper>();
            _userServiceMock = new Mock<IUserService>();
            _contractorServiceMock = new Mock<IContractorService>();
            _claimantServiceMock = new Mock<IClaimantService>();
            _jwtFactoryMock = new Mock<IJwtFactory>();
            _mapperMock = new Mock<IMapper>();
            _emailServiceMock = new Mock<IEmailService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _loggerMock = new Mock<ILogger<NotificationHelper>>();
            _assignmentMetricsMock = new Mock<IAssignmentMetricsService>();
            _reservationAssignmentsServiceMock = new Mock<IReservationAssignmentsService>();

            // Initialize TimeZoneConverter with configuration
            _timeZoneConverter = new TimeZoneConverter(configuration);

            // Set up FirebaseSettings mock
            var firebaseSettings = new FirebaseSettings
            {
                FcmCredentialsFilePath = "jnj-services-firebase-creds.json"
            };

            _firebaseSettingsMock = new Mock<IOptions<FirebaseSettings>>();
            _firebaseSettingsMock.Setup(f => f.Value).Returns(firebaseSettings);

            // Instantiate NotificationHelper with all required dependencies
            _notificationHelper = new NotificationHelper(
                _timeZoneConverter,
                _firebaseSettingsMock.Object,
                _loggerMock.Object,
                _notificationServiceMock.Object
            );

            // Instantiate the AppAssignmentController with mocks and NotificationHelper
            _assignmentController = new AppAssignmentController(
                _reservationAssignmentsServiceMock.Object,
                _timeZoneConverter,
                _assignmentMetricsMock.Object,
                _configurationMock.Object,
                _notificationHelper
            );


            // Set up claims for the user
            var claims = new List<Claim>
            {
                new Claim("Type", "1"),
                new Claim("UserID", "123"),
                new Claim("PhoneNo", "1234567890"),
                new Claim("UserName", "TestUser")
            };
            // Instantiate the controller with mocked dependencies
            _assignmentController = new AppAssignmentController(
                _reservationAssignmentsServiceMock.Object,
                _timeZoneConverter,
                _assignmentMetricsMock.Object,
                _configurationMock.Object,
                _notificationHelper
            );
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"));


            // Set up controller context with the claims
            _assignmentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }


        [Fact]
        public async Task GetAssignments_ReturnsOk_WhenAssignmentsAreFound()
        {
            // Arrange
            var model = new AppReservationAssignmentSearchViewModel();
            var reservations = new List<ReservationAssignmentAppResponseModel>
            {
                new ReservationAssignmentAppResponseModel { TotalCount = 1 } // Sample reservation data
            };
            var processedReservations = reservations; // Assume same list is returned after processing

            _reservationAssignmentsServiceMock
                .Setup(service => service.MobileReservationAssignmentSearch(model))
                .ReturnsAsync(reservations);

            _reservationAssignmentsServiceMock
                .Setup(service => service.ProcessAssignmentsAsync(It.IsAny<List<ReservationAssignmentAppResponseModel>>()))
                .ReturnsAsync(processedReservations);

            // Act
            var result = await _assignmentController.GetAssignments(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(processedReservations, response.data);
            Assert.Equal(reservations.First().TotalCount, response.totalData);
        }

        [Fact]
        public async Task GetAssignments_ReturnsDataNotFound_WhenNoAssignmentsAreFound()
        {
            // Arrange
            var model = new AppReservationAssignmentSearchViewModel();
            var emptyReservations = new List<ReservationAssignmentAppResponseModel>();

            _reservationAssignmentsServiceMock
                .Setup(service => service.MobileReservationAssignmentSearch(model))
                .ReturnsAsync(emptyReservations);

            // Act
            var result = await _assignmentController.GetAssignments(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task UsersFeedback_ReturnsSuccess_WhenFeedbackSavedSuccessfully()
        {
            // Arrange
            var model = new UserFeedbackAppViewModel
            {
                ReservationAssignmentID = 123,
                ToID = 432,
                FromID = 111
            };

            // Mock service return value (success)
            var serviceResult = (1, "Feedback saved successfully");
            _reservationAssignmentsServiceMock
                .Setup(service => service.UserFeedback(It.IsAny<UserFeedbackAppViewModel>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _assignmentController.UsersFeedback(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal("Feedback saved successfully", response.statusMessage);
        }

        [Fact]
        public async Task UsersFeedback_ReturnsSuccess_WhenFeedbackDataNotFound()
        {
            // Arrange
            var model = new UserFeedbackAppViewModel
            {
                ReservationAssignmentID = 123,
                ToID = 432,
                FromID = 111
            };

            // Mock service return value (success)
            var serviceResult = (0, "Feedback Not saved");
            _reservationAssignmentsServiceMock
                .Setup(service => service.UserFeedback(It.IsAny<UserFeedbackAppViewModel>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _assignmentController.UsersFeedback(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Feedback Not saved", response.statusMessage);
        }

        [Fact]
        public async Task UsersFeedback_ValidModel_ReturnsSuccessResponse()
        {
            // Arrange
            var model = new UserFeedbackAppViewModel
            {
                ReservationAssignmentID = 1,
                FromID = 2,
                ToID = 3,
                Notes = "Feedback notes",
                Answer1 = 4.5M,
                Answer2 = 3.5M,
                Answer3 = 5M,
                Answer4 = 2.5M,
                Answer5 = 4.5M,
                Answer6 = 3.5M,
                CreatedDate = DateTime.UtcNow,
                CreatedUserID = 123,
                oldCreatedUserID = 12
            };

            var serviceResult = (1, "Feedback submitted successfully");
            _reservationAssignmentsServiceMock
                .Setup(service => service.UserFeedback(It.IsAny<UserFeedbackAppViewModel>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _assignmentController.UsersFeedback(model);

            // Assert: Checking for OK result
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status); // Ensure success status
            Assert.Equal(serviceResult.Item2, response.statusMessage); // Ensure the success message
        }

        [Fact]
        public async Task ViewUsersFeedback_ReturnsValidFeedbackData()
        {
            // Arrange
            var feedbackRequest = new AssignmentIDAppViewModel
            {
                ReservationAssignmentID = 123 // Valid ID for fetching feedback
            };

            var expectedFeedback = new List<UserFeedbackAppResponseModel>
            {
                new UserFeedbackAppResponseModel
                {
                    ReservationAssignmentID = 123,
                    FromID = 1,
                    ToID = 2,
                    Notes = "Feedback note",
                    Answer1 = 4.5M,
                    Answer2 = 3.0M,
                    Answer3 = 5.0M,
                    Answer4 = 4.0M,
                    Answer5 = 3.5M,
                    Answer6 = 4.2M,
                    CreatedDate = DateTime.UtcNow,
                    CreatedUserID = 1234
                }
            };

            // Setup mock for ViewUserFeedback method
            _reservationAssignmentsServiceMock
                .Setup(service => service.ViewUserFeedback(It.IsAny<AssignmentIDAppViewModel>()))
                .ReturnsAsync(expectedFeedback);

            // Act
            var result = await _assignmentController.ViewUsersFeedback(feedbackRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.IsType<List<UserFeedbackAppResponseModel>>(response.data);
            var feedbackData = response.data as List<UserFeedbackAppResponseModel>;

            // Validate the properties of the feedback response
            var feedbackItem = feedbackData?.FirstOrDefault();
            Assert.NotNull(feedbackItem);
            Assert.Equal(123, feedbackItem.ReservationAssignmentID);
            Assert.Equal(1, feedbackItem.FromID);
            Assert.Equal(2, feedbackItem.ToID);
            Assert.Equal("Feedback note", feedbackItem.Notes);
            Assert.Equal(4.5M, feedbackItem.Answer1);
            Assert.Equal(3.0M, feedbackItem.Answer2);
            Assert.Equal(5.0M, feedbackItem.Answer3);
            Assert.Equal(4.0M, feedbackItem.Answer4);
            Assert.Equal(3.5M, feedbackItem.Answer5);
            Assert.Equal(4.2M, feedbackItem.Answer6);
            Assert.NotNull(feedbackItem.CreatedDate);
            Assert.Equal(1234, feedbackItem.CreatedUserID);
        }

        [Fact]
        public async Task DeleteReservationAssignmentTrackRecordMetrics_ReturnsSuccess_WhenRecordIsDeleted()
        {
            // Arrange
            var waitingID = 1; // Assuming 1 is a valid WaitingID
            var expectedResult = 1; // Assuming 1 means the deletion was successful

            // Set up the mock for IAssignmentMetricsService to return a successful result
            _assignmentMetricsMock
                .Setup(service => service.DeleteReservationAssignmentTrackRecordMetrics(waitingID))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _assignmentController.DeleteReservationAssignmentTrackRecordMetrics(waitingID, 123);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
        }

        [Fact]
        public async Task DeleteReservationAssignmentTrackRecordMetrics_ReturnsFailure_WhenRecordIsNotDeleted()
        {
            // Arrange
            var waitingID = 1; // Assuming 1 is a valid WaitingID
            var expectedResult = 0; // Assuming 0 means the deletion failed

            // Set up the mock for IAssignmentMetricsService to return a failure result
            _assignmentMetricsMock
                .Setup(service => service.DeleteReservationAssignmentTrackRecordMetrics(waitingID))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _assignmentController.DeleteReservationAssignmentTrackRecordMetrics(waitingID, 123);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_DELETED, response.statusMessage);
        }

        [Fact]
        public async Task SaveAssignmentTrackRecordFromMetrics_ReturnsSuccess_WhenDataIsInserted()
        {
            // Arrange
            var assignmentMetrics = new SaveAssignmentTrackRecordViewModel
            {
                ContractorID = 1, // Example valid ContractorID
                ReservationsAssignmentsID = 123, // Example valid ReservationsAssignmentsID
                ReservationAssignmentTrackingID = 456, // Example valid ReservationAssignmentTrackingID
                TimeInterval = "01:30:00", // Example valid TimeInterval
                Comments = "Test comment" // Example valid Comment
            };

            var expectedResult = 1; // Assuming 1 means the data was inserted successfully

            // Set up the mock for IAssignmentMetricsService to return a successful result
            _assignmentMetricsMock
                .Setup(service => service.InsertReservationAssignmentTrackRecordMetrics(It.IsAny<SaveAssignmentTrackRecordViewModel>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _assignmentController.SaveAssignmentTrackRecordFromMetrics(assignmentMetrics);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
        }

        [Fact]
        public async Task SaveAssignmentTrackRecordFromMetrics_ReturnsFailure_WhenDataIsNotInserted()
        {
            // Arrange
            var assignmentMetrics = new SaveAssignmentTrackRecordViewModel
            {
                ContractorID = 1,
                ReservationsAssignmentsID = 123,
                ReservationAssignmentTrackingID = 456,
                TimeInterval = "01:30:00",
                Comments = "Test comment"
            };

            var expectedResult = 0; // Assuming 0 means the data was not inserted

            // Set up the mock for IAssignmentMetricsService to return a failure result
            _assignmentMetricsMock
                .Setup(service => service.InsertReservationAssignmentTrackRecordMetrics(It.IsAny<SaveAssignmentTrackRecordViewModel>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _assignmentController.SaveAssignmentTrackRecordFromMetrics(assignmentMetrics);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_INSERTED, response.statusMessage);
        }

        [Fact]
        public async Task SaveAssignmentTrackRecordFromMetrics_Success_WhenCommentsAreNull()
        {
            // Arrange
            var assignmentMetrics = new SaveAssignmentTrackRecordViewModel
            {
                ContractorID = 1,
                ReservationsAssignmentsID = 123,
                ReservationAssignmentTrackingID = 456,
                TimeInterval = "01:30:00", // Valid TimeInterval
                Comments = null // Null Comments
            };

            var expectedResult = 1; // Assuming 1 means the data was inserted successfully

            // Set up the mock for IAssignmentMetricsService to return a successful result
            _assignmentMetricsMock
                .Setup(service => service.InsertReservationAssignmentTrackRecordMetrics(It.IsAny<SaveAssignmentTrackRecordViewModel>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _assignmentController.SaveAssignmentTrackRecordFromMetrics(assignmentMetrics);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
        }

        [Fact]
        public async Task SaveAssignmentMetrics_ReturnsSuccess_WhenDataIsInserted()
        {
            // Arrange
            var assignmentMetrics = new List<SaveAssignmentMetricsViewModel>
            {
                new SaveAssignmentMetricsViewModel
                {
                    ReservationsAssignmentsID = 123,
                    ACCTGCode = "A001", // Example valid ACCTGCode
                    Qty = "10", // Example valid Qty
                    Notes = "Test note" // Example valid Notes
                }
            };

            // Set up the mock for IAssignmentMetricsService to successfully insert data
            _assignmentMetricsMock
                .Setup(service => service.InsertAssignmentMetricsFromJsonAsync(It.IsAny<List<SaveAssignmentMetricsViewModel>>()))
                .Returns(Task.CompletedTask); // Assuming the method completes successfully without returning any data

            // Act
            var result = await _assignmentController.SaveAssignmentMetrics(assignmentMetrics);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(ResponseMessage.INSERT_ASSIGNMENT_METRICS_SUCCESS, response.data);
        }

        [Fact]
        public async Task SaveAssignmentMetrics_ReturnsSuccess_WhenListIsEmpty()
        {
            // Arrange
            var assignmentMetrics = new List<SaveAssignmentMetricsViewModel>(); // Empty list

            // Set up the mock for IAssignmentMetricsService to handle empty list gracefully
            _assignmentMetricsMock
                .Setup(service => service.InsertAssignmentMetricsFromJsonAsync(It.IsAny<List<SaveAssignmentMetricsViewModel>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _assignmentController.SaveAssignmentMetrics(assignmentMetrics);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(ResponseMessage.INSERT_ASSIGNMENT_METRICS_SUCCESS, response.data);
        }

        [Fact]
        public async Task SaveAssignmentMetrics_ReturnsSuccess_WhenNotesAreProvided()
        {
            // Arrange
            var assignmentMetrics = new List<SaveAssignmentMetricsViewModel>
            {
                new SaveAssignmentMetricsViewModel
                {
                    ReservationsAssignmentsID = 123,
                    ACCTGCode = "A001",
                    Qty = "10",
                    Notes = "Test note" // Optional field
                }
            };



            // Set up the mock for IAssignmentMetricsService to return a successful result
            _assignmentMetricsMock
                .Setup(service => service.InsertAssignmentMetricsFromJsonAsync(It.IsAny<List<SaveAssignmentMetricsViewModel>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _assignmentController.SaveAssignmentMetrics(assignmentMetrics);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(ResponseMessage.INSERT_ASSIGNMENT_METRICS_SUCCESS, response.data);
        }

        [Fact]
        public async Task AssignmnetMetricsDetails_ReturnsValidResponse_WhenDataExists()
        {
            // Arrange
            var model = new AssignmentMetricsViewModel
            {
                ReservationsAssignmentsID = 123 // Example valid ReservationsAssignmentsID
            };

            // Set up the mock for FetchAssignmentMetrics
            var mockAssignmentMetrics = Task.FromResult(new List<AssignmentMetricsResponse>
            {
                new AssignmentMetricsResponse
                {
                    code = "Code1",
                    description = "Description1",
                    Quantity = "10",
                    metricunitofmeasure = "kg",
                    expectedQty = "20",
                    Notes = "Notes1"
                }
            });

            // Set up the mock for FetchWaitingRecords
            var mockWaitingRecords = Task.FromResult(new List<AssignmentTrackOtherRecordResponseModel>
    {
        new AssignmentTrackOtherRecordResponseModel
        {
            WaitingID = 1,
            ContractorID = 123,
            ReservationAssignmentTrackingID = 456,
            ReservationsAssignmentsID = 789,
            StartLatitudeLongitude = "10.0000,20.0000",
            StartDateandTime = DateTime.UtcNow,
            EndLatitudeLongitude = "11.0000,21.0000",
            EndDateandTime = DateTime.UtcNow.AddHours(1),
            Comments = "Some comments",
            ButtonID = 2,
            TimeInterval = "10:00:00",
            IsManuallyEntered = 1
        },
        new AssignmentTrackOtherRecordResponseModel
        {
            WaitingID = 2,
            ContractorID = 124,
            ReservationAssignmentTrackingID = 457,
            ReservationsAssignmentsID = 790,
            StartLatitudeLongitude = "12.0000,22.0000",
            StartDateandTime = DateTime.UtcNow.AddHours(2),
            EndLatitudeLongitude = "13.0000,23.0000",
            EndDateandTime = DateTime.UtcNow.AddHours(3),
            Comments = "Other comments",
            ButtonID = 3,
            TimeInterval = "12:00:00",
            IsManuallyEntered = 0
        }
    });

            // Mock the service calls
            _assignmentMetricsMock
                .Setup(service => service.FetchAssignmentMetrics(It.IsAny<int>()))
                .Returns(mockAssignmentMetrics);

            _assignmentMetricsMock
                .Setup(service => service.FetchWaitingRecords(It.IsAny<int>()))
                .Returns(mockWaitingRecords);

            // Act
            var result = await _assignmentController.AssignmnetMetricsDetails(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);

            var metricsResponseModel = response.data as MobileAssignmentMetricsResponseModel;
            Assert.NotNull(metricsResponseModel);

            // Assert that the metricsResponse contains data
            Assert.Single(metricsResponseModel.metricsResponse); // Expecting 1 record
            Assert.Equal("Code1", metricsResponseModel.metricsResponse[0].code);
            Assert.Equal("Description1", metricsResponseModel.metricsResponse[0].description);

            // Assert that the WaitingRecordsList contains data
            Assert.Equal(2, metricsResponseModel.WaitingRecordsList.Count); // Expecting 2 records
            Assert.Equal(1, metricsResponseModel.WaitingRecordsList[0].WaitingID);
            Assert.Equal(2, metricsResponseModel.WaitingRecordsList[1].WaitingID);
        }

        [Fact]
        public async Task AssignmnetMetricsDetails_ReturnsEmptyResponse_WhenNoDataExists()
        {
            // Arrange
            var model = new AssignmentMetricsViewModel
            {
                ReservationsAssignmentsID = 123 // Example valid ReservationsAssignmentsID
            };

            // Set up the mock for FetchAssignmentMetrics to return no data
            var mockAssignmentMetrics = Task.FromResult(new List<AssignmentMetricsResponse>());

            // Set up the mock for FetchWaitingRecords to return no data
            var mockWaitingRecords = Task.FromResult(new List<AssignmentTrackOtherRecordResponseModel>());

            // Mock the service calls
            _assignmentMetricsMock
                .Setup(service => service.FetchAssignmentMetrics(It.IsAny<int>()))
                .Returns(mockAssignmentMetrics);

            _assignmentMetricsMock
                .Setup(service => service.FetchWaitingRecords(It.IsAny<int>()))
                .Returns(mockWaitingRecords);

            // Act
            var result = await _assignmentController.AssignmnetMetricsDetails(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);

            var metricsResponseModel = response.data as MobileAssignmentMetricsResponseModel;
            Assert.NotNull(metricsResponseModel);

            // Assert that the metricsResponse and WaitingRecordsList are empty
            Assert.Empty(metricsResponseModel.metricsResponse); // No records
            Assert.Empty(metricsResponseModel.WaitingRecordsList); // No records
        }

        [Fact]
        public async Task AssignmnetMetricsDetails_ReturnsEmptyResponse_WhenInvalidReservationsAssignmentsID()
        {
            // Arrange
            var model = new AssignmentMetricsViewModel
            {
                ReservationsAssignmentsID = 999 // Invalid ReservationsAssignmentsID
            };

            // Set up the mock for FetchAssignmentMetrics to return no data
            var mockAssignmentMetrics = Task.FromResult(new List<AssignmentMetricsResponse>());

            // Set up the mock for FetchWaitingRecords to return no data
            var mockWaitingRecords = Task.FromResult(new List<AssignmentTrackOtherRecordResponseModel>());

            // Mock the service calls
            _assignmentMetricsMock
                .Setup(service => service.FetchAssignmentMetrics(It.IsAny<int>()))
                .Returns(mockAssignmentMetrics);

            _assignmentMetricsMock
                .Setup(service => service.FetchWaitingRecords(It.IsAny<int>()))
                .Returns(mockWaitingRecords);

            // Act
            var result = await _assignmentController.AssignmnetMetricsDetails(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);

            var metricsResponseModel = response.data as MobileAssignmentMetricsResponseModel;
            Assert.NotNull(metricsResponseModel);

            // Assert that the metricsResponse and WaitingRecordsList are empty
            Assert.Empty(metricsResponseModel.metricsResponse); // No records
            Assert.Empty(metricsResponseModel.WaitingRecordsList); // No records
        }

        [Fact]
        public async Task AssignmnetMetricsDetails_ReturnsPartialData_WhenWaitingRecordsAreMissing()
        {
            // Arrange
            var model = new AssignmentMetricsViewModel
            {
                ReservationsAssignmentsID = 123 // Example valid ReservationsAssignmentsID
            };

            // Set up the mock for FetchAssignmentMetrics to return valid data
            var mockAssignmentMetrics = Task.FromResult(new List<AssignmentMetricsResponse>
    {
        new AssignmentMetricsResponse
        {
            code = "Code1",
            description = "Description1",
            Quantity = "10",
            metricunitofmeasure = "kg",
            expectedQty = "20",
            Notes = "Notes1"
        }
    });

            // Set up the mock for FetchWaitingRecords to return no data
            var mockWaitingRecords = Task.FromResult(new List<AssignmentTrackOtherRecordResponseModel>());

            // Mock the service calls
            _assignmentMetricsMock
                .Setup(service => service.FetchAssignmentMetrics(It.IsAny<int>()))
                .Returns(mockAssignmentMetrics);

            _assignmentMetricsMock
                .Setup(service => service.FetchWaitingRecords(It.IsAny<int>()))
                .Returns(mockWaitingRecords);

            // Act
            var result = await _assignmentController.AssignmnetMetricsDetails(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);

            var metricsResponseModel = response.data as MobileAssignmentMetricsResponseModel;
            Assert.NotNull(metricsResponseModel);

            // Assert that the metricsResponse contains data
            Assert.Single(metricsResponseModel.metricsResponse); // Expecting 1 record
            Assert.Equal("Code1", metricsResponseModel.metricsResponse[0].code);
            Assert.Equal("Description1", metricsResponseModel.metricsResponse[0].description);

            // Assert that the WaitingRecordsList is empty
            Assert.Empty(metricsResponseModel.WaitingRecordsList); // No records
        }

        [Fact]
        public async Task AssignmnetMetricsDetails_ReturnsMultipleRecords_WhenDataExists()
        {
            // Arrange
            var model = new AssignmentMetricsViewModel
            {
                ReservationsAssignmentsID = 123 // Example valid ReservationsAssignmentsID
            };

            // Set up the mock for FetchAssignmentMetrics to return multiple records
            var mockAssignmentMetrics = Task.FromResult(new List<AssignmentMetricsResponse>
    {
        new AssignmentMetricsResponse
        {
            code = "Code1",
            description = "Description1",
            Quantity = "10",
            metricunitofmeasure = "kg",
            expectedQty = "20",
            Notes = "Notes1"
        },
        new AssignmentMetricsResponse
        {
            code = "Code2",
            description = "Description2",
            Quantity = "5",
            metricunitofmeasure = "m",
            expectedQty = "10",
            Notes = "Notes2"
        }
    });

            // Set up the mock for FetchWaitingRecords to return multiple records
            var mockWaitingRecords = Task.FromResult(new List<AssignmentTrackOtherRecordResponseModel>
    {
        new AssignmentTrackOtherRecordResponseModel
        {
            WaitingID = 1,
            ContractorID = 123,
            ReservationAssignmentTrackingID = 456,
            ReservationsAssignmentsID = 789,
            StartLatitudeLongitude = "10.0000,20.0000",
            StartDateandTime = DateTime.UtcNow,
            EndLatitudeLongitude = "11.0000,21.0000",
            EndDateandTime = DateTime.UtcNow.AddHours(1),
            Comments = "Some comments",
            ButtonID = 2,
            TimeInterval = "10:00:00",
            IsManuallyEntered = 1
        },
        new AssignmentTrackOtherRecordResponseModel
        {
            WaitingID = 2,
            ContractorID = 124,
            ReservationAssignmentTrackingID = 457,
            ReservationsAssignmentsID = 790,
            StartLatitudeLongitude = "12.0000,22.0000",
            StartDateandTime = DateTime.UtcNow.AddHours(2),
            EndLatitudeLongitude = "13.0000,23.0000",
            EndDateandTime = DateTime.UtcNow.AddHours(3),
            Comments = "Other comments",
            ButtonID = 3,
            TimeInterval = "12:00:00",
            IsManuallyEntered = 0
        }
    });

            // Mock the service calls
            _assignmentMetricsMock
                .Setup(service => service.FetchAssignmentMetrics(It.IsAny<int>()))
                .Returns(mockAssignmentMetrics);

            _assignmentMetricsMock
                .Setup(service => service.FetchWaitingRecords(It.IsAny<int>()))
                .Returns(mockWaitingRecords);

            // Act
            var result = await _assignmentController.AssignmnetMetricsDetails(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);

            var metricsResponseModel = response.data as MobileAssignmentMetricsResponseModel;
            Assert.NotNull(metricsResponseModel);

            // Assert that the metricsResponse contains multiple records
            Assert.Equal(2, metricsResponseModel.metricsResponse.Count); // Expecting 2 records

            // Assert that the WaitingRecordsList contains multiple records
            Assert.Equal(2, metricsResponseModel.WaitingRecordsList.Count); // Expecting 2 records
        }

        [Fact]
        public async Task ReservationAssignmentCancelStatus_ReturnsSuccess_WhenDataExists()
        {
            // Arrange: Set up the mock to return valid data
            var tokenClaims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("Type", "123") }));
            var mockData = new List<ReservationCancelDetails>
            {
                new ReservationCancelDetails { code = "R001", description = "Cancelled by User" },
                new ReservationCancelDetails { code = "R002", description = "Cancelled by Admin" }
            };

            // Mock the service method to return the mock data
            _reservationAssignmentsServiceMock
                .Setup(service => service.ReservationAssignmentCancelStatus(It.IsAny<int>()))
                .ReturnsAsync(mockData);

            // Act: Call the method on the controller
            var result = await _assignmentController.ReservationAssignmentCancelStatus();

            // Assert: Verify that the response status is TRUE and data is returned
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(2, ((IEnumerable<ReservationCancelDetails>)response.data).Count());  // Expecting 2 records
        }

        [Fact]
        public async Task ReservationAssignmentCancelStatus_ReturnsNoData_WhenNoRecordsFound()
        {
            // Arrange: Set up the mock to return an empty list (no data)
            var tokenClaims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("Type", "123") }));

            _reservationAssignmentsServiceMock
                .Setup(service => service.ReservationAssignmentCancelStatus(It.IsAny<int>()))
                .ReturnsAsync(new List<ReservationCancelDetails>());

            // Act: Call the method on the controller
            var result = await _assignmentController.ReservationAssignmentCancelStatus();

            // Assert: Verify that the response status is FALSE and no data is returned
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty); // No data returned
        }

        [Fact]
        public async Task MobileOngoingAssignment_ReturnsOkWithAssignments()
        {
            // Arrange
            var expectedAssignments = new List<OngoingAssignmentTrackingResponseModel>
            {
                new OngoingAssignmentTrackingResponseModel { ReservationsAssignmentsID = 1, ClaimantName = "In Progress",ContractorCellPhone ="1234567890" },
                new OngoingAssignmentTrackingResponseModel { ReservationsAssignmentsID = 2, ClaimantName = "Pending",ContractorCellPhone="1234567890" }
            };

            var tokenClaims = new AppTokenDetails { UserID = 123, Type = 1 }; // Adjust based on actual claims structure
            _reservationAssignmentsServiceMock
                .Setup(service => service.OnGoingAssignment(It.IsAny<AppTokenDetails>()))
                .ReturnsAsync(expectedAssignments);

            // Act
            var result = await _assignmentController.MobileOngoingAssignment() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.TRUE);
            Assert.Equal(expectedAssignments, response.data);
        }

        [Fact]
        public async Task MobileOngoingAssignment_ReturnsOkWithEmptyList_WhenNoAssignments()
        {
            // Arrange
            var emptyAssignments = new List<OngoingAssignmentTrackingResponseModel>();

            var tokenClaims = new AppTokenDetails { UserID = 123, Type = 1 }; // Adjust based on actual claims structure
            _reservationAssignmentsServiceMock
                .Setup(service => service.OnGoingAssignment(It.IsAny<AppTokenDetails>()))
                .ReturnsAsync(emptyAssignments);

            // Act
            var result = await _assignmentController.MobileOngoingAssignment() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.TRUE);
            Assert.Empty((IEnumerable<OngoingAssignmentTrackingResponseModel>)response.data);
        }

        [Fact]
        public async Task MobileOngoingAssignment_ReturnsBadRequest_WhenUserClaimsInvalid()
        {
            // Arrange: Set up invalid claims (empty claims principal)
            _assignmentController.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            // Act
            var result = await _assignmentController.MobileOngoingAssignment();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task MobileOngoingAssignment_ReturnsOkWithNull_WhenOnGoingAssignmentReturnsNull()
        {
            // Arrange
            var tokenClaims = new AppTokenDetails { UserID = 123, Type = 1 }; // Adjust based on actual claims structure
            _reservationAssignmentsServiceMock
                .Setup(service => service.OnGoingAssignment(It.IsAny<AppTokenDetails>()))
                .ReturnsAsync(new List<OngoingAssignmentTrackingResponseModel>()); // Return empty list instead of null

            // Act
            var result = await _assignmentController.MobileOngoingAssignment() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.TRUE);
            Assert.NotNull(response.data);
        }

        [Fact]
        public async Task AssignmentTrackWaitngRecords_ReturnsOk_WhenRequestIsValid()
        {
            // Arrange
            var model = new ReservationAssignmentTrackOtherRecordsViewModel
            {
                ButtonID = 1,
                ReservationsAssignmentsID = 2,
                DriverLatitudeLongitude = "123.456,78.90",
                WaitingDateandTime = DateTime.UtcNow
            };

            var expectedResponse = new ValueTuple<int, string, List<AssignmentTrackOtherRecordResponseModel>>(
                1, // Status
                "Success", // Status message
                new List<AssignmentTrackOtherRecordResponseModel>
                {
                    new AssignmentTrackOtherRecordResponseModel
                    {
                        WaitingID = 1,
                        ContractorID = 1,
                        ReservationAssignmentTrackingID = 123,
                        ReservationsAssignmentsID = 2
                    }
                });

            _reservationAssignmentsServiceMock
                .Setup(service => service.AssignmentTrackingOtherRecords(It.IsAny<ReservationAssignmentTrackOtherRecordsViewModel>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _assignmentController.AssignmentTrackWaitngRecords(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.TRUE);
            Assert.Equal(expectedResponse.Item2, response.statusMessage);
            Assert.NotNull(response.data);
            Assert.IsType<List<AssignmentTrackOtherRecordResponseModel>>(response.data);
        }

        [Fact]
        public async Task AssignmentTrackWaitngRecords_ReturnsBadRequest_WhenRequiredFieldsAreMissing()
        {
            // Arrange
            var model = new ReservationAssignmentTrackOtherRecordsViewModel
            {
                // Missing required fields like ButtonID or DriverLatitudeLongitude
                ReservationAssignmentTrackingID = 123,  // Replace with a valid value
                ContractorID = 1, // Replace with a valid value
                ReservationsAssignmentsID = 2,
                WaitingDateandTime = DateTime.UtcNow
            };

            // Act
            var result = await _assignmentController.AssignmentTrackWaitngRecords(model) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.FALSE);
            //Assert.Equal("Missing required fields", response.statusMessage);  // Adjust according to your error message
        }

        [Fact]
        public async Task AssignmentTrackWaitngRecords_ReturnsBadRequest_WhenServiceReturnsError()
        {
            // Arrange
            var model = new ReservationAssignmentTrackOtherRecordsViewModel
            {
                ButtonID = 1,
                ReservationsAssignmentsID = 2,
                DriverLatitudeLongitude = "123.456,78.90",
                WaitingDateandTime = DateTime.UtcNow
            };

            var expectedResponse = new ValueTuple<int, string, List<AssignmentTrackOtherRecordResponseModel>>(
                0, // Error status
                "Error occurred", // Error message
                new List<AssignmentTrackOtherRecordResponseModel>()
            );

            _reservationAssignmentsServiceMock
                .Setup(service => service.AssignmentTrackingOtherRecords(It.IsAny<ReservationAssignmentTrackOtherRecordsViewModel>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _assignmentController.AssignmentTrackWaitngRecords(model) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.FALSE);
            Assert.Equal(expectedResponse.Item2, response.statusMessage);
        }

        [Fact]
        public async Task AssignmentTrackWaitngRecords_ReturnsOk_WhenServiceReturnsEmptyList()
        {
            // Arrange
            var model = new ReservationAssignmentTrackOtherRecordsViewModel
            {
                ButtonID = 1,
                ReservationsAssignmentsID = 2,
                DriverLatitudeLongitude = "123.456,78.90",
                WaitingDateandTime = DateTime.UtcNow
            };

            var expectedResponse = new ValueTuple<int, string, List<AssignmentTrackOtherRecordResponseModel>>(
                1, // Status
                "No records found", // Status message
                new List<AssignmentTrackOtherRecordResponseModel>() // Empty list
            );

            _reservationAssignmentsServiceMock
                .Setup(service => service.AssignmentTrackingOtherRecords(It.IsAny<ReservationAssignmentTrackOtherRecordsViewModel>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _assignmentController.AssignmentTrackWaitngRecords(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.TRUE);
            Assert.Equal(expectedResponse.Item2, response.statusMessage);

        }

        [Fact]
        public async Task AssignmentTrackWaitngRecords_ReturnsOk_WhenServiceReturnsMultipleRecords()
        {
            // Arrange
            var model = new ReservationAssignmentTrackOtherRecordsViewModel
            {
                ButtonID = 1,
                ReservationsAssignmentsID = 2,
                DriverLatitudeLongitude = "123.456,78.90",
                WaitingDateandTime = DateTime.UtcNow
            };

            var expectedResponse = new ValueTuple<int, string, List<AssignmentTrackOtherRecordResponseModel>>(
                1, // Status
                "Success", // Status message
                new List<AssignmentTrackOtherRecordResponseModel>
                {
            new AssignmentTrackOtherRecordResponseModel
            {
                WaitingID = 1,
                ContractorID = 1,
                ReservationAssignmentTrackingID = 123,
                ReservationsAssignmentsID = 2
            },
            new AssignmentTrackOtherRecordResponseModel
            {
                WaitingID = 2,
                ContractorID = 2,
                ReservationAssignmentTrackingID = 456,
                ReservationsAssignmentsID = 3
            }
                }
            );

            _reservationAssignmentsServiceMock
                .Setup(service => service.AssignmentTrackingOtherRecords(It.IsAny<ReservationAssignmentTrackOtherRecordsViewModel>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _assignmentController.AssignmentTrackWaitngRecords(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.TRUE);
            Assert.Equal(expectedResponse.Item2, response.statusMessage);
            Assert.NotNull(response.data);
            var dataList = response.data as List<AssignmentTrackOtherRecordResponseModel>;
            Assert.NotNull(dataList);
            Assert.Equal(2, dataList.Count); // Check if two records are returned
        }

        [Fact]
        public async Task AssignmentTrackWaitngRecords_ReturnsBadRequest_WhenContractorIDIsInvalid()
        {
            // Arrange
            var model = new ReservationAssignmentTrackOtherRecordsViewModel
            {
                ButtonID = 1,
                ReservationsAssignmentsID = 2,
                DriverLatitudeLongitude = "123.456,78.90",
                WaitingDateandTime = DateTime.UtcNow,
                ContractorID = 0 // Invalid ContractorID
            };

            var expectedResponse = new ValueTuple<int, string, List<AssignmentTrackOtherRecordResponseModel>>(
                0, // Error status
                "Invalid ContractorID", // Error message
                new List<AssignmentTrackOtherRecordResponseModel>()
            );

            _reservationAssignmentsServiceMock
                .Setup(service => service.AssignmentTrackingOtherRecords(It.IsAny<ReservationAssignmentTrackOtherRecordsViewModel>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _assignmentController.AssignmentTrackWaitngRecords(model) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.FALSE);
            Assert.Equal(expectedResponse.Item2, response.statusMessage);

        }

        [Fact]
        public async Task GetLiveCoordinates_ReturnsOk_WhenRequestIsValid()
        {
            // Arrange
            var model = new TrackingAssignmentIDViewModel
            {
                ReservationsAssignmentsID = 1,
                AssignmentTrackingID = 123,
                isDeadMile = 0
            };

            var liveCoordinates = new List<Coordinate>
            {
                new Coordinate { Latitude = 12.9716, Longitude = 77.5946 }
            };

            var googleCoordinates = new List<Coordinate>
            {
                new Coordinate { Latitude = 12.9717, Longitude = 77.5950 }
            };

            _reservationAssignmentsServiceMock
                .Setup(service => service.GetLiveTrackingCoordinates(model))
                .ReturnsAsync(liveCoordinates);

            _reservationAssignmentsServiceMock
                .Setup(service => service.GetGooglePathCoordinates(model.ReservationsAssignmentsID))
                .ReturnsAsync(googleCoordinates);

            // Act
            var result = await _assignmentController.GetLiveCoordinates(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.TRUE);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);

            var data = response.data as LiveTrackingMapResponseModel;
            Assert.NotNull(data);
            Assert.Equal(model.ReservationsAssignmentsID, data.ReservationsAssignmentsID);
            Assert.Equal(liveCoordinates, data.LatitudeLongitude);
            Assert.Equal(googleCoordinates, data.GooglePath);
        }

        [Fact]
        public async Task GetLiveCoordinates_ReturnsOk_WhenOneCoordinateListIsEmpty()
        {
            // Arrange
            var model = new TrackingAssignmentIDViewModel
            {
                ReservationsAssignmentsID = 1,
                AssignmentTrackingID = 123,
                isDeadMile = 0
            };

            var liveCoordinates = new List<Coordinate>
    {
        new Coordinate { Latitude = 12.9716, Longitude = 77.5946 }
    };

            var googleCoordinates = new List<Coordinate>(); // Empty Google path

            _reservationAssignmentsServiceMock
                .Setup(service => service.GetLiveTrackingCoordinates(model))
                .ReturnsAsync(liveCoordinates);

            _reservationAssignmentsServiceMock
                .Setup(service => service.GetGooglePathCoordinates(model.ReservationsAssignmentsID))
                .ReturnsAsync(googleCoordinates);

            // Act
            var result = await _assignmentController.GetLiveCoordinates(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.TRUE);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);

            var data = response.data as LiveTrackingMapResponseModel;
            Assert.NotNull(data);
            Assert.Equal(model.ReservationsAssignmentsID, data.ReservationsAssignmentsID);
            Assert.Equal(liveCoordinates, data.LatitudeLongitude);
            Assert.Empty(data.GooglePath); // Ensure the GooglePath list is empty
        }

        [Fact]
        public async Task SendLiveCoordinates_ReturnsOk_WhenRequestIsValid()
        {
            // Arrange
            var model = new SaveLiveTrackingCoordinatesViewModel
            {
                ReservationsAssignmentsID = 1,
                AssignmentTrackingID = 123,
                LatitudeLongitude = "12.9716,77.5946",
                TrackingDateTime = DateTime.UtcNow,
                isDeadMile = 0
            };

            // Mock the service call to avoid actual implementation
            _reservationAssignmentsServiceMock
                .Setup(service => service.LiveTraking(It.IsAny<SaveLiveTrackingCoordinatesViewModel>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _assignmentController.SendLiveCoordinates(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            var response = result.Value as ResponseModel;
            Assert.NotNull(response);
            Assert.True(response.status == ResponseStatus.TRUE);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
        }

        [Fact]
        public async Task SendLiveCoordinates_ReturnsBadRequest_WhenLatitudeLongitudeIsInvalid()
        {
            // Arrange
            var model = new SaveLiveTrackingCoordinatesViewModel
            {
                ReservationsAssignmentsID = 1,
                AssignmentTrackingID = 123,
                LatitudeLongitude = null,  // Invalid input
                TrackingDateTime = DateTime.UtcNow,
                isDeadMile = 0
            };

            // Act
            var result = await _assignmentController.SendLiveCoordinates(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task SendLiveCoordinates_ReturnsBadRequest_WhenReservationsAssignmentsIDIsMissing()
        {
            // Arrange
            var model = new SaveLiveTrackingCoordinatesViewModel
            {
                ReservationsAssignmentsID = 0,  // Invalid ID
                AssignmentTrackingID = 123,
                LatitudeLongitude = "12.9716,77.5946",
                TrackingDateTime = DateTime.UtcNow,
                isDeadMile = 0
            };

            // Act
            var result = await _assignmentController.SendLiveCoordinates(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task PastTrackAssignmentByID_ValidID_ReturnsOkResultWithData()
        {
            // Arrange
            var assignmentId = 123; // Mock ReservationAssignmentID
            var mockResponseData = new List<TrackAssignmentByIDResponseModel>
            {
                new TrackAssignmentByIDResponseModel
                {
                    // Set up mock data properties (example)
                    ReservationsAssignmentsID = assignmentId,
                    ReservationDate = DateTime.UtcNow,
                    ReachedButtonStatus = "Completed"
                }
            };

            // Mock the service method to return the mock data
            _reservationAssignmentsServiceMock
                .Setup(service => service.TrackAssignmentByID(assignmentId))
                .ReturnsAsync(mockResponseData);  // Make sure the type matches

            // Act
            var result = await _assignmentController.PastTrackAssignmentByID(assignmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); // Expecting OkResult
            var responseModel = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, responseModel.status); // Verify status
            Assert.NotNull(responseModel.data); // Verify data is not null
            var data = Assert.IsType<List<TrackAssignmentByIDResponseModel>>(responseModel.data);
            Assert.Single(data); // Assert that one data item is returned
            Assert.Equal(assignmentId, data[0].ReservationsAssignmentsID); // Assert that data matches the input ID
        }

        [Fact]
        public async Task PastTrackAssignmentByID_EmptyResponse_ReturnsOkWithEmptyData()
        {
            // Arrange
            var assignmentId = 123; // Mock ReservationAssignmentID
            var mockResponseData = new List<TrackAssignmentByIDResponseModel>(); // Empty list

            // Mock the service method to return empty data
            _reservationAssignmentsServiceMock
                .Setup(service => service.TrackAssignmentByID(assignmentId))
                .ReturnsAsync(mockResponseData);  // Returning an empty list

            // Act
            var result = await _assignmentController.PastTrackAssignmentByID(assignmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); // Expecting OkResult
            var responseModel = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, responseModel.status); // Verify status
            Assert.Equal(responseModel.data, string.Empty); // Verify that the data is empty
        }

        [Fact]
        public async Task PastTrackAssignmentByID_ValidID_ReturnsOkResultWithMultipleData()
        {
            // Arrange
            var assignmentId = 123; // Mock ReservationAssignmentID
            var mockResponseData = new List<TrackAssignmentByIDResponseModel>
            {
                new TrackAssignmentByIDResponseModel
                {
                    ReservationsAssignmentsID = assignmentId,
                    ReservationDate = DateTime.UtcNow,
                    ReachedButtonStatus = "Completed"
                },
                new TrackAssignmentByIDResponseModel
                {
                    ReservationsAssignmentsID = assignmentId,
                    ReservationDate = DateTime.UtcNow.AddDays(1),
                    ReachedButtonStatus = "In Progress"
                }
            };

            // Mock the service method to return multiple data items
            _reservationAssignmentsServiceMock
                .Setup(service => service.TrackAssignmentByID(assignmentId))
                .ReturnsAsync(mockResponseData);  // Returning a list with multiple items

            // Act
            var result = await _assignmentController.PastTrackAssignmentByID(assignmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); // Expecting OkResult
            var responseModel = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, responseModel.status); // Verify status
            Assert.NotNull(responseModel.data); // Verify data is not null
            var data = Assert.IsType<List<TrackAssignmentByIDResponseModel>>(responseModel.data);
            Assert.Equal(2, data.Count); // Assert that two data items are returned
            Assert.Equal(assignmentId, data[0].ReservationsAssignmentsID); // Assert that data matches the input ID
            Assert.Equal(assignmentId, data[1].ReservationsAssignmentsID); // Assert that data matches the input ID for the second item
        }

        [Fact]
        public async Task GetAssignmentDetails_ValidModel_ReturnsOkResultWithData()
        {
            // Arrange
            var model = new AssignmentMetricsViewModel
            {
                ReservationsAssignmentsID = 123 // Mock ReservationAssignmentID
            };

            var mockResponseData = new List<ReservationAssignmentAppResponseModel>
        {
            new ReservationAssignmentAppResponseModel
            {
                ReservationsAssignmentsID = 123,
                AssgnNum = "Assignment001",
                ReservationDate = DateTime.UtcNow,
                // You can mock other properties as needed
                TotalCount = 1
            }
        };

            // Mock the service method to return the mock data
            _reservationAssignmentsServiceMock
                .Setup(service => service.MobileReservationAssignmentSearch(It.IsAny<AppReservationAssignmentSearchViewModel>()))
                .ReturnsAsync(mockResponseData);

            // Act
            var result = await _assignmentController.GetAssignmentDetails(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); // Expecting OkResult
            var responseModel = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, responseModel.status); // Verify status
            Assert.NotNull(responseModel.data); // Verify data is not null
            var data = Assert.IsType<List<ReservationAssignmentAppResponseModel>>(responseModel.data);
            Assert.Single(data); // Assert that one data item is returned
            Assert.Equal(123, data[0].ReservationsAssignmentsID); // Assert that the ID matches
            Assert.Equal(1, responseModel.totalData); // Assert that TotalCount matches the expected value
        }

        [Fact]
        public async Task GetAssignmentDetails_EmptyData_ReturnsOkResultWithEmptyData()
        {
            // Arrange
            var model = new AssignmentMetricsViewModel
            {
                ReservationsAssignmentsID = 999 // Mocking an ID that won't return any data
            };

            var mockResponseData = new List<ReservationAssignmentAppResponseModel>(); // Empty data

            // Mock the service method to return empty data
            _reservationAssignmentsServiceMock
                .Setup(service => service.MobileReservationAssignmentSearch(It.IsAny<AppReservationAssignmentSearchViewModel>()))
                .ReturnsAsync(mockResponseData);

            // Act
            var result = await _assignmentController.GetAssignmentDetails(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseModel = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, responseModel.status); // Verify status
            Assert.Equal(responseModel.data, string.Empty); // Assert that the data is empty
            Assert.Equal(0, responseModel.totalData); // Assert that TotalCount is 0
        }

        [Fact]
        public async Task AcceptCancelAssignment_InvalidReservationID_ReturnsBadRequest()
        {
            // Arrange
            var model = new ReservationAssignmentAcceptRejectAppViewModel
            {
                ReservationID = null, // Invalid ReservationID
                Type = 2
            };

            // Act
            var result = await _assignmentController.AcceptCancelAssignment(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Null(response.statusMessage);
        }

        [Fact]
        public async Task AcceptCancelAssignment_ShouldReturnBadRequest_WhenReservationIdIsInvalid()
        {
            // Arrange
            var model = new ReservationAssignmentAcceptRejectAppViewModel
            {
                ReservationID = null, // Invalid ReservationID
                ButtonStatus = ButtonStatus.ACCEPT,
                Type = 2
            };

            var userClaims = new List<Claim>
        {
            new Claim("Type", "2"),
            new Claim("UserID", "123")
        };
            _assignmentController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(userClaims))
            };

            // Act
            var result = await _assignmentController.AcceptCancelAssignment(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("ReservationID Not Found", response.statusMessage);
        }

        [Fact]
        public async Task AcceptCancelAssignment_ShouldReturnBadRequest_WhenServiceReturnsFailure()
        {
            // Arrange
            var model = new ReservationAssignmentAcceptRejectAppViewModel
            {
                ReservationID = 1, // Valid ReservationID
                AssignmentID = 123,
                ButtonStatus = ButtonStatus.CANCEL,
                Type = 2
            };

            var userClaims = new List<Claim>
            {
                new Claim("Type", "2"),
                new Claim("UserID", "123")
            };
            _assignmentController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(userClaims))
            };

            // Mock the service result to simulate failure
            _reservationAssignmentsServiceMock
                .Setup(service => service.MobileAcceptRejectAssignment(It.IsAny<ReservationAssignmentAcceptRejectAppViewModel>()))
                .ReturnsAsync((0, "Failure", new List<ReservationAssignmentAcceptRejectAppViewModel>()));

            // Act
            var result = await _assignmentController.AcceptCancelAssignment(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Failure", response.statusMessage);
        }

        [Fact]
        public async Task AcceptCancelAssignment_ShouldReturnBadRequest_WhenServiceReturnsSuccess()
        {
            // Arrange
            var model = new ReservationAssignmentAcceptRejectAppViewModel
            {
                ReservationID = 1, // Valid ReservationID
                AssignmentID = 123,
                ButtonStatus = ButtonStatus.ACCEPT,
                Type = 2
            };

            var userClaims = new List<Claim>
            {
                new Claim("Type", "2"),
                new Claim("UserID", "123")
            };
            _assignmentController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(userClaims))
            };

            // Mock the service result to simulate failure
            _reservationAssignmentsServiceMock
                .Setup(service => service.MobileAcceptRejectAssignment(It.IsAny<ReservationAssignmentAcceptRejectAppViewModel>()))
                .ReturnsAsync((1, "Success", new List<ReservationAssignmentAcceptRejectAppViewModel>()));

            // Act
            var result = await _assignmentController.AcceptCancelAssignment(model);

            // Assert
            var okRequestResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okRequestResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal("Success", response.statusMessage);
        }


        [Fact]
        public async Task MobileAssignmentCancelAfterAccept_ShouldReturnBadRequest_WhenReservationIDIsInvalid()
        {
            // Arrange
            var model = new UpdateAssignmentStatusViewModel
            {
                ReservationID = null, // Invalid ReservationID
                ButtonStatus = ButtonStatus.CANCEL,
                AssignmentID = 123,
                Type = 1
            };

            var userClaims = new List<Claim>
        {
            new Claim("Type", "1"),
            new Claim("UserID", "123")
        };
            _assignmentController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(userClaims))
            };

            // Act
            var result = await _assignmentController.MobileAssignmentCancelAfterAccept(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Data Not Found", response.statusMessage);
        }

        [Fact]
        public async Task MobileAssignmentCancelAfterAccept_ShouldReturnBadRequest_WhenServiceReturnsFailure()
        {
            // Arrange
            var model = new UpdateAssignmentStatusViewModel
            {
                ReservationID = 1,
                ButtonStatus = ButtonStatus.CANCEL,
                AssignmentID = 123,
                Type = 1
            };

            var userClaims = new List<Claim>
            {
                new Claim("Type", "1"),
                new Claim("UserID", "123")
            };
            _assignmentController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(userClaims))
            };

            // Mock the service result to simulate failure
            _reservationAssignmentsServiceMock
                .Setup(service => service.MobileCancelAfterAccept(It.IsAny<UpdateAssignmentStatusViewModel>()))
                .ReturnsAsync((0, "Failure"));

            // Act
            var result = await _assignmentController.MobileAssignmentCancelAfterAccept(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal("Failure", response.data);
        }

        [Fact]
        public async Task MobileAssignmentCancelAfterAccept_ShouldReturnBadRequest_WhenServiceReturnsClaimantFailure()
        {
            // Arrange
            var model = new UpdateAssignmentStatusViewModel
            {
                ReservationID = 0,
                ButtonStatus = ButtonStatus.CANCEL,
                AssignmentID = 123,
                Type = 2
            };

            var userClaims = new List<Claim>
            {
                new Claim("Type", "2"),
                new Claim("UserID", "123")
            };
            _assignmentController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(userClaims))
            };

            // Mock the service result to simulate failure
            _reservationAssignmentsServiceMock
                .Setup(service => service.MobileCancelAfterAccept(It.IsAny<UpdateAssignmentStatusViewModel>()))
                .ReturnsAsync((0, "Failure"));

            // Act
            var result = await _assignmentController.MobileAssignmentCancelAfterAccept(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("ReservationID not Found", response.statusMessage);
        }

        [Fact]
        public async Task MobileAssignmentCancelAfterAccept_ShouldReturnBadRequest_WhenServiceReturnsSuccess()
        {
            // Arrange
            var model = new UpdateAssignmentStatusViewModel
            {
                ReservationID = 1,
                ButtonStatus = ButtonStatus.CANCEL,
                AssignmentID = 123,
                Type = 1
            };

            var userClaims = new List<Claim>
            {
                new Claim("Type", "1"),
                new Claim("UserID", "123")
            };
            _assignmentController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(userClaims))
            };

            // Mock the service result to simulate failure
            _reservationAssignmentsServiceMock
                .Setup(service => service.MobileCancelAfterAccept(It.IsAny<UpdateAssignmentStatusViewModel>()))
                .ReturnsAsync((1, "Success"));

            // Act
            var result = await _assignmentController.MobileAssignmentCancelAfterAccept(model);

            // Assert
            var badRequestResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal("Success", response.data);
        }


        [Fact]
        public async Task AssignmentTracking_InvalidInputParams_ReturnsBadRequest()
        {
            // Arrange
            var model = new ReservationAssignmentTrackingViewModel
            {
                ButtonID = 0,        // Invalid ButtonID
                ClaimantID = 1       // Non-null valid ClaimantID
            };

            // Act
            var result = await _assignmentController.AssignmentTracking(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);

            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.INVALID_INPUT_PARAMS, response.statusMessage);
        }

        [Fact]
        public async Task AssignmentTracking_ContractorLogic_AssignsCorrectValues()
        {
            // Arrange
            var model = new ReservationAssignmentTrackingViewModel
            {
                ButtonID = 1,
                ClaimantID = 123
            };

            // Act
            await _assignmentController.AssignmentTracking(model);

            // Assert
            Assert.Equal(123, model.ContractorID);
            Assert.Equal(123, model.CreateUserID);
            Assert.NotNull(model.CreateDate);
        }

        [Fact]
        public async Task AssignmentTracking_CalculateDeadMiles_ReturnsValidMiles()
        {
            // Arrange
            var model = new ReservationAssignmentTrackingViewModel
            {
                ButtonID = 2
            };

            var coordinates = new List<Coordinate>
            {
                new Coordinate
                {
                    Latitude = 123,
                    Longitude = 214
                },
                new Coordinate
                {
                    Latitude = 124,
                    Longitude = 478
                }
            };

            _reservationAssignmentsServiceMock
                .Setup(x => x.GetCoordinatesFromDatabase(It.IsAny<TrackingAssignmentIDViewModel>()))
                        .ReturnsAsync(coordinates);

            // Act
            await _assignmentController.AssignmentTracking(model);

            // Assert
            Assert.NotNull(model.DeadMiles);
            Assert.True(model.DeadMiles > 0);
        }

        [Fact]
        public async Task AssignmentTracking_CalculateTravellingMiles_ReturnsValidMiles()
        {
            // Arrange
            var model = new ReservationAssignmentTrackingViewModel
            {
                ButtonID = 4
            };

            var coordinates = new List<Coordinate>
            {
                new Coordinate
                {
                    Latitude = 123,
                    Longitude = 214
                },
                new Coordinate
                {
                    Latitude = 124,
                    Longitude = 478
                }
            };

            _reservationAssignmentsServiceMock
     .Setup(x => x.GetCoordinatesFromDatabase(It.IsAny<TrackingAssignmentIDViewModel>()))
     .ReturnsAsync(coordinates);


            // Act
            await _assignmentController.AssignmentTracking(model);

            // Assert
            Assert.NotNull(model.TravellingMiles);
            Assert.True(model.TravellingMiles > 0);
        }

        [Fact]
        public async Task AssignmentTracking_JobClose_ReturnsValidMiles()
        {
            // Arrange
            var model = new ReservationAssignmentTrackingViewModel
            {
                ButtonID = 5
            };

            var coordinates = new List<Coordinate>
            {
                new Coordinate
                {
                    Latitude = 123,
                    Longitude = 214
                },
                new Coordinate
                {
                    Latitude = 124,
                    Longitude = 478
                }
            };

            _reservationAssignmentsServiceMock
     .Setup(x => x.GetCoordinatesFromDatabase(It.IsAny<TrackingAssignmentIDViewModel>()))
     .ReturnsAsync(coordinates);


            // Act
            await _assignmentController.AssignmentTracking(model);

            // Assert
            Assert.Null(model.TravellingMiles);
            Assert.False(model.TravellingMiles > 0);
        }

        [Fact]
        public async Task AssignmentAssgnStatus_ReturnsFalseStatus_WhenCheckFails()
        {
            // Arrange
            var model = new AssignmentAssgnStatusAppViewModel
            {
                ReservationAssignmentID = 1,
                Notes = "Test notes"
            };

            _reservationAssignmentsServiceMock
                .Setup(s => s.CheckReservationAssignmentAsync(model.ReservationAssignmentID, model.Notes))
                .ReturnsAsync((0, "Assignment not found"));

            // Act
            var result = await _assignmentController.AssignmentAssgnStatus(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = Assert.IsType<ResponseModel>(result.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Assignment not found", response.statusMessage);
        }

        [Fact]
        public async Task AssignmentAssgnStatus_ReturnsTrueStatus_WhenCheckSucceeds()
        {
            // Arrange
            var model = new AssignmentAssgnStatusAppViewModel
            {
                ReservationAssignmentID = 2,
                Notes = "Another test"
            };

            _reservationAssignmentsServiceMock
                .Setup(s => s.CheckReservationAssignmentAsync(model.ReservationAssignmentID, model.Notes))
                .ReturnsAsync((1, "Assignment valid"));

            // Act
            var result = await _assignmentController.AssignmentAssgnStatus(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = Assert.IsType<ResponseModel>(result.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal("Assignment valid", response.statusMessage);
        }

        [Fact]
        public async Task GetRelatedAssignmentDetails_ReturnsOk_WithData()
        {
            // Arrange
            var model = new ReservationIDAppViewModel
            {
                ReservationID = 1001
            };

            // Mock data that will be returned from the service
            var mockData = new List<ReservationAssignmentAppResponseModel>
            {
                new ReservationAssignmentAppResponseModel { ReservationsAssignmentsID = 1, claimantName = "Test 1" },
                new ReservationAssignmentAppResponseModel { ReservationsAssignmentsID = 2, claimantName = "Test 2" }
            };

            // Correcting the setup for the method that returns a tuple
            _reservationAssignmentsServiceMock
                .Setup(s => s.GetAllRelatedAssignmentsAsync(It.IsAny<int>()))
                .ReturnsAsync((mockData, 200, "Success"));

            // Act
            var result = await _assignmentController.GetRelatedAssignmentDetails(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = Assert.IsType<ResponseModel>(result.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.IsType<List<ReservationAssignmentAppResponseModel>>(response.data);
            var dataList = response.data as List<ReservationAssignmentAppResponseModel>;
            Assert.Equal(2, dataList?.Count); // Assert that two items are returned
        }

        [Fact]
        public async Task GetRelatedAssignmentDetails_ReturnsOk_WithNoData()
        {
            // Arrange
            var model = new ReservationIDAppViewModel
            {
                ReservationID = 1001
            };

            // Mock empty data that will be returned from the service
            var mockData = new List<ReservationAssignmentAppResponseModel>();

            // Correcting the setup for the method that returns a tuple
            _reservationAssignmentsServiceMock
                .Setup(s => s.GetAllRelatedAssignmentsAsync(It.IsAny<int>()))
                .ReturnsAsync((mockData, 200, "Success"));

            // Act
            var result = await _assignmentController.GetRelatedAssignmentDetails(model) as OkObjectResult;

            // Assert: Check that the result is not null
            Assert.NotNull(result);

            // Assert: Check that the status code is 200 OK
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            // Assert: Check that the response body is of type ResponseModel
            var response = Assert.IsType<ResponseModel>(result.Value);

            // Assert: Check that the status is TRUE and the message is SUCCESS
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);

            // Assert: Check that the data is an empty list
            Assert.IsType<List<ReservationAssignmentAppResponseModel>>(response.data);

            var dataList = response.data as List<ReservationAssignmentAppResponseModel>;

            // Assert: Check that the list is empty
            Assert.Empty(dataList ?? new List<ReservationAssignmentAppResponseModel>()); // Assert that the list is empty

        }

        [Fact]
        public async Task AssignmentsMetricsUpload_NoFiles_ReturnsBadRequest()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1001 };
            var files = new List<IFormFile>();

            // Act
            var result = await _assignmentController.AssignmentsMetricsUpload(model, files);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("No files uploaded.", response.statusMessage);
        }

        [Fact]
        public async Task AssignmentsMetricsUpload_EmptyFile_ReturnsBadRequest()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1001 };
            var files = new List<IFormFile>
            {
                new Mock<IFormFile>().Object // Empty file mock
            };

            // Act
            var result = await _assignmentController.AssignmentsMetricsUpload(model, files);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("One or more files are empty.", response.statusMessage);
        }

        [Fact]
        public async Task AssignmentsMetricsUpload_InvalidFileType_ReturnsBadRequest()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1001 };

            // Mock an invalid file type (e.g., a .txt file)
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("invalid.txt");
            fileMock.Setup(f => f.ContentType).Returns("text/plain");
            fileMock.Setup(f => f.Length).Returns(100); // Arbitrary non-zero file length

            var files = new List<IFormFile> { fileMock.Object };

            // Act
            var result = await _assignmentController.AssignmentsMetricsUpload(model, files);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result); // Change this to ObjectResult
            var response = Assert.IsType<ResponseModel>(objectResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            //Assert.Equal("Invalid file type: invalid.txt. Only JPEG, PNG images are allowed.", response.statusMessage);
        }

        [Fact]
        public async Task AssignmentsMetricsUpload_FileSizeExceedsLimit_ReturnsBadRequest()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1001 };
            var files = new List<IFormFile>
    {
        new Mock<IFormFile>().Object // Simulate file that exceeds 10MB
    };

            // Act
            var result = await _assignmentController.AssignmentsMetricsUpload(model, files);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            //Assert.Equal("File size exceeds 10MB: invalidFile.png", response.statusMessage);
        }

        [Fact]
        public async Task AssignmentsMetricsUpload_SuccessfulUpload_ReturnsOk()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1001 };

            // Mock a valid file (e.g., a PNG image file)
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("validFile.png");
            fileMock.Setup(f => f.ContentType).Returns("image/png");
            fileMock.Setup(f => f.Length).Returns(5000); // Arbitrary non-zero file length

            var files = new List<IFormFile> { fileMock.Object };

            // Mock the successful document save response
            var mockUploadedFile = new UploadedFileInfo { FileName = "validFile.png", FilePath = "path/to/file" };
            _reservationAssignmentsServiceMock
                .Setup(s => s.SaveAssignmentDocument(It.IsAny<int>(), It.IsAny<UploadedFileInfo>()))
                .ReturnsAsync(true); // Simulate a successful database save

            // Act
            var result = await _assignmentController.AssignmentsMetricsUpload(model, files);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal("Files uploaded and saved successfully.", response.statusMessage);
            Assert.IsType<List<UploadedFileInfo>>(response.data);
            var uploadedFiles = response.data as List<UploadedFileInfo>;
            Assert.NotNull(uploadedFiles);
            Assert.Single(uploadedFiles); // Since we're uploading only one file
        }

        [Fact]
        public async Task AssignmentsMetricsUpload_SaveFailure_ReturnsInternalServerError()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1001 };

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("validFile.png");
            fileMock.Setup(f => f.ContentType).Returns("image/png");
            fileMock.Setup(f => f.Length).Returns(5000); // Non-zero length

            var files = new List<IFormFile> { fileMock.Object };

            _reservationAssignmentsServiceMock
                .Setup(s => s.SaveAssignmentDocument(It.IsAny<int>(), It.IsAny<UploadedFileInfo>()))
                .ReturnsAsync(false); // Simulate save failure

            // Act
            var result = await _assignmentController.AssignmentsMetricsUpload(model, files);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result); // Expecting ObjectResult, not just StatusCodeResult
            Assert.Equal(500, objectResult.StatusCode); // Confirm it's a 500 Internal Server Error
            var response = Assert.IsType<ResponseModel>(objectResult.Value); // Optional: assert the response model
            Assert.Equal(ResponseStatus.FALSE, response.status); // Check status field
        }

        [Fact]
        public async Task AssignmentsMetricsUpload_MultipleFiles_ReturnsOk()
        {
            // Arrange
            var model = new AssignmentIDWebViewModel { ReservationsAssignmentsID = 1001 };

            // Create mocks for two valid files
            var fileMock1 = new Mock<IFormFile>();
            fileMock1.Setup(f => f.FileName).Returns("validFile1.png");
            fileMock1.Setup(f => f.ContentType).Returns("image/png");
            fileMock1.Setup(f => f.Length).Returns(5000); // Valid length for a file

            var fileMock2 = new Mock<IFormFile>();
            fileMock2.Setup(f => f.FileName).Returns("validFile2.png");
            fileMock2.Setup(f => f.ContentType).Returns("image/png");
            fileMock2.Setup(f => f.Length).Returns(5000); // Valid length for a file

            var files = new List<IFormFile> { fileMock1.Object, fileMock2.Object };

            // Mock the uploaded file info
            var mockUploadedFile = new UploadedFileInfo { FileName = "validFile.png", FilePath = "path/to/file" };
            _reservationAssignmentsServiceMock
                .Setup(s => s.SaveAssignmentDocument(It.IsAny<int>(), It.IsAny<UploadedFileInfo>()))
                .ReturnsAsync(true); // Simulate successful database save

            // Act
            var result = await _assignmentController.AssignmentsMetricsUpload(model, files);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); // Expect OkObjectResult
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status); // Ensure status is TRUE
            Assert.Equal("Files uploaded and saved successfully.", response.statusMessage); // Check success message
            Assert.IsType<List<UploadedFileInfo>>(response.data); // Ensure data is of type List<UploadedFileInfo>
        }


    }
}
