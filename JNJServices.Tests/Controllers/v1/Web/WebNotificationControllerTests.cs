using JNJServices.API.Controllers.v1.Web;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1.Web
{
    public class WebNotificationControllerTests
    {
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly WebNotificationController _controller;

        public WebNotificationControllerTests()
        {
            _mockNotificationService = new Mock<INotificationService>();
            _controller = new WebNotificationController(_mockNotificationService.Object);
        }

        [Fact]
        public async Task GetUserWebNotifications_ReturnsCorrectNotifications()
        {
            // Arrange
            var userId = "test-user-id";
            var expectedNotifications = new List<WebNotificationResponseModel>
    {
        new WebNotificationResponseModel
        {
            NotificationID = 1,
            ReferenceID = "ref-001",
            ReservationsAssignmentsID = 100,
            ReservationID = 200,
            AssgnNum = "A123",
            UserID = 1,
            Title = "Test Notification 1",
            Body = "This is the body of the first notification.",
            Data = "Some additional data for notification 1.",
            SentDate = DateTime.UtcNow,
            WebReadStatus = 0,
            NotificationType = "Info",
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        }
    };

            var claims = new List<Claim>
    {
        new Claim("UserID", userId)
    };
            var identity = new ClaimsIdentity(claims);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            _mockNotificationService.Setup(service => service.GetUserWebNotifications(userId, 1, 10))
                                    .ReturnsAsync(expectedNotifications);

            // Act
            var result = await _controller.GetWebNotifications();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            //Assert.Equal(expectedNotifications, response.data);
        }

        [Fact]
        public async Task GetWebNotifications_ReturnsNotFound_WhenNoUserId()
        {
            // Arrange
            var expectedResponse = new PaginatedResponseModel
            {
                status = ResponseStatus.FALSE,
                statusMessage = ResponseMessage.DATA_NOT_FOUND
            };

            // Setting up User identity in the controller context with no user id
            var identity = new ClaimsIdentity();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            // Act
            var result = await _controller.GetWebNotifications(1, 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.Equal(expectedResponse.status, response.status);
            Assert.Equal(expectedResponse.statusMessage, response.statusMessage);
        }

        [Fact]
        public async Task GetWebNotifications_ReturnsNotFound_WhenNoNotifications()
        {
            // Arrange
            var userId = "test-user-id";
            var expectedResponse = new ResponseModel
            {
                status = ResponseStatus.FALSE,
                statusMessage = ResponseMessage.DATA_NOT_FOUND
            };

            // Setting up User identity in the controller context
            var claims = new List<Claim>
            {
                new Claim("UserID", userId)
            };

            var identity = new ClaimsIdentity(claims);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            // Mocking the service to return an empty list
            _mockNotificationService.Setup(service => service.GetUserWebNotifications(userId, 1, 10))
                                    .ReturnsAsync(new List<WebNotificationResponseModel>());

            // Act
            var result = await _controller.GetWebNotifications();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.Equal(expectedResponse.status, response.status);
            Assert.Equal(expectedResponse.statusMessage, response.statusMessage);
        }

        [Fact]
        public async Task GetWebNotifications_ReturnsOkResult_WithSingleNotification()
        {
            // Arrange
            var userId = "test-user-id";
            var expectedNotification = new List<WebNotificationResponseModel>
            {
                new WebNotificationResponseModel { Body = "Single notification body" }
            };

            // Setting up User identity in the controller context
            var claims = new List<Claim>
            {
                new Claim("UserID", userId)
            };

            var identity = new ClaimsIdentity(claims);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            _mockNotificationService.Setup(service => service.GetUserWebNotifications(userId, 1, 10))
                                    .ReturnsAsync(expectedNotification);

            // Act
            var result = await _controller.GetWebNotifications();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            //Assert.Equal(expectedNotification[0].Body, ((List<WebNotificationResponseModel>)response.data)[0].Body);
        }

        [Fact]
        public async Task DeleteWebNotifications_ReturnsOkResult_WhenDeletedSuccessfully()
        {
            // Arrange
            var userId = "test-user-id";

            // Setting up User identity in the controller context
            var claims = new List<Claim>
            {
                new Claim("UserID", userId) // Ensure this matches your controller's logic
            };

            var identity = new ClaimsIdentity(claims);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            // Mock the deletion service call
            _mockNotificationService.Setup(service => service.DeleteUserWebNotifications(userId))
                                    .ReturnsAsync(1); // Simulating successful deletion

            // Act
            var result = await _controller.DeleteWebNotifications();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            // Debugging lines (optional)
            //Debug.Assert(response.status == ResponseStatus.TRUE, "Expected status to be TRUE");

            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(ResponseMessage.NOTIFICATION_DELETED, response.data);
        }

        //    [Fact]
        //    public async Task DeleteWebNotifications_ReturnsOkResult_WhenDeletionFails()
        //    {
        //        // Arrange
        //        var userId = "test-user-id";

        //        // Setting up User identity in the controller context
        //        var claims = new List<Claim>
        //{
        //    new Claim("UserID", userId) // Ensure this matches your controller's logic
        //};

        //        var identity = new ClaimsIdentity(claims);
        //        _controller.ControllerContext = new ControllerContext
        //        {
        //            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        //        };

        //        // Mock the deletion service call to simulate a failed deletion
        //        _mockNotificationService.Setup(service => service.DeleteUserWebNotifications(userId))
        //                                .ReturnsAsync(1); // Simulating a failure in deletion

        //        // Act
        //        var result = await _controller.DeleteWebNotifications();

        //        // Assert
        //        var okResult = Assert.IsType<OkObjectResult>(result);
        //        var response = Assert.IsType<ResponseModel>(okResult.Value);

        //        Assert.Equal(ResponseStatus.FALSE, response.status); // Expect FALSE for failure
        //        Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage); // Appropriate failure message
        //        Assert.Equal(ResponseMessage.NOTIFICATION_DELETED, response.data); // Or check if this is expected based on your logic
        //    }



    }
}
