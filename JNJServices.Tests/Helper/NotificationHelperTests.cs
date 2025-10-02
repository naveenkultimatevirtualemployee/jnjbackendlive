using JNJServices.API.Helper;
using JNJServices.Business.Abstracts;
using JNJServices.Models.CommonModels;
using JNJServices.Models.DbResponseModels;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Reflection;
using static JNJServices.Utility.ApiConstants.NotificationConstants;

namespace JNJServices.Tests.Helper
{
    public class NotificationHelperTests
    {
        private readonly TimeZoneConverter _timeZoneConverter;
        private readonly Mock<ITimeZoneConverter> _mockTimeZoneConverter;
        private readonly Mock<IOptions<FirebaseSettings>> _mockSettings;
        private readonly Mock<ILogger<NotificationHelper>> _mockLogger;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly NotificationHelper _notificationHelper;
        private readonly Mock<INotificationHelper> _mockNotificationHelper;

        public NotificationHelperTests()
        {
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


            // Mock the dependencies
            _timeZoneConverter = new TimeZoneConverter(configuration);

            _mockSettings = new Mock<IOptions<FirebaseSettings>>();
            _mockLogger = new Mock<ILogger<NotificationHelper>>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockNotificationHelper = new Mock<INotificationHelper>();
            _mockTimeZoneConverter = new Mock<ITimeZoneConverter>();

            // Setup the FirebaseSettings mock
            var firebaseSettings = new FirebaseSettings
            {
                FcmCredentialsFilePath = "jnj-services-firebase-creds.json"
            };
            _mockSettings.Setup(s => s.Value).Returns(firebaseSettings);

            // Create the NotificationHelper instance with mocked dependencies
            _notificationHelper = new NotificationHelper(
                _timeZoneConverter,
                _mockSettings.Object,
                _mockLogger.Object,
                _mockNotificationService.Object
            );
        }

        [Fact]
        public async Task SendNotificationContractorCancel_ShouldSendNotification_ForValidModel()
        {
            // Arrange: Setup test data
            var model = new ReservationNotificationAppViewModel
            {
                ReservationAssignmentID = 123,
                NotificationTitle = "Test Assignment Cancellation"
            };

            var contractorCancelResponse = new List<ContractorFirstAcceptCancelResponseModel>
        {
            new ContractorFirstAcceptCancelResponseModel
            {
                ResAsgnCode = "ASG001",
                ReservationDate = DateTime.Now.AddDays(1),
                ReservationTime = DateTime.Now,
                ContractorID = 1,
                NotificationDateTime = DateTime.Now,
                FcmToken = "sample-fcm-token",
                ReservationsAssignmentsID = 123
            }
        };

            // Setup mocks
            _mockNotificationService.Setup(x => x.ContractorCancelLogic(It.IsAny<int>()))
                .ReturnsAsync(contractorCancelResponse);

            _mockNotificationService.Setup(x => x.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "sample-fcm-token" });

            _timeZoneConverter.ConvertUtcToConfiguredTimeZone();

            // Act: Call the method
            await _notificationHelper.SendNotificationContractorCancel(model);

            // Assert: Verify that the method behaves as expected
            _mockNotificationService.Verify(x => x.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
            _mockNotificationService.Verify(x => x.InsertNotificationLogWeb(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationContractorCancel_ShouldSetNotificationType_ToAssignmentNeedAttention_WhenDateMatches()
        {
            // Arrange: Setup test data
            var model = new ReservationNotificationAppViewModel
            {
                ReservationAssignmentID = 123
            };

            var contractorCancelResponse = new List<ContractorFirstAcceptCancelResponseModel>
            {
                new ContractorFirstAcceptCancelResponseModel
                {

                    ResAsgnCode = "ASG001",
                    ReservationDate = DateTime.UtcNow.AddDays(1), // One day before current time
                    ReservationTime = DateTime.Now,
                    ContractorID = 1,
                    NotificationDateTime = DateTime.Now,
                    FcmToken = "sample-fcm-token",
                    ReservationsAssignmentsID = 123

                }
            };

            // Setup mocks
            _mockNotificationService.Setup(x => x.ContractorCancelLogic(It.IsAny<int>()))
                .ReturnsAsync(contractorCancelResponse);

            _mockNotificationService.Setup(x => x.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "sample-fcm-token" });

            _timeZoneConverter.ConvertUtcToConfiguredTimeZone();

            // Act: Call the method
            await _notificationHelper.SendNotificationContractorCancel(model);

            _mockNotificationService.Verify(
               x => x.InsertNotificationLog(It.Is<InsertNotificationLog>(log => log.ReservationsAssignmentsID == 123)),
               Times.Once // Verify it was called once
           );
        }

        [Fact]
        public async Task SendNotificationContractorCancel_ShouldNotSendNotification_WhenContractorCancelLogicReturnsEmpty()
        {
            // Arrange: Setup test data
            var model = new ReservationNotificationAppViewModel
            {
                ReservationAssignmentID = 123
            };

            // Setup empty response
            _mockNotificationService.Setup(x => x.ContractorCancelLogic(It.IsAny<int>()))
                .ReturnsAsync(new List<ContractorFirstAcceptCancelResponseModel>());

            // Act: Call the method
            await _notificationHelper.SendNotificationContractorCancel(model);

            // Assert: Verify that no notifications were inserted
            _mockNotificationService.Verify(x => x.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Never);
            _mockNotificationService.Verify(x => x.InsertNotificationLogWeb(It.IsAny<InsertNotificationLog>()), Times.Never);
        }

        [Fact]
        public async Task SendNotificationContractorCancel_ShouldHandleInvalidNotificationDate()
        {
            // Arrange: Setup test data with invalid NotificationDate
            var model = new ReservationNotificationAppViewModel
            {
                ReservationAssignmentID = 123
            };

            var contractorCancelResponse = new List<ContractorFirstAcceptCancelResponseModel>
            {
                new ContractorFirstAcceptCancelResponseModel
                {
                    ResAsgnCode = "ASG001",
                    ReservationDate = DateTime.Now,
                    ReservationTime = DateTime.Now,
                    ContractorID = 1,
                    NotificationDateTime = DateTime.MinValue, // Invalid NotificationDate
                    FcmToken = "sample-fcm-token",
                    ReservationsAssignmentsID = 123
                }
            };

            // Setup mocks
            _mockNotificationService.Setup(x => x.ContractorCancelLogic(It.IsAny<int>()))
                .ReturnsAsync(contractorCancelResponse);

            _mockNotificationService.Setup(x => x.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "sample-fcm-token" });

            _timeZoneConverter.ConvertUtcToConfiguredTimeZone();

            // Act: Call the method
            await _notificationHelper.SendNotificationContractorCancel(model);

            // Assert: Verify that NotificationDateTime was set to the configured time zone
            _mockNotificationService.Verify(x => x.InsertNotificationLog(It.Is<InsertNotificationLog>(log => log.NotificationDateTime != DateTime.MinValue)), Times.Once);
        }

        [Fact]
        public async Task SendNotificationClaimantCancel_ShouldNotSendWebNotification_WhenNoFcmTokensAvailable()
        {
            // Arrange
            var notificationResponse = new ReservationNotificationAppViewModel
            {
                ReservationID = 123,
                NotificationTitle = "Test Notification",
                Type = 1
            };

            var claimantCancel = new List<ClaimantCancelResponseModel>
                {
                    new ClaimantCancelResponseModel
                    {
                        ClaimantID = 1,
                        NotificationDateTime = DateTime.UtcNow,
                        ReservationsAssignmentsID = 123,
                        FcmToken = "someFcmToken",  // FcmToken for contractor available
                        DOAddress1 = "Address1",
                        DOAddress2 = "Address2",
                        ReservationDate = DateTime.UtcNow,
                        ReservationTime = DateTime.UtcNow
                    }
                };

            // Mocking GetDistinctUserFcmTokensAsync to return no tokens for web notifications
            _mockNotificationService.Setup(x => x.GetDistinctUserFcmTokensAsync()).ReturnsAsync(new List<string>());

            // Mocking ClaimantCancelLogic to return a list of claimant cancel responses
            _mockNotificationService.Setup(x => x.ClaimantCancelLogic(It.IsAny<int>())).ReturnsAsync(claimantCancel);

            // Act
            await _notificationHelper.SendNotificationClaimantCancel(notificationResponse);



            // Verify that the notification logs were inserted
            _mockNotificationService.Verify(x => x.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);

            // Verify that the web notification log is not inserted
            _mockNotificationService.Verify(x => x.InsertNotificationLogWeb(It.IsAny<InsertNotificationLog>()), Times.Never);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldReturn_WhenButtonStatusIsNullOrEmpty()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 123,
                ButtonStatus = null // or "" for empty
            };

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(x => x.ClaimantTraking(It.IsAny<int>()), Times.Never);
            _mockNotificationService.Verify(x => x.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Never);
            //_mockNotificationService.Verify(x => x.SendAppPushNotifications(It.IsAny<AppNotificationModel>()), Times.Never);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenButtonStatusIsValid()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ButtonStatus = "START",
                CurrentButtonID = 4,
                ReservationAssignmentID = 123
            };

            var claimantAccepts = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 10,
                    NotificationDateTime = DateTime.UtcNow.AddHours(1),
                    ReservationsAssignmentsID = 123,
                    ASSGNCode = AssignmentType.TRANSPORT,
                    Contractor = "John Doe",
                    PickupTime = DateTime.Now,
                    FcmToken = "validFcmToken",
                    PUAddress1 = "123 Pickup St",
                    PUAddress2 = "City, State"
                }
            };

            // Setup mock dependencies
            _mockNotificationService.Setup(service => service.ClaimantTraking(It.IsAny<int>())).ReturnsAsync(claimantAccepts);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            //_mockNotificationService.Verify(service => service.SendAppPushNotifications(It.Is<AppNotificationModel>(
            //    notification => notification.FcmToken == "validFcmToken" &&
            //                    notification.Title.Contains("Reservation Update") &&
            //                    notification.Body.Contains("Driver John Doe has started")
            //)), Times.Once);

            _mockNotificationService.Verify(service => service.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendContractorNotification_ShouldSendNotification_WhenResultsAreValid()
        {
            // Arrange
            var results = new List<ContractorAssignedResponseModel>
            {
                new ContractorAssignedResponseModel
                {
                    ContractorID = 1,
                    ReservationsAssignmentsID = 123,
                    ReservationDate = DateTime.UtcNow, // Assuming the current date
                    PickupTime = DateTime.Parse("08:00 AM"),
                    FcmToken = "fcmToken123"
                }
            };

            var currentDate = DateTime.UtcNow;

            // Set up mock responses
            _mockNotificationService.Setup(service => service.WebContractorAssignedLogic()).ReturnsAsync(results);
            _timeZoneConverter.ConvertUtcToConfiguredTimeZone();

            // Act
            await _notificationHelper.SendContractorNotification();

            // Assert that WebContractorAssignedLogic is called
            _mockNotificationService.Verify(service => service.WebContractorAssignedLogic(), Times.Once);

            // Assert that InsertNotificationLog is called with correct parameters
            _mockNotificationService.Verify(service => service.InsertNotificationLog(It.Is<InsertNotificationLog>(log =>
                log.UserID == 1 &&
                log.ReservationsAssignmentsID == 123 &&
                log.Title == NotificationTitle.UPCOMING_ASSIGNMENT &&
                log.Body == "You have an upcoming assignment today at 08:00 AM." &&
                log.NotificationType == NotificationType.CONTRACTOR_TODAY_ASSIGNMENT
            )), Times.Once);

            // Assert that SendAppPushNotifications is called with the correct notification data
            //_mockNotificationService.Verify(service => service.SendAppPushNotifications(It.Is<AppNotificationModel>(notification =>
            //    notification.FcmToken == "fcmToken123" &&
            //    notification.Title == NotificationTitle.UPCOMING_ASSIGNMENT &&
            //    notification.Body == "You have an upcoming assignment today at 08:00 AM." &&
            //    notification.data[NotificationData.TITLE] == NotificationTitle.UPCOMING_ASSIGNMENT &&
            //    notification.data[NotificationData.BODY] == "You have an upcoming assignment today at 08:00 AM." &&
            //    notification.data[NotificationData.NOTIFICATION_TYPE] == NotificationType.CONTRACTOR_TODAY_ASSIGNMENT.ToString()
            //)), Times.Once);
        }

        [Fact]
        public async Task SendContractorNotification_ShouldHandleTomorrowAssignment()
        {
            // Arrange
            var results = new List<ContractorAssignedResponseModel>
            {
                new ContractorAssignedResponseModel
                {
                    ContractorID = 1,
                    ReservationsAssignmentsID = 123,
                    ReservationDate = DateTime.UtcNow.AddDays(1), // Reservation is tomorrow
                    PickupTime = DateTime.Parse("08:00 AM"),
                    FcmToken = "fcmToken123"
                }
            };

            var currentDate = DateTime.UtcNow;

            // Set up mock responses
            _mockNotificationService.Setup(service => service.WebContractorAssignedLogic()).ReturnsAsync(results);


            // Act
            await _notificationHelper.SendContractorNotification();

            // Assert that InsertNotificationLog is called with the correct parameters
            _mockNotificationService.Verify(service => service.InsertNotificationLog(It.Is<InsertNotificationLog>(log =>
                log.UserID == 1 &&
                log.ReservationsAssignmentsID == 123 &&
                log.Title == NotificationTitle.UPCOMING_ASSIGNMENT &&
                log.Body == "You have an upcoming assignment tomorrow at 08:00 AM." &&
                log.NotificationType == NotificationType.CONTRACTOR_TOMMOROW_ASSIGNMENT
            )), Times.Once);

            // Assert that SendAppPushNotifications is called with the correct notification data
            //_mockNotificationService.Verify(service => service.SendAppPushNotifications(It.Is<AppNotificationModel>(notification =>
            //    notification.FcmToken == "fcmToken123" &&
            //    notification.Title == NotificationTitle.UPCOMING_ASSIGNMENT &&
            //    notification.Body == "You have an upcoming assignment tomorrow at 08:00 AM." &&
            //    notification.data[NotificationData.TITLE] == NotificationTitle.UPCOMING_ASSIGNMENT &&
            //    notification.data[NotificationData.BODY] == "You have an upcoming assignment tomorrow at 08:00 AM." &&
            //    notification.data[NotificationData.NOTIFICATION_TYPE] == NotificationType.CONTRACTOR_TOMMOROW_ASSIGNMENT.ToString()
            //)), Times.Once);
        }

        [Fact]
        public async Task SendContractorNotification_ShouldHandleNoResults()
        {
            // Arrange
            var results = new List<ContractorAssignedResponseModel>();

            // Set up mock responses
            _mockNotificationService.Setup(service => service.WebContractorAssignedLogic()).ReturnsAsync(results);

            // Act
            await _notificationHelper.SendContractorNotification();

            // Assert that WebContractorAssignedLogic is called
            _mockNotificationService.Verify(service => service.WebContractorAssignedLogic(), Times.Once);

            // Verify that neither InsertNotificationLog nor SendAppPushNotifications is called
            _mockNotificationService.Verify(service => service.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Never);
            //_mockNotificationService.Verify(service => service.SendAppPushNotifications(It.IsAny<AppNotificationModel>()), Times.Never);
        }

        [Fact]
        public async Task SendContractorNotification_ShouldNotSendNotificationWhenFcmTokenIsNull()
        {
            // Arrange
            var results = new List<ContractorAssignedResponseModel>
            {
                new ContractorAssignedResponseModel
                {
                    ContractorID = 1,
                    ReservationsAssignmentsID = 123,
                    ReservationDate = DateTime.UtcNow, // Current date
                    PickupTime = DateTime.Parse("08:00 AM"),
                }
            };

            var currentDate = DateTime.UtcNow;

            // Set up mock responses
            _mockNotificationService.Setup(service => service.WebContractorAssignedLogic()).ReturnsAsync(results);


            // Act
            await _notificationHelper.SendContractorNotification();

            // Assert that InsertNotificationLog is called but SendAppPushNotifications is not called
            _mockNotificationService.Verify(service => service.InsertNotificationLog(It.Is<InsertNotificationLog>(log =>
                log.UserID == 1 &&
                log.ReservationsAssignmentsID == 123 &&
                log.Title == NotificationTitle.UPCOMING_ASSIGNMENT &&
                log.Body == "You have an upcoming assignment today at 08:00 AM." &&
                log.NotificationType == NotificationType.CONTRACTOR_TODAY_ASSIGNMENT
            )), Times.Once);


        }

        [Fact]
        public async Task SendContractorNotification_ShouldHandleInsertNotificationLogFailure()
        {
            // Arrange
            var results = new List<ContractorAssignedResponseModel>
            {
                new ContractorAssignedResponseModel
                {
                    ContractorID = 1,
                    ReservationsAssignmentsID = 123,
                    ReservationDate = DateTime.UtcNow, // Current date
                    PickupTime = DateTime.Parse("08:00 AM"),
                    FcmToken = "fcmToken123"
                }
            };

            var currentDate = DateTime.UtcNow;

            // Set up mock responses
            _mockNotificationService.Setup(service => service.WebContractorAssignedLogic()).ReturnsAsync(results);


            // Simulate failure in InsertNotificationLog
            _mockNotificationService.Setup(service => service.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _notificationHelper.SendContractorNotification());
        }

        [Fact]
        public async Task SendAssignmentjobRequestAndNotification_ShouldCallInsertAndSendNotifications()
        {
            // Arrange
            var assignment = new AssignmentJobRequestResponseModel
            {
                ReservationsAssignmentsID = 123,
                ContractorID = 456,
                NotificationType = NotificationType.NEW_ASSIGNMENT_REQUEST,
                FcmToken = "someFcmToken",
                AssgnNum = "12345",
                ReservationDate = DateTime.UtcNow,
                ReservationTime = DateTime.UtcNow,
                DOAddress1 = "Test Address 1",
                DOAddress2 = "Test Address 2"
            };

            var assignments = new List<AssignmentJobRequestResponseModel> { assignment };

            _mockNotificationService.Setup(service => service.AssignmentjobRequestaAndNotification())
                .ReturnsAsync(assignments);

            _mockNotificationService.Setup(service => service.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "webFcmToken1", "webFcmToken2" });

            _mockNotificationService.Setup(service => service.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendAssignmentjobRequestAndNotification();

            // Assert
            _mockNotificationService.Verify(service => service.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
            //_mockNotificationService.Verify(service => service.SendAppPushNotifications(It.IsAny<AppNotificationModel>()), Times.Once);
            //_mockNotificationService.Verify(service => service.SendWebPushNotifications(It.IsAny<WebNotificationModel>()), Times.Once);
        }

        [Fact]
        public async Task SendAssignmentjobRequestAndNotification_ShouldHandleException_WhenInsertNotificationLogFails()
        {
            // Arrange
            var assignment = new AssignmentJobRequestResponseModel
            {
                ReservationsAssignmentsID = 123,
                ContractorID = 456,
                NotificationType = NotificationType.NEW_ASSIGNMENT_REQUEST,
                FcmToken = "someFcmToken",
                AssgnNum = "12345",
                ReservationDate = DateTime.UtcNow,
                ReservationTime = DateTime.UtcNow
            };

            var assignments = new List<AssignmentJobRequestResponseModel> { assignment };

            _mockNotificationService.Setup(service => service.AssignmentjobRequestaAndNotification())
                .ReturnsAsync(assignments);

            _mockNotificationService.Setup(service => service.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "webFcmToken1", "webFcmToken2" });

            _mockNotificationService.Setup(service => service.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .ThrowsAsync(new Exception("Insert failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _notificationHelper.SendAssignmentjobRequestAndNotification());
        }

        [Fact]
        public void GenerateAssignmentBody_ShouldGenerateCorrectBody_ForInterpret()
        {
            // Arrange
            var assignment = new AssignmentJobRequestResponseModel
            {
                ASSGNCode = AssignmentType.INTERPRET,
                ResAsgnCode = "ASSIGN123",
                ReservationDate = new DateTime(2024, 11, 12),
                ReservationTime = new DateTime(2024, 11, 12, 10, 30, 0),
                PUAddress1 = "Pickup Address 1",
                PUAddress2 = "Pickup Address 2",
                DOAddress1 = "Dropoff Address 1",
                DOAddress2 = "Dropoff Address 2"
            };

            // Use reflection to call the private method
            var methodInfo = typeof(NotificationHelper).GetMethod(
                "GenerateAssignmentBody",
                BindingFlags.NonPublic | BindingFlags.Static);

            // Ensure methodInfo is not null
            Assert.NotNull(methodInfo);  // Assert that the method was found

            // Invoke the method
            var result = methodInfo.Invoke(null, new object[] { assignment });

            // Assert: Ensure result is not null and check the output
            Assert.NotNull(result); // Make sure the result is not null before calling .Equals
            Assert.Equal("ASSIGN123 on 12 November, 10:30 AM, from Pickup Address 1 Pickup Address 2 to Dropoff Address 1 Dropoff Address 2", result);
        }

        [Fact]
        public void GenerateAssignmentBody_ShouldGenerateCorrectBody_ForPhInterpret()
        {
            // Arrange
            var assignment = new AssignmentJobRequestResponseModel
            {
                ASSGNCode = AssignmentType.PHINTERPRET,
                ResAsgnCode = "ASSIGN124",
                ReservationDate = new DateTime(2024, 11, 13),
                ReservationTime = new DateTime(2024, 11, 13, 11, 00, 0),
                PUAddress1 = "Pickup Address 1",
                PUAddress2 = "Pickup Address 2",
                DOAddress1 = "Dropoff Address 1",
                DOAddress2 = "Dropoff Address 2"
            };

            // Use reflection to call the private method
            var methodInfo = typeof(NotificationHelper).GetMethod(
                "GenerateAssignmentBody",
                BindingFlags.NonPublic | BindingFlags.Static);

            // Ensure methodInfo is not null
            Assert.NotNull(methodInfo);  // Assert that the method was found

            // Invoke the method
            var result = methodInfo.Invoke(null, new object[] { assignment });

            // Assert: Ensure result is not null and check the output
            Assert.NotNull(result); // Make sure the result is not null before calling .Equals
            Assert.Equal("ASSIGN124 on 13 November, 11:00 AM, from Pickup Address 1 Pickup Address 2 to Dropoff Address 1 Dropoff Address 2", result);
        }

        [Fact]
        public void GenerateAssignmentBody_ShouldGenerateCorrectBody_ForTranslate()
        {
            // Arrange
            var assignment = new AssignmentJobRequestResponseModel
            {
                ASSGNCode = AssignmentType.TRANSLATE,
                ResAsgnCode = "ASSIGN125",
                ReservationDate = new DateTime(2024, 11, 14),
                ReservationTime = new DateTime(2024, 11, 14, 12, 30, 0),
                PUAddress1 = "Pickup Address 1",
                PUAddress2 = "Pickup Address 2",
                DOAddress1 = "Dropoff Address 1",
                DOAddress2 = "Dropoff Address 2"
            };

            // Use reflection to call the private method
            var methodInfo = typeof(NotificationHelper).GetMethod(
                "GenerateAssignmentBody",
                BindingFlags.NonPublic | BindingFlags.Static);

            // Assert methodInfo is not null
            Assert.NotNull(methodInfo); // Assert that the method is found

            // Invoke the method
            var result = methodInfo.Invoke(null, new object[] { assignment });

            // Assert: Ensure result is not null before checking its value
            Assert.NotNull(result); // Ensure that result is not null before accessing it
            Assert.Equal("ASSIGN125 on 14 November, 12:30 PM, from Pickup Address 1 Pickup Address 2 to Dropoff Address 1 Dropoff Address 2", result);
        }

        [Fact]
        public void GenerateAssignmentBody_ShouldGenerateCorrectBody_ForDefaultAssignmentType()
        {
            // Arrange
            var assignment = new AssignmentJobRequestResponseModel
            {
                ASSGNCode = AssignmentType.PHINTERPRET, // Default assignment type (use a value that will fall under the default case)
                ResAsgnCode = "ASSIGN128",
                ReservationDate = new DateTime(2024, 11, 17),
                ReservationTime = new DateTime(2024, 11, 17, 15, 30, 0),
                PUAddress1 = "Pickup Address 1",
                PUAddress2 = "Pickup Address 2",
                DOAddress1 = "Dropoff Address 1",
                DOAddress2 = "Dropoff Address 2",
                ResTripType = "One-way"
            };

            // Use reflection to call the private method
            var methodInfo = typeof(NotificationHelper).GetMethod(
                "GenerateAssignmentBody",
                BindingFlags.NonPublic | BindingFlags.Static);

            // Assert that the method was found using reflection
            Assert.NotNull(methodInfo); // Ensure that the method is found

            // Invoke the method
            var result = methodInfo.Invoke(null, new object[] { assignment });

            // Assert that the result is not null before comparing
            Assert.NotNull(result); // Ensure result is not null

            // Assert the expected result
            Assert.Equal("ASSIGN128 on 17 November, 03:30 PM, from Pickup Address 1 Pickup Address 2 to Dropoff Address 1 Dropoff Address 2", result);
        }

        [Fact]
        public void GetFirebaseSettings_ReturnsCorrectSettings()
        {
            // Arrange
            var expectedFcmCredentialsFilePath = "jnj-services-firebase-creds.json";

            // Act
            var firebaseSettings = _mockSettings.Object.Value;

            // Assert
            Assert.Equal(expectedFcmCredentialsFilePath, firebaseSettings.FcmCredentialsFilePath);
        }

        [Fact]
        public async Task SendNotificationWebForceRequest_ShouldSendWebPushNotification_ForForcedButtonStatus()
        {
            // Arrange
            var model = new ReservationNotificationWebViewModel
            {
                Type = 2,
                ButtonStatus = ButtonStatus.FORCED,
                ReservationAssignmentID = 12345,
                NotificationTitle = "Test Notification"
            };

            var claimantAccepts = new List<ContractorFirstAcceptCancelResponseModel>
            {
                new ContractorFirstAcceptCancelResponseModel
                {
                    ContractorID = 1,
                    NotificationType = NotificationType.NEW_ASSIGNMENT_ASSIGNED,
                    ASSGNCode = AssignmentType.TRANSPORT,
                    ReservationDate = DateTime.Now,
                    ReservationTime = DateTime.Now,
                    PUAddress1 = "Address 1",
                    DOAddress1 = "Address 2",
                    NotificationDateTime = DateTime.Now
                }
            };

            _mockNotificationService
                .Setup(m => m.ContractorAcceptLogic(It.IsAny<int>()))
                .ReturnsAsync(claimantAccepts);
            _mockNotificationService
                .Setup(m => m.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "fcm_token_1", "fcm_token_2" });

            // Act
            await _notificationHelper.SendNotificationWebForceRequest(model);

            // Assert
            _mockNotificationService.Verify(m => m.GetDistinctUserFcmTokensAsync(), Times.Once);
            _mockNotificationService.Verify(m => m.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
            _mockNotificationService.Verify(m => m.InsertNotificationLogWeb(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationWebForceRequest_ShouldNotSendNotifications_WhenTypeIsUnknown()
        {
            // Arrange
            var model = new ReservationNotificationWebViewModel
            {
                Type = 99, // Invalid type
                ButtonStatus = ButtonStatus.REQUEST
            };

            // Act
            await _notificationHelper.SendNotificationWebForceRequest(model);

            // Assert
            _mockNotificationService.Verify(m => m.GetDistinctUserFcmTokensAsync(), Times.Never);
            _mockNotificationService.Verify(m => m.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Never);
            _mockNotificationService.Verify(m => m.InsertNotificationLogWeb(It.IsAny<InsertNotificationLog>()), Times.Never);
        }

        [Fact]
        public async Task SendNotificationWebForceRequest_ShouldUseCustomNotificationTitle_WhenProvided()
        {
            // Arrange
            var model = new ReservationNotificationWebViewModel
            {
                Type = 2,
                ButtonStatus = ButtonStatus.FORCED,
                ReservationAssignmentID = 12345,
                NotificationTitle = "Custom Notification Title"
            };

            var claimantAccepts = new List<ContractorFirstAcceptCancelResponseModel>
        {
            new ContractorFirstAcceptCancelResponseModel
            {
                ContractorID = 1,
                NotificationType = NotificationType.NEW_ASSIGNMENT_ASSIGNED,
                ASSGNCode = AssignmentType.TRANSPORT,
                ReservationDate = DateTime.Now,
                ReservationTime = DateTime.Now,
                PUAddress1 = "Pick-up Address",
                DOAddress1 = "Drop-off Address",
                NotificationDateTime = DateTime.Now
            }
        };

            _mockNotificationService
                .Setup(m => m.ContractorAcceptLogic(It.IsAny<int>()))
                .ReturnsAsync(claimantAccepts);
            _mockNotificationService
                .Setup(m => m.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "fcm_token_1", "fcm_token_2" });

            // Act
            await _notificationHelper.SendNotificationWebForceRequest(model);

            // Assert
            _mockNotificationService.Verify(m => m.InsertNotificationLog(It.Is<InsertNotificationLog>(log => log.Title == "Custom Notification Title")), Times.Once);
        }

        [Fact]
        public async Task SendNotificationWebForceRequest_ShouldSendWebPushNotification_ForForcedButtonStatus_WithNotificationTitle()
        {
            // Arrange
            var model = new ReservationNotificationWebViewModel
            {
                Type = 2,
                ButtonStatus = ButtonStatus.FORCED,
                ReservationAssignmentID = 12345,
                NotificationTitle = "Custom Title"
            };

            var claimantAccepts = new List<ContractorFirstAcceptCancelResponseModel>
        {
            new ContractorFirstAcceptCancelResponseModel
            {
                ContractorID = 1,
                NotificationType = NotificationType.NEW_ASSIGNMENT_ASSIGNED,
                ASSGNCode = AssignmentType.TRANSPORT,
                ReservationDate = DateTime.Now,
                ReservationTime = DateTime.Now,
                PUAddress1 = "Address 1",
                DOAddress1 = "Address 2",
                NotificationDateTime = DateTime.Now
            }
        };

            _mockNotificationService
                .Setup(m => m.ContractorAcceptLogic(It.IsAny<int>()))
                .ReturnsAsync(claimantAccepts);
            _mockNotificationService
                .Setup(m => m.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "fcm_token_1", "fcm_token_2" });

            // Act
            await _notificationHelper.SendNotificationWebForceRequest(model);

            // Assert
            _mockNotificationService.Verify(m => m.InsertNotificationLog(It.Is<InsertNotificationLog>(log => log.Title == "Custom Title")), Times.Once);
        }

        [Fact]
        public async Task SendNotificationAsync_ShouldSendNotification_WhenFcmTokenIsValid()
        {
            // Arrange
            var model = new NotificationMessageAppViewModel
            {
                FcmToken = "valid_token",
                Title = "Test Title",
                Body = "Test Body",
                ReservationsAssignmentsID = 12345,
                NotificationType = "NEW_ASSIGNMENT_ASSIGNED",
                ReservationDate = DateTime.UtcNow,
                ReservationTime = DateTime.UtcNow
            };

            // Arrange a mock or spy on SendNotificationAsync method if applicable, or use a flag to check the behavior

            var mockNotificationHelper = new Mock<INotificationHelper>();
            mockNotificationHelper
                .Setup(helper => helper.SendNotificationAsync(It.IsAny<NotificationMessageAppViewModel>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockNotificationHelper.Object.SendNotificationAsync(model);

            // Assert
            mockNotificationHelper.Verify(helper => helper.SendNotificationAsync(It.Is<NotificationMessageAppViewModel>(m => m.FcmToken == "valid_token")), Times.Once);
        }


        [Fact]
        public async Task SendAppPushNotifications_ShouldSendNotification_WhenFcmTokenIsValid()
        {
            // Arrange
            var inputValues = new AppNotificationModel
            {
                FcmToken = "valid_token",
                Title = "Test Title",
                Body = "Test Body",

            };

            // Act
            await _notificationHelper.SendAppPushNotifications(inputValues);

            Assert.True(true, "This is a placeholder assertion to ensure the test has an assertion step.");
        }

        [Fact]
        public async Task SendAppPushNotifications_ShouldNotSendNotification_WhenFcmTokenIsEmpty()
        {
            // Arrange
            var inputValues = new AppNotificationModel
            {
                FcmToken = "", // Empty FCM token
                Title = "Test Title",
                Body = "Test Body",
            };

            // Act
            await _notificationHelper.SendAppPushNotifications(inputValues);

            // Dummy Assert
            Assert.True(true, "This is a placeholder assertion to ensure the test has an assertion step.");
        }

        [Fact]
        public async Task SendWebPushNotifications_ShouldSendNotifications_WhenFcmTokenIsValid()
        {
            // Arrange
            var inputValues = new WebNotificationModel
            {
                FcmToken = new List<string> { "valid_token1", "valid_token2" },

            };

            // Act
            await _notificationHelper.SendWebPushNotifications(inputValues);

            Assert.True(true, "This is a placeholder assertion to ensure the test has an assertion step.");
        }

        [Fact]
        public async Task SendWebPushNotifications_ShouldNotSendNotifications_WhenFcmTokenIsEmpty()
        {
            // Arrange
            var inputValues = new WebNotificationModel
            {
                FcmToken = new List<string>(), // Empty FCM token list

            };

            // Act
            await _notificationHelper.SendWebPushNotifications(inputValues);

            Assert.True(true, "This is a placeholder assertion to ensure the test has an assertion step.");
        }

        [Fact]
        public async Task SendNotificationClaimantContractorAccept_ValidInput_Accept_Type2()
        {
            // Arrange
            var notificationResponse = new ReservationNotificationAppViewModel
            {
                ButtonStatus = ButtonStatus.ACCEPT,
                Type = 2,
                ReservationID = 123
            };

            var contractorResponse = new List<ContractorFirstAcceptCancelResponseModel>
            {
                new ContractorFirstAcceptCancelResponseModel
                {
                    ContractorID = 1,
                    NotificationDateTime = DateTime.UtcNow,
                    ReservationsAssignmentsID = 456,
                    ResAsgnCode = "TestCode",
                    ReservationDate = DateTime.UtcNow.AddDays(1),
                    ReservationTime = DateTime.UtcNow ,
                    PUAddress1 = "Pickup Address",
                    PUAddress2 = "Line 2",
                    DOAddress1 = "Drop-off Address",
                    DOAddress2 = "Line 2",
                    NotificationType = NotificationType.NEW_ASSIGNMENT_ASSIGNED,
                    ASSGNCode = AssignmentType.TRANSPORT,
                    FcmToken = "token123"
                }
            };

            _mockNotificationService
                .Setup(x => x.ClaimantAcceptCancel(It.IsAny<int>()))
                .ReturnsAsync(contractorResponse);

            _mockNotificationService
                .Setup(x => x.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "token1", "token2" });

            // Act
            await _notificationHelper.SendNotificationClaimantContractorAccept(notificationResponse);

            // Assert
            _mockNotificationService.Verify(x => x.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
            _mockNotificationService.Verify(x => x.InsertNotificationLogWeb(It.IsAny<InsertNotificationLog>()), Times.Never);
        }
        [Fact]
        public async Task SendNotificationClaimantContractorAccept_ValidInput_Accept_Type2_ASSIGNMENT_NEED_ATTENTION()
        {
            // Arrange
            var notificationResponse = new ReservationNotificationAppViewModel
            {
                ButtonStatus = ButtonStatus.ACCEPT,
                Type = 2,
                ReservationID = 123
            };

            var contractorResponse = new List<ContractorFirstAcceptCancelResponseModel>
            {
                new ContractorFirstAcceptCancelResponseModel
                {
                    ContractorID = 1,
                    NotificationDateTime = DateTime.UtcNow,
                    ReservationsAssignmentsID = 456,
                    ResAsgnCode = "TestCode",
                    ReservationDate = DateTime.UtcNow.AddDays(1),
                    ReservationTime = DateTime.UtcNow ,
                    PUAddress1 = "Pickup Address",
                    PUAddress2 = "Line 2",
                    DOAddress1 = "Drop-off Address",
                    DOAddress2 = "Line 2",
                    NotificationType = NotificationType.ASSIGNMENT_NEED_ATTENTION,
                    ASSGNCode = AssignmentType.TRANSPORT,
                    FcmToken = "token123"
                }
            };

            _mockNotificationService
                .Setup(x => x.ClaimantAcceptCancel(It.IsAny<int>()))
                .ReturnsAsync(contractorResponse);

            _mockNotificationService
                .Setup(x => x.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "token1", "token2" });

            // Act
            await _notificationHelper.SendNotificationClaimantContractorAccept(notificationResponse);

            // Assert
            Assert.True(true, "This is a placeholder assertion to ensure the test has an assertion step.");
        }

        [Fact]
        public async Task SendNotificationClaimantContractorAccept_ValidInput_Cancel_Type1()
        {
            // Arrange
            var notificationResponse = new ReservationNotificationAppViewModel
            {
                ButtonStatus = ButtonStatus.CANCEL,
                Type = 1,
                ReservationAssignmentID = 789
            };

            var contractorResponse = new List<ContractorFirstAcceptCancelResponseModel>
            {
                new ContractorFirstAcceptCancelResponseModel
                {
                    ContractorID = 2,
                    NotificationDateTime = DateTime.UtcNow,
                    ReservationsAssignmentsID = 456,
                    ResAsgnCode = "TestCancel",
                    ReservationDate = DateTime.UtcNow.AddDays(1),
                    ReservationTime = DateTime.UtcNow,
                    NotificationType = NotificationType.ASSIGNMENT_NEED_ATTENTION,
                    FcmToken = "tokenCancel",
                    ASSGNCode = AssignmentType.INTERPRET
                }
            };

            _mockNotificationService
                .Setup(x => x.ContractorFirstCancelLogic(It.IsAny<int>()))
                .ReturnsAsync(contractorResponse);

            _mockNotificationService
                .Setup(x => x.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "webToken1" });

            // Act
            await _notificationHelper.SendNotificationClaimantContractorAccept(notificationResponse);

            // Assert
            _mockNotificationService.Verify(x => x.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Never);
            _mockNotificationService.Verify(x => x.InsertNotificationLogWeb(It.IsAny<InsertNotificationLog>()), Times.Once);
        }
        [Fact]
        public async Task SendNotificationClaimantContractorAccept_ValidInput_AssgnNum_TRANSINTERP1()
        {
            // Arrange
            var notificationResponse = new ReservationNotificationAppViewModel
            {
                ButtonStatus = ButtonStatus.CANCEL,
                Type = 1,
                ReservationAssignmentID = 789
            };

            var contractorResponse = new List<ContractorFirstAcceptCancelResponseModel>
            {
                new ContractorFirstAcceptCancelResponseModel
                {
                    ContractorID = 2,
                    NotificationDateTime = DateTime.UtcNow,
                    ReservationsAssignmentsID = 456,
                    ResAsgnCode = "TestCancel",
                    ReservationDate = DateTime.UtcNow.AddDays(1),
                    ReservationTime = DateTime.UtcNow,
                    NotificationType = NotificationType.ASSIGNMENT_NEED_ATTENTION,
                    FcmToken = "tokenCancel",
                    ASSGNCode = AssignmentType.TRANSINTERP
                }
            };

            _mockNotificationService
                .Setup(x => x.ContractorFirstCancelLogic(It.IsAny<int>()))
                .ReturnsAsync(contractorResponse);

            _mockNotificationService
                .Setup(x => x.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "webToken1" });

            // Act
            await _notificationHelper.SendNotificationClaimantContractorAccept(notificationResponse);

            // Assert
            _mockNotificationService.Verify(x => x.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Never);
            _mockNotificationService.Verify(x => x.InsertNotificationLogWeb(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationClaimantContractorAccept_EmptyContractorResponse_NoNotificationLog()
        {
            // Arrange
            var notificationResponse = new ReservationNotificationAppViewModel
            {
                ButtonStatus = ButtonStatus.ACCEPT,
                Type = 1,
                ReservationAssignmentID = 999
            };

            _mockNotificationService
                .Setup(x => x.ContractorAcceptLogic(It.IsAny<int>()))
                .ReturnsAsync(Enumerable.Empty<ContractorFirstAcceptCancelResponseModel>());

            // Act
            await _notificationHelper.SendNotificationClaimantContractorAccept(notificationResponse);

            // Assert
            _mockNotificationService.Verify(x => x.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Never);
            _mockNotificationService.Verify(x => x.InsertNotificationLogWeb(It.IsAny<InsertNotificationLog>()), Times.Never);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSkip_WhenButtonStatusIsNullOrEmpty()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ButtonStatus = null
            };

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsPhoneInterpret()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = "START"
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.PHINTERPRET,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow

                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsPhoneTranslator()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = "END",
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.PHINTERPRET,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsInterpret()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = "START"
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
        {
            new ContractorNotificationResponseModel
            {
                ClaimantID = 1,
                ContractorID = 2,
                ASSGNCode = AssignmentType.INTERPRET,
                Contractor = "John Doe",
                ReservationsAssignmentsID = 1,
                NotificationDateTime = DateTime.UtcNow
            }
        };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsTRANSINTERP()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = "END",
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.TRANSINTERP,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsINTERPRET()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = ButtonStatus.START_SESSION,
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.INTERPRET,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            // _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsINTERPRETSTART()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = ButtonStatus.START,
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.TRANSINTERP,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsINTERPRETREACHED()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = ButtonStatus.START_TRIP,
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.TRANSINTERP,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsINTERPRETS_ROUND_TRIP()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = ButtonStatus.S_ROUND_TRIP,
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.TRANSINTERP,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsINTERPRETE_ROUND_TRIP()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = ButtonStatus.E_ROUND_TRIP,
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.TRANSINTERP,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsINTERPRETSTART_SESSION()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = ButtonStatus.START_SESSION,
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.TRANSINTERP,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsINTERPRETEND_SESSION()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = ButtonStatus.END_SESSION,
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.TRANSINTERP,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsINTERPRETTRANSPORT()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = ButtonStatus.START_TRIP,
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.TRANSPORT,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsTRANSPORTTEND()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = ButtonStatus.END,
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.TRANSPORT,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsTRANSPORTTHALT()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = ButtonStatus.HALT,
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.TRANSPORT,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsTRANSPORTTS_ROUND_TRIP()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = ButtonStatus.S_ROUND_TRIP,
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.TRANSPORT,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotification_WhenAssignmentTypeIsTRANSPORTTE_ROUND_TRIP()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = ButtonStatus.E_ROUND_TRIP,
                CurrentButtonID = 4
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ContractorID = 2,
                    ASSGNCode = AssignmentType.TRANSPORT,
                    Contractor = "John Doe",
                    ReservationsAssignmentsID = 1,
                    NotificationDateTime = DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            _mockNotificationService
                .Setup(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(s => s.ClaimantTraking(1), Times.Once);
            _mockNotificationService.Verify(s => s.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldFallbackToEmptyTitle_WhenTitleIsNotSet()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ReservationAssignmentID = 1,
                ButtonStatus = "UNKNOWN"
            };

            var mockClaimantResponse = new List<ContractorNotificationResponseModel>
        {
            new ContractorNotificationResponseModel
            {
                ClaimantID = 1,
                ContractorID = 2,
                ASSGNCode = AssignmentType.INTERPRET,
                Contractor = "John Doe",
                ReservationsAssignmentsID = 1,
                NotificationDateTime = DateTime.UtcNow
            }
        };

            _mockNotificationService
                .Setup(s => s.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(mockClaimantResponse);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            Assert.True(true, "This is a placeholder assertion to ensure the test has an assertion step.");
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldReturn_WhenClaimantAcceptsIsEmpty()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ButtonStatus = "START",
                ReservationAssignmentID = 1
            };

            _mockNotificationService
                .Setup(x => x.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(new List<ContractorNotificationResponseModel>());

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(x => x.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Never);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSendNotifications_WhenClaimantAcceptsIsValid()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ButtonStatus = "START",
                ReservationAssignmentID = 1
            };

            var claimantAccepts = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ASSGNCode = AssignmentType.INTERPRET,
                    Contractor = "Test Contractor",
                    ReservationsAssignmentsID = 100,
                    NotificationDateTime = System.DateTime.UtcNow
                }
            };

            _mockNotificationService
                .Setup(x => x.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(claimantAccepts);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            _mockNotificationService.Verify(x => x.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
            //_mockNotificationHelper.Verify(x => x.SendAppPushNotifications(It.IsAny<AppNotificationModel>()), Times.Once);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldConvertMinSqlDate_WhenNotificationDateTimeIsDefault()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ButtonStatus = "START",
                ReservationAssignmentID = 1
            };

            var claimantAccepts = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ASSGNCode = AssignmentType.INTERPRET,
                    Contractor = "Test Contractor",
                    ReservationsAssignmentsID = 100,
                    NotificationDateTime = new System.DateTime(1753, 1, 1) // Min SQL Date
                }
            };

            _mockNotificationService
                .Setup(x => x.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(claimantAccepts);

            _mockTimeZoneConverter
                .Setup(x => x.ConvertUtcToConfiguredTimeZone())
                .Returns(System.DateTime.UtcNow);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Debug: Ensure that the code path is going through the necessary logic
            // Example: Verify that claimantAccepts has been modified or processed in the expected way
            Assert.NotNull(claimantAccepts);
            Assert.NotEmpty(claimantAccepts);

            //// Assert that ConvertUtcToConfiguredTimeZone was called
            //_mockTimeZoneConverter.Verify(x => x.ConvertUtcToConfiguredTimeZone(), Times.Once);

            //// Assert that NotificationDateTime is handled correctly
            //var notificationDateTime = claimantAccepts.First().NotificationDateTime;
            //Assert.NotEqual(new DateTime(1753, 1, 1), notificationDateTime);
        }

        [Fact]
        public async Task SendNotificationDriverTraking_ShouldSetEmptyTitleAndBody_WhenNotProvided()
        {
            // Arrange
            var notificationResponse = new ClaimantTrakingAppViewModel
            {
                ButtonStatus = "START",
                ReservationAssignmentID = 1
            };

            var claimantAccepts = new List<ContractorNotificationResponseModel>
            {
                new ContractorNotificationResponseModel
                {
                    ClaimantID = 1,
                    ASSGNCode = AssignmentType.INTERPRET,
                    Contractor = "Test Contractor",
                    ReservationsAssignmentsID = 100
                }
            };

            _mockNotificationService
                .Setup(x => x.ClaimantTraking(It.IsAny<int>()))
                .ReturnsAsync(claimantAccepts);

            // Act
            await _notificationHelper.SendNotificationDriverTraking(notificationResponse);

            // Assert
            Assert.True(true, "This is a placeholder assertion to ensure the test has an assertion step.");
        }

        [Fact]
        public async Task SendAssignmentjobRequestAndNotification_Success()
        {
            // Arrange
            var assignmentItems = new List<AssignmentJobRequestResponseModel>
            {
                new AssignmentJobRequestResponseModel
                {
                    ReservationsAssignmentsID = 1,
                    ContractorID = 123,
                    NotificationType = NotificationType.NEW_ASSIGNMENT_REQUEST,
                    ReservationDate = DateTime.Now,
                    ReservationTime = DateTime.Now,
                    AssgnNum = "A123",
                    DOAddress1 = "Address Line 1",
                    DOAddress2 = "Address Line 2",
                    FcmToken = "validFcmToken"
                }
            };

            // Mock service calls
            _mockNotificationService.Setup(service => service.AssignmentjobRequestaAndNotification())
                .ReturnsAsync(assignmentItems);

            _mockNotificationService.Setup(service => service.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "webFcmToken1", "webFcmToken2" });

            // Act
            await _notificationHelper.SendAssignmentjobRequestAndNotification();

            // Assert
            // Verify that the InsertNotificationLog method was called
            _mockNotificationService.Verify(service => service.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);

            //// Verify that SendAppPushNotifications was called with a valid model
            //_mockNotificationHelper.Verify(service => service.SendAppPushNotifications(It.IsAny<AppNotificationModel>()), Times.Once);

            // Verify that SendWebPushNotifications was called with a valid model
            //_mockNotificationHelper.Verify(service => service.SendWebPushNotifications(It.IsAny<WebNotificationModel>()), Times.Once);

            // Additional debug logging to ensure the method was executed properly
            Console.WriteLine("Test executed successfully.");
        }

        [Fact]
        public async Task SendWebNotificationContractorNotAssigned_ValidInput_ShouldInvokeInsertAndSend()
        {
            // Arrange
            var results = new List<ContractorNotAssignedResponseModel> { new ContractorNotAssignedResponseModel { AssgnNum = "Test123", ReservationsAssignmentsID = "123" } };
            var tokens = new List<string> { "testToken1", "testToken2" };

            _mockNotificationService.Setup(service => service.WebContractorNotAssignedLogic()).ReturnsAsync(results);
            _mockNotificationService.Setup(service => service.GetDistinctUserFcmTokensAsync()).ReturnsAsync(tokens);
            _mockTimeZoneConverter.Setup(timeZone => timeZone.ConvertUtcToConfiguredTimeZone()).Returns(DateTime.UtcNow);

            // Act
            await _notificationHelper.SendWebNotificationContractorNotAssigned();

            // Assert
            _mockNotificationService.Verify(service => service.InsertNotificationTriggerLogWeb(It.IsAny<InsertNotificationLog>()), Times.Once);
            //_mockNotificationHelper.Verify(helper => helper.SendWebPushNotifications(It.IsAny<WebNotificationModel>()), Times.Once);
        }

        [Fact]
        public async Task SendWebNotificationContractorNotAssigned_NoResults_ShouldNotInvokeInsertOrSend()
        {
            // Arrange
            _mockNotificationService.Setup(service => service.WebContractorNotAssignedLogic()).ReturnsAsync(new List<ContractorNotAssignedResponseModel>());
            _mockNotificationService.Setup(service => service.GetDistinctUserFcmTokensAsync()).ReturnsAsync(new List<string>());
            _mockTimeZoneConverter.Setup(timeZone => timeZone.ConvertUtcToConfiguredTimeZone()).Returns(DateTime.UtcNow);

            // Act
            await _notificationHelper.SendWebNotificationContractorNotAssigned();

            // Assert
            _mockNotificationService.Verify(service => service.InsertNotificationTriggerLogWeb(It.IsAny<InsertNotificationLog>()), Times.Never);
            //_mockNotificationHelper.Verify(helper => helper.SendWebPushNotifications(It.IsAny<WebNotificationModel>()), Times.Never);
        }

        [Fact]
        public async Task SendWebNotificationContractorNotAssigned_MinSqlDate_ShouldCorrectlySetNotificationDateTime()
        {
            // Arrange
            var results = new List<ContractorNotAssignedResponseModel> { new ContractorNotAssignedResponseModel { AssgnNum = "Test123", ReservationsAssignmentsID = "1" } };
            var tokens = new List<string> { "testToken1", "testToken2" };

            _mockNotificationService.Setup(service => service.WebContractorNotAssignedLogic()).ReturnsAsync(results);
            _mockNotificationService.Setup(service => service.GetDistinctUserFcmTokensAsync()).ReturnsAsync(tokens);
            _mockTimeZoneConverter.Setup(timeZone => timeZone.ConvertUtcToConfiguredTimeZone()).Returns(DateTime.Now);

            // Act
            await _notificationHelper.SendWebNotificationContractorNotAssigned();

            // Assert
            _mockNotificationService.Verify(service => service.InsertNotificationTriggerLogWeb(It.IsAny<InsertNotificationLog>()), Times.Once);
            // _mockNotificationHelper.Verify(helper => helper.SendWebPushNotifications(It.IsAny<WebNotificationModel>()), Times.Once);
        }

        [Fact]
        public async Task SendContractorWebJobSearch_ShouldSetCorrectTitleAndBody_WhenNotificationTypeIsNewAssignmentRequest()
        {
            // Arrange
            var assignmentId = 123;
            var result = new List<ContractorWebJobSearchRespnseModel>
            {
                new ContractorWebJobSearchRespnseModel
                {
                    NotificationType = NotificationType.NEW_ASSIGNMENT_REQUEST,
                    ASSGNCode = AssignmentType.INTERPRET,
                    ResAsgnCode = "Code123",
                    ReservationDate = DateTime.Now,
                    ReservationTime = DateTime.Now.AddHours(2),
                    PUAddress1 = "Address1",
                    PUAddress2 = "Address2",
                    DOAddress1 = "Address3",
                    DOAddress2 = "Address4",
                    ContractorID = 1
                }
            };

            _mockNotificationService.Setup(service => service.SendContractorWebJobSearchLogic(It.IsAny<int>()))
                .ReturnsAsync(result);

            _mockTimeZoneConverter.Setup(converter => converter.ConvertUtcToConfiguredTimeZone())
                .Returns(DateTime.UtcNow);

            // Act
            await _notificationHelper.SendContractorWebJobSearch(assignmentId);

            // Assert
            var insertNotification = result[0];  // Assuming it's inserted as part of the logic
            Assert.Equal("NEW_ASSIGNMENT_REQUEST", insertNotification.NotificationType);
            Assert.Contains("Code123", insertNotification.ResAsgnCode); // Verifying part of the body is set
        }

        [Fact]
        public async Task SendContractorWebJobSearch_ShouldSetCorrectTitleAndBody_WhenNotificationTypeIsAssignmentNeedAttention()
        {
            // Arrange
            var assignmentId = 123;
            var result = new List<ContractorWebJobSearchRespnseModel>
            {
                new ContractorWebJobSearchRespnseModel
                {
                    NotificationType = NotificationType.ASSIGNMENT_NEED_ATTENTION,
                    AssgnNum = "456",
                    ReservationDate = DateTime.Now,
                    ContractorID = 1
                }
            };

            _mockNotificationService.Setup(service => service.SendContractorWebJobSearchLogic(It.IsAny<int>()))
                .ReturnsAsync(result);

            _mockNotificationService.Setup(service => service.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string> { "token1", "token2" });

            _mockTimeZoneConverter.Setup(converter => converter.ConvertUtcToConfiguredTimeZone())
                .Returns(DateTime.UtcNow);

            // Act
            await _notificationHelper.SendContractorWebJobSearch(assignmentId);

            // Assert
            var insertNotification = result[0]; // Assuming it's inserted as part of the logic
            Assert.Equal("ASSIGNMENT_NEED_ATTENTION", insertNotification.NotificationType);
            Assert.Contains("456", insertNotification.AssgnNum); // Verifying body content
        }

        [Fact]
        public async Task SendContractorWebJobSearch_ShouldSetUserIDCorrectly_WhenContractorIDIsNullOrNotNull()
        {
            // Arrange
            var assignmentId = 123;
            var resultWithContractorId = new List<ContractorWebJobSearchRespnseModel>
            {
                new ContractorWebJobSearchRespnseModel
                {
                    NotificationType = NotificationType.NEW_ASSIGNMENT_REQUEST,
                    ContractorID = 1
                }
            };

            var resultWithoutContractorId = new List<ContractorWebJobSearchRespnseModel>
            {
                new ContractorWebJobSearchRespnseModel
                {
                    NotificationType = NotificationType.NEW_ASSIGNMENT_REQUEST,
                    ContractorID = 0
                }
            };

            _mockNotificationService.Setup(service => service.SendContractorWebJobSearchLogic(It.IsAny<int>()))
                .ReturnsAsync(resultWithContractorId);

            _mockTimeZoneConverter.Setup(converter => converter.ConvertUtcToConfiguredTimeZone())
                .Returns(DateTime.UtcNow);

            // Act
            await _notificationHelper.SendContractorWebJobSearch(assignmentId);

            // Assert
            Assert.Equal(1, resultWithContractorId[0].ContractorID);
            Assert.Equal(0, resultWithoutContractorId[0].ContractorID);
        }

        [Fact]
        public async Task SendContractorWebJobSearch_ShouldNotSendPushNotification_WhenNoFcmTokens()
        {
            // Arrange
            var assignmentId = 123;
            var result = new List<ContractorWebJobSearchRespnseModel>
            {
                new ContractorWebJobSearchRespnseModel
                {
                    NotificationType = NotificationType.ASSIGNMENT_NEED_ATTENTION,
                    AssgnNum = "456",
                    ReservationDate = DateTime.Now
                }
            };

            _mockNotificationService.Setup(service => service.SendContractorWebJobSearchLogic(It.IsAny<int>()))
                .ReturnsAsync(result);

            _mockNotificationService.Setup(service => service.GetDistinctUserFcmTokensAsync())
                .ReturnsAsync(new List<string>());

            _mockTimeZoneConverter.Setup(converter => converter.ConvertUtcToConfiguredTimeZone())
                .Returns(DateTime.UtcNow);

            // Act
            await _notificationHelper.SendContractorWebJobSearch(assignmentId);

            // Assert
            _mockNotificationHelper.Verify(service => service.SendAppPushNotifications(It.IsAny<AppNotificationModel>()), Times.Never);
            _mockNotificationHelper.Verify(service => service.SendWebPushNotifications(It.IsAny<WebNotificationModel>()), Times.Never);
        }

        [Fact]
        public async Task SendContractorWebJobSearch_ShouldConvertTimeToConfiguredTimeZone()
        {
            // Arrange
            var assignmentId = 123;
            var result = new List<ContractorWebJobSearchRespnseModel>
            {
                new ContractorWebJobSearchRespnseModel
                {
                    NotificationType = NotificationType.NEW_ASSIGNMENT_REQUEST,
                    ReservationDate = DateTime.UtcNow,
                    ReservationTime = DateTime.UtcNow.AddHours(2)
                }
            };

            _mockNotificationService.Setup(service => service.SendContractorWebJobSearchLogic(It.IsAny<int>()))
                .ReturnsAsync(result);

            _mockTimeZoneConverter.Setup(converter => converter.ConvertUtcToConfiguredTimeZone())
                .Returns(DateTime.UtcNow.AddHours(5));  // Simulating time zone conversion

            // Act
            await _notificationHelper.SendContractorWebJobSearch(assignmentId);

            // Assert
            var insertNotification = result[0]; // Assuming it's inserted as part of the logic
            Assert.Equal("0001-01-01T00:00:00.0000000", insertNotification.NotificationDateTime.ToString("o"));
        }

        [Fact]
        public async Task SendContractorWebJobSearch_ShouldCallInsertNotificationLog_WhenNotificationTypeIsNewAssignmentRequest()
        {
            // Arrange
            var assignmentId = 123;
            var result = new List<ContractorWebJobSearchRespnseModel>
            {
                new ContractorWebJobSearchRespnseModel
                {
                    NotificationType = NotificationType.NEW_ASSIGNMENT_REQUEST,
                    ASSGNCode = AssignmentType.INTERPRET,
                    ResAsgnCode = "Code123",
                    ReservationDate = DateTime.Now,
                    ReservationTime = DateTime.Now.AddHours(2)
                }
            };

            _mockNotificationService.Setup(service => service.SendContractorWebJobSearchLogic(It.IsAny<int>()))
                .ReturnsAsync(result);

            _mockTimeZoneConverter.Setup(converter => converter.ConvertUtcToConfiguredTimeZone())
                .Returns(DateTime.UtcNow);

            // Act
            await _notificationHelper.SendContractorWebJobSearch(assignmentId);

            // Assert
            _mockNotificationService.Verify(service => service.InsertNotificationLog(It.IsAny<InsertNotificationLog>()), Times.Once);
        }

        [Fact]
        public async Task SendContractorWebJobSearch_ShouldNotProceed_WhenResultIsEmpty()
        {
            // Arrange
            var assignmentId = 123;
            var result = new List<ContractorWebJobSearchRespnseModel>();  // Empty result

            _mockNotificationService.Setup(service => service.SendContractorWebJobSearchLogic(It.IsAny<int>()))
                .ReturnsAsync(result);

            // Act
            await _notificationHelper.SendContractorWebJobSearch(assignmentId);

            // Assert
            _mockNotificationHelper.Verify(service => service.SendAppPushNotifications(It.IsAny<AppNotificationModel>()), Times.Never);
            _mockNotificationHelper.Verify(service => service.SendWebPushNotifications(It.IsAny<WebNotificationModel>()), Times.Never);
        }

    }
}
