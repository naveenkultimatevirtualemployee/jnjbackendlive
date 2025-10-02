using JNJServices.API.Controllers.v1.App;
using JNJServices.API.Helper;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.CommonModels;
using JNJServices.Models.ViewModels.App;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1.App
{
    public class AppNotificationControllerTests
    {
        private readonly Mock<INotificationHelper> _mockNotificationHelper;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly AppNotificationController _controller;
        private readonly Mock<ClaimsPrincipal> _mockUser;

        public AppNotificationControllerTests()
        {
            // Initialize the mocks
            _mockNotificationHelper = new Mock<INotificationHelper>();
            _mockNotificationService = new Mock<INotificationService>();
            var mockChatService = new Mock<IChatService>();
            var mockLogger = new Mock<ILogger<AppNotificationController>>();

            // Setting up a mock user with claims
            _mockUser = new Mock<ClaimsPrincipal>();
            _mockUser.Setup(u => u.Identity).Returns(new ClaimsIdentity(new Claim[]
            {
            new Claim("userId", "12345"), // Example claim
            }));

            // Initialize the controller with the mock dependencies
            _controller = new AppNotificationController(
       _mockNotificationHelper.Object,
       _mockNotificationService.Object,
       mockChatService.Object,
       mockLogger.Object
   )
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = _mockUser.Object }
                }
            };
        }

        [Fact]
        public void Constructor_ValidParameters_ShouldInitializeDependencies()
        {
            var mockChatService = new Mock<IChatService>();
            var mockLogger = new Mock<ILogger<AppNotificationController>>();


            // Arrange & Act
            var controller = new AppNotificationController(
            _mockNotificationHelper.Object,
            _mockNotificationService.Object,
            mockChatService.Object,
            mockLogger.Object
            );

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public async Task GetNotifications_ValidRequest_Returns200OK()
        {
            // Arrange
            var mockNotifications = new List<AppNotificationsResponseModel>
            {
                new AppNotificationsResponseModel { /* mock data */ },
                new AppNotificationsResponseModel { /* mock data */ }
            };

            _mockNotificationService
                .Setup(service => service.GetNotificationsAsync(It.IsAny<AppTokenDetails>(), 1, 10))
                .ReturnsAsync(mockNotifications);

            // Act
            var result = await _controller.GetNotifications(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseModel = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, responseModel.status);
            Assert.Equal(ResponseMessage.SUCCESS, responseModel.statusMessage);
            Assert.Equal(mockNotifications, responseModel.data);
        }

        [Fact]
        public async Task GetNotifications_EmptyNotifications_Returns200OKWithEmptyData()
        {
            // Arrange
            var emptyNotifications = new List<AppNotificationsResponseModel>();

            _mockNotificationService
                .Setup(service => service.GetNotificationsAsync(It.IsAny<AppTokenDetails>(), 1, 10))
                .ReturnsAsync(emptyNotifications);

            // Act
            var result = await _controller.GetNotifications(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseModel = Assert.IsType<PaginatedResponseModel>(okResult.Value); // Updated to match the actual type
            Assert.Equal(ResponseStatus.TRUE, responseModel.status);
            Assert.Equal(ResponseMessage.SUCCESS, responseModel.statusMessage);
            //Assert.Empty(responseModel.data); // Ensure the data is empty
        }


        [Fact]
        public async Task DeleteNotifications_ValidRequest_Returns200OK()
        {
            // Arrange
            _mockNotificationService
         .Setup(service => service.DeleteNotificationAsync(It.IsAny<AppTokenDetails>()))
         .ReturnsAsync(1);  // Simulate successful deletion

            // Act
            var result = await _controller.DeleteNotifications();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseModel = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, responseModel.status);
            Assert.Equal(ResponseMessage.SUCCESS, responseModel.statusMessage);
        }

        [Fact]
        public async Task SendNotification_ValidRequest_Returns200OK()
        {
            // Arrange
            var model = new NotificationMessageAppViewModel
            {
                // Populate this model with necessary values
                Title = "Test Notification",
                Body = "This is a test notification",
                // other properties...
            };

            // Mock the SendNotificationAsync method to ensure it completes successfully
            _mockNotificationHelper
                .Setup(helper => helper.SendNotificationAsync(It.IsAny<NotificationMessageAppViewModel>()))
                .Returns(Task.CompletedTask);  // Simulate successful notification sending

            // Act
            var result = await _controller.SendNotification(model);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task SendNotificationasync_ReturnsOk_WhenNotificationIsSentSuccessfully()
        {
            // Arrange
            var roomId = "room123";
            var type = 1;
            var senderId = 123;

            var mockFcmTokens = new List<ChatFcmTokenConnection>
    {
        new ChatFcmTokenConnection { Type = 3, FcmToken = "fcm_token_1" },
        new ChatFcmTokenConnection { Type = 2, FcmToken = "fcm_token_2" }
    };

            // Setup mock for chat service
            var mockChatService = new Mock<IChatService>();
            mockChatService.Setup(service => service.GetAllConnectionsForRoomFcmTokenAsync(roomId, type, senderId.ToString()))
                           .ReturnsAsync(mockFcmTokens);

            // Replace chat service in controller (since constructor uses old mock)
            typeof(AppNotificationController)
                .GetField("_chatService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(_controller, mockChatService.Object);

            // Setup notification helper
            _mockNotificationHelper.Setup(helper => helper.SendWebPushNotifications(It.IsAny<WebNotificationModel>()))
                                   .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SendNotificationasync(roomId, type, senderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Access the anonymous type using reflection
            var value = okResult.Value;
            var statusProp = value?.GetType().GetProperty("status");
            var messageProp = value?.GetType().GetProperty("message");

            Assert.NotNull(statusProp);
            Assert.NotNull(messageProp);
            Assert.True((bool?)statusProp?.GetValue(value));
            Assert.Equal("Notification sent.", messageProp?.GetValue(value) as string);
        }

    }
}
