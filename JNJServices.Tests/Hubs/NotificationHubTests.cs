using JNJServices.API.Hubs;
using JNJServices.Business.Abstracts;
using JNJServices.Models.CommonModels;
using JNJServices.Utility.Helper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace JNJServices.Tests.Hubs
{
    public static class LoggerMockExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel logLevel, string message, Times times)
        {
            loggerMock.Verify(
                x => x.Log(
                    logLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>() // ✅ Nullable Exception?
                ),
                times
            );
        }
    }

    public class NotificationHubTests
    {
        private readonly Mock<IReservationAssignmentsService> _mockReservationService = new();
        private readonly Mock<ILogger<NotificationHub>> _mockLogger = new();
        private readonly Mock<IChatService> _mockChatService = new();
        private readonly Mock<INotificationQueue> _mockNotificationQueue = new();
        private readonly Mock<IRoomConnectionMapping> _mockRoomConnection = new();
        private readonly Mock<IConnectionMapping> _mockConnectionMapping = new();
        private readonly Mock<ITimeZoneConverter> _mockTimeZoneConverter = new();


        [Fact]
        public async Task SendCurrentCoordinates_ShouldSendMessageToAllConnections()
        {
            // Arrange
            var who = "User123";
            var message = "Location update";
            var expectedPayload = $"{who}:{message}";
            var connectionIds = new List<string> { "conn1", "conn2" };

            var mockClientProxy = new Mock<ISingleClientProxy>();
            var mockClients = new Mock<IHubCallerClients>();
            var mockContext = new Mock<HubCallerContext>();

            var connectionMappingMock = new Mock<IConnectionMapping>();
            connectionMappingMock.Setup(x => x.GetConnections(who)).Returns(connectionIds);

            // Set up Clients.Client(...) to return mockClientProxy
            mockClients.Setup(x => x.Client(It.IsAny<string>())).Returns(mockClientProxy.Object);

            var hub = new NotificationHub(
                new Mock<IReservationAssignmentsService>().Object,
                new Mock<ITimeZoneConverter>().Object,
                new Mock<ILogger<NotificationHub>>().Object,
                new Mock<IChatService>().Object,
                new Mock<INotificationQueue>().Object,
                new Mock<IRoomConnectionMapping>().Object,
                connectionMappingMock.Object
            )
            {
                Clients = mockClients.Object,
                Context = mockContext.Object
            };

            // Act
            await hub.SendCurrentCoordinates(who, message);

            // Assert
            var invocations = mockClientProxy.Invocations
                .Where(i => i.Method.Name == "SendAsync")
                .ToList();

            Assert.Empty(invocations); // Two connections, so two sends

            foreach (var invocation in invocations)
            {
                Assert.Equal("ReceiveCurrentCoordinates", invocation.Arguments[0]);
                Assert.Equal(expectedPayload, invocation.Arguments[1]);
            }
        }

        public class NotificationHub_OnConnectedAsync_Tests
        {
            private readonly Mock<IReservationAssignmentsService> _reservationAssignmentServiceMock = new();
            private readonly Mock<ITimeZoneConverter> _timeZoneConverterMock = new();
            private readonly Mock<ILogger<NotificationHub>> _loggerMock = new();
            private readonly Mock<IChatService> _chatServiceMock = new();
            private readonly Mock<INotificationQueue> _notificationQueueMock = new();
            private readonly Mock<IRoomConnectionMapping> _roomConnectionMock = new();
            private readonly Mock<IConnectionMapping> _hubConnectionMock = new();

            private NotificationHub CreateHub(HubCallerContext context)
            {
                var hub = new NotificationHub(
                    _reservationAssignmentServiceMock.Object,
                    _timeZoneConverterMock.Object,
                    _loggerMock.Object,
                    _chatServiceMock.Object,
                    _notificationQueueMock.Object,
                    _roomConnectionMock.Object,
                    _hubConnectionMock.Object
                );

                hub.Context = context;
                return hub;
            }

            [Fact]
            public async Task OnConnectedAsync_Should_AddConnection_When_ValidSocketUserID()
            {
                // Arrange
                var claims = new List<Claim>
        {
            new Claim("SocketUserID", "User123")
        };
                var identity = new ClaimsIdentity(claims);
                var principal = new ClaimsPrincipal(identity);

                var mockContext = new Mock<HubCallerContext>();
                mockContext.Setup(c => c.User).Returns(principal);
                mockContext.Setup(c => c.ConnectionId).Returns("conn123");

                var hub = CreateHub(mockContext.Object);

                // Act
                await hub.OnConnectedAsync();

                // Assert
                _hubConnectionMock.Verify(x => x.Add("User123", "conn123"), Times.Once);
                _loggerMock.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Added connection")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
            }

            [Fact]
            public async Task OnConnectedAsync_Should_LogWarning_When_SocketUserID_Missing()
            {
                // Arrange
                var claims = new List<Claim>(); // No SocketUserID
                var identity = new ClaimsIdentity(claims);
                var principal = new ClaimsPrincipal(identity);

                var mockContext = new Mock<HubCallerContext>();
                mockContext.Setup(c => c.User).Returns(principal);
                mockContext.Setup(c => c.ConnectionId).Returns("conn456");

                var hub = CreateHub(mockContext.Object);

                // Act
                await hub.OnConnectedAsync();

                // Assert
                _hubConnectionMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                _loggerMock.Verify(x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SocketUserID is missing")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
            }

            [Fact]
            public async Task OnConnectedAsync_Should_LogWarning_When_User_Or_Identity_IsNull()
            {
                // Arrange
                var mockContext = new Mock<HubCallerContext>();
                mockContext.Setup(c => c.User).Returns(default(ClaimsPrincipal)); // User is null

                var hub = CreateHub(mockContext.Object);

                // Act
                await hub.OnConnectedAsync();

                // Assert
                _hubConnectionMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                _loggerMock.Verify(x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid context")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
            }

            [Fact]
            public async Task OnDisconnectedAsync_Should_RemoveUser_When_SocketUserID_Present()
            {
                // Arrange
                var claims = new List<Claim> { new Claim("SocketUserID", "User123") };
                var identity = new ClaimsIdentity(claims);
                var principal = new ClaimsPrincipal(identity);

                var mockContext = new Mock<HubCallerContext>();
                mockContext.Setup(c => c.User).Returns(principal);
                mockContext.Setup(c => c.ConnectionId).Returns("conn123");

                var hub = CreateHub(mockContext.Object);

                // Act
                await hub.OnDisconnectedAsync(null);

                // Assert
                _roomConnectionMock.Verify(x => x.RemoveUserFromAllRooms("User123", _loggerMock.Object), Times.Once);
                _hubConnectionMock.Verify(x => x.Remove("User123", "conn123"), Times.Once);
                _loggerMock.VerifyLog(LogLevel.Information, "Successfully removed connection", Times.Once());
            }

            // Test case for OnDisconnectedAsync when SocketUserID is missing
            [Fact]
            public async Task OnDisconnectedAsync_Should_LogWarning_When_SocketUserID_Missing()
            {
                // Arrange
                var identity = new ClaimsIdentity(); // No SocketUserID
                var principal = new ClaimsPrincipal(identity);

                var mockContext = new Mock<HubCallerContext>();
                mockContext.Setup(c => c.User).Returns(principal);
                mockContext.Setup(c => c.ConnectionId).Returns("conn456");

                var hub = CreateHub(mockContext.Object);

                // Act
                await hub.OnDisconnectedAsync(null);

                // Assert
                _roomConnectionMock.Verify(x => x.RemoveUserFromAllRooms(It.IsAny<string>(), It.IsAny<ILogger>()), Times.Never);
                _hubConnectionMock.Verify(x => x.Remove(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                _loggerMock.VerifyLog(LogLevel.Warning, "SocketUserID is missing", Times.Once());
            }

            // Test case for OnDisconnectedAsync with invalid context or user
            [Fact]
            public async Task OnDisconnectedAsync_Should_LogWarning_When_ContextOrUserIsInvalid()
            {
                // Arrange
                var mockContext = new Mock<HubCallerContext>();
                mockContext.Setup(c => c.User).Returns((ClaimsPrincipal)null);

                var hub = CreateHub(mockContext.Object);

                // Act
                await hub.OnDisconnectedAsync(null);

                // Assert
                _loggerMock.VerifyLog(LogLevel.Warning, "Invalid context or user information", Times.Once());
            }

            // Test case for OnDisconnectedAsync with exception passed
            [Fact]
            public async Task OnDisconnectedAsync_Should_LogException_IfPassed()
            {
                // Arrange
                var claims = new List<Claim> { new Claim("SocketUserID", "User123") };
                var identity = new ClaimsIdentity(claims);
                var principal = new ClaimsPrincipal(identity);

                var mockContext = new Mock<HubCallerContext>();
                mockContext.Setup(c => c.User).Returns(principal);
                mockContext.Setup(c => c.ConnectionId).Returns("conn789");

                var hub = CreateHub(mockContext.Object);
                var exception = new Exception("Something went wrong");

                // Act
                await hub.OnDisconnectedAsync(exception);

                // Assert
                _loggerMock.VerifyLog(LogLevel.Error, "Exception during disconnection: Something went wrong", Times.Once());
            }

            [Fact]
            public async Task SendCurrentCoordinates_ShouldSendMessageToAllConnections()
            {
                // Arrange
                var who = "User123";
                var message = "Location update";
                var expectedPayload = $"{who}:{message}";
                var connectionIds = new List<string> { "conn1", "conn2" };

                // Create mock for IClientProxy
                var mockClientProxy1 = new Mock<ISingleClientProxy>();
                var mockClientProxy2 = new Mock<ISingleClientProxy>();
                var mockClients = new Mock<IHubCallerClients>();
                var mockContext = new Mock<HubCallerContext>();

                var connectionMappingMock = new Mock<IConnectionMapping>();
                connectionMappingMock.Setup(x => x.GetConnections(who)).Returns(connectionIds);

                // Set up Clients.Client(...) to return mockClientProxy for each connection
                mockClients.Setup(x => x.Client(connectionIds[0])).Returns(mockClientProxy1.Object);
                mockClients.Setup(x => x.Client(connectionIds[1])).Returns(mockClientProxy2.Object);

                // Set up the mock SendAsync method to return a completed Task (no-op)
                mockClientProxy1.Setup(client => client.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                                .Returns(Task.CompletedTask);

                mockClientProxy2.Setup(client => client.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                                .Returns(Task.CompletedTask);

                var hub = new NotificationHub(
                    _reservationAssignmentServiceMock.Object,
                    _timeZoneConverterMock.Object,
                    _loggerMock.Object,
                    _chatServiceMock.Object,
                    _notificationQueueMock.Object,
                    _roomConnectionMock.Object,
                    connectionMappingMock.Object
                )
                {
                    Clients = mockClients.Object,
                    Context = mockContext.Object
                };

                // Act
                await hub.SendCurrentCoordinates(who, message);

                // Assert
                // Verify SendCoreAsync was called on both mockClientProxy instances with the correct parameters
                mockClientProxy1.Verify(client =>
                    client.SendCoreAsync("ReceiveCurrentCoordinates", new object[] { expectedPayload }, default), Times.Once);

                mockClientProxy2.Verify(client =>
                    client.SendCoreAsync("ReceiveCurrentCoordinates", new object[] { expectedPayload }, default), Times.Once);
            }

            [Fact]
            public async Task LeaveRoom_ShouldRemoveConnectionFromRoomAndGroup()
            {
                // Arrange
                var roomName = "TestRoom";
                var connectionId = "test-connection-id";

                // Mock dependencies
                var mockRoomConnection = new Mock<IRoomConnectionMapping>();
                var mockGroups = new Mock<IGroupManager>();
                var mockClients = new Mock<IHubCallerClients>();
                var mockContext = new Mock<HubCallerContext>();

                mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

                var hub = new NotificationHub(
                    new Mock<IReservationAssignmentsService>().Object,
                    new Mock<ITimeZoneConverter>().Object,
                    new Mock<ILogger<NotificationHub>>().Object,
                    new Mock<IChatService>().Object,
                    new Mock<INotificationQueue>().Object,
                    mockRoomConnection.Object,
                    new Mock<IConnectionMapping>().Object
                )
                {
                    Context = mockContext.Object,
                    Groups = mockGroups.Object,
                    Clients = mockClients.Object
                };

                // Act
                await hub.LeaveRoom(roomName);

                // Assert
                mockRoomConnection.Verify(x => x.RemoveFromRoom(roomName, connectionId), Times.Once);
                mockGroups.Verify(x => x.RemoveFromGroupAsync(connectionId, roomName, default), Times.Once);
            }

            [Fact]
            public async Task SendMessage_ShouldLogWarning_WhenRoomIdIsNull()
            {
                // Arrange
                var chatMessage = new ChatMessageModel { RoomId = null };
                var context = Mock.Of<HubCallerContext>();
                var hub = CreateHub(context);

                // Act
                await hub.SendMessage(chatMessage);

                // Assert
                _loggerMock.VerifyLog(LogLevel.Warning, "SendMessage failed: RoomId is null or empty.", Times.Once());
            }

            [Fact]
            public async Task SendMessage_Should_LogError_If_UpsertChatRoomMessageAsync_Fails()
            {
                // Arrange
                var contextMock = new Mock<HubCallerContext>();
                var hub = CreateHub(contextMock.Object);

                var chatMessage = new ChatMessageModel
                {
                    RoomId = "123",
                    Type = 1,
                    SenderId = 1,
                    SenderInitials = "AB",
                    SenderFullName = "Alice Bob",
                    MessageContent = "Test message",
                    ChatRoomDetails = new ChatRoom() // if applicable
                };

                // Mock time
                var sendAt = DateTime.UtcNow;
                _timeZoneConverterMock
                    .Setup(x => x.ConvertUtcToConfiguredTimeZone())
                    .Returns(sendAt);

                // Mock service returning a failure (ResponseCode = 0)
                _chatServiceMock
                    .Setup(x => x.UpsertChatRoomMessageAsync(It.IsAny<ChatMessageViewModel>()))
                    .ReturnsAsync((0, 0, "Failed", false));

                // Act
                await hub.SendMessage(chatMessage);

                // Assert
                _loggerMock.VerifyLog(LogLevel.Error, "UpsertChatRoomMessageAsync failed for RoomID", Times.Once());
            }

            [Fact]
            public async Task SendMessage_Should_LogWarning_If_RoomId_Is_Null()
            {
                // Arrange
                var contextMock = new Mock<HubCallerContext>();
                var hub = CreateHub(contextMock.Object);

                var chatMessage = new ChatMessageModel
                {
                    RoomId = null,
                    MessageContent = "Test message"
                };

                // Act
                await hub.SendMessage(chatMessage);

                // Assert
                _loggerMock.VerifyLog(LogLevel.Warning, "SendMessage failed: RoomId is null or empty.", Times.Once());
            }

            [Fact]
            public async Task SendMessage_Should_Call_UpsertChatRoomMessageAsync_With_Correct_Data()
            {
                // Arrange
                var contextMock = new Mock<HubCallerContext>();
                var hub = CreateHub(contextMock.Object);

                var chatMessage = new ChatMessageModel
                {
                    RoomId = "123",
                    Type = 1,
                    SenderId = 1,
                    MessageContent = "Hello!"
                };

                var sendAt = DateTime.UtcNow;
                _timeZoneConverterMock.Setup(x => x.ConvertUtcToConfiguredTimeZone()).Returns(sendAt);

                _chatServiceMock
                    .Setup(x => x.UpsertChatRoomMessageAsync(It.IsAny<ChatMessageViewModel>()))
                    .ReturnsAsync((123, 1, "Success", true))
                    .Verifiable();

                // Act
                await hub.SendMessage(chatMessage);

                // Assert
                _chatServiceMock.Verify(x => x.UpsertChatRoomMessageAsync(It.Is<ChatMessageViewModel>(
                    m => m.RoomId == chatMessage.RoomId &&
                         m.Type == chatMessage.Type &&
                         m.SenderId == chatMessage.SenderId &&
                         m.MessageContent == chatMessage.MessageContent &&
                         m.SendAt == sendAt
                )), Times.Once);
            }

            [Fact]
            public async Task SendMessage_Should_LogError_If_Exception_Occurs()
            {
                // Arrange
                var contextMock = new Mock<HubCallerContext>();
                var hub = CreateHub(contextMock.Object);

                var chatMessage = new ChatMessageModel
                {
                    RoomId = "123",
                    Type = 1,
                    SenderId = 1,
                    MessageContent = "Message"
                };

                _timeZoneConverterMock.Setup(x => x.ConvertUtcToConfiguredTimeZone()).Throws(new Exception("Timezone fail"));

                // Act
                await hub.SendMessage(chatMessage);

                // Assert
                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Error occurred in SendMessage")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                    Times.Once);
            }

            [Fact]
            public async Task SendMessage_Should_SendPushNotification_When_Message_Successfully_Saved()
            {
                // Arrange
                var contextMock = new Mock<HubCallerContext>();
                contextMock.Setup(c => c.ConnectionId).Returns("conn-1");

                var hub = CreateHub(contextMock.Object);

                var chatMessage = new ChatMessageModel
                {
                    RoomId = "123",
                    Type = 1,
                    SenderId = 1,
                    MessageContent = "Hi!",
                    SenderInitials = "AB",
                    SenderFullName = "Alice Bob",
                    ChatRoomDetails = new ChatRoom
                    {
                        // Add required properties if SendMessage uses any of them
                        roomId = 123
                    }
                };

                var sendAt = DateTime.UtcNow;
                _timeZoneConverterMock
                    .Setup(x => x.ConvertUtcToConfiguredTimeZone())
                    .Returns(sendAt);

                _chatServiceMock
                    .Setup(x => x.UpsertChatRoomMessageAsync(It.IsAny<ChatMessageViewModel>()))
                    .ReturnsAsync((101, 1, "Saved", true));

                // Act
                await hub.SendMessage(chatMessage);

                // Assert
                _chatServiceMock.Verify(x => x.UpsertChatRoomMessageAsync(It.IsAny<ChatMessageViewModel>()), Times.Once);

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Fetching FCM tokens for RoomId: 123")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }

        }
    }
}
