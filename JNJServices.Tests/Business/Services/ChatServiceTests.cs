using Dapper;
using JNJServices.Business.Services;
using JNJServices.Data;
using JNJServices.Models.CommonModels;
using JNJServices.Utility.DbConstants;
using Moq;
using System.Data;

namespace JNJServices.Tests.Business.Services
{
    public class ChatServiceTests
    {
        private readonly Mock<IDapperContext> _dapperContextMock;
        private readonly ChatService _chatService;

        public ChatServiceTests()
        {
            _dapperContextMock = new Mock<IDapperContext>();
            _chatService = new ChatService(_dapperContextMock.Object);
        }

        [Fact]
        public async Task GetActiveChatRoomsAsync_ShouldReturnActiveChatRooms_WhenDataExists()
        {
            // Arrange
            var expectedChatRooms = new List<ChatRoomsActiveResponse>
    {
        new ChatRoomsActiveResponse { RoomId = "1" }
    };

            _dapperContextMock
                .Setup(c => c.ExecuteQueryAsync<ChatRoomsActiveResponse>(
                    ProcEntities.GetActiveChatRooms,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedChatRooms);

            // Act
            var result = await _chatService.GetActiveChatRoomsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedChatRooms.Count, result.Count);
            _dapperContextMock.Verify(c => c.ExecuteQueryAsync<ChatRoomsActiveResponse>(
                ProcEntities.GetActiveChatRooms,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task GetConnectionKeysForRoom_ShouldReturnKeys_WhenDataExists()
        {
            // Arrange
            var roomName = "Room1";
            var expectedKeys = new List<string> { "Key1", "Key2" };

            _dapperContextMock
                .Setup(c => c.ExecuteQueryAsync<string>(
                    ProcEntities.spGetAllConnectionsForRoom,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedKeys);

            // Act
            var result = await _chatService.GetConnectionKeysForRoom(roomName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedKeys.Count, result.Count);
            _dapperContextMock.Verify(c => c.ExecuteQueryAsync<string>(
                ProcEntities.spGetAllConnectionsForRoom,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task GetAllConnectionsForRoomFcmTokenAsync_ShouldReturnTokens_WhenDataExists()
        {
            // Arrange
            var roomId = "Room1";
            var type = 1;
            var senderId = "123";
            var expectedConnections = new List<ChatFcmTokenConnection>
    {
        new ChatFcmTokenConnection { FcmToken = "Token1", ConnectionID = "Conn1", Type = 1 }
    };

            _dapperContextMock
                .Setup(c => c.ExecuteQueryAsync<ChatFcmTokenConnection>(
                    ProcEntities.spGetAllConnectionsForRoomFcmToken,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .ReturnsAsync(expectedConnections);

            // Act
            var result = await _chatService.GetAllConnectionsForRoomFcmTokenAsync(roomId, type, senderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedConnections.Count, result.Count);
            _dapperContextMock.Verify(c => c.ExecuteQueryAsync<ChatFcmTokenConnection>(
                ProcEntities.spGetAllConnectionsForRoomFcmToken,
                It.IsAny<DynamicParameters>(),
                CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task InsertOrUpdateChatRoom_ReturnsExpectedTuple()
        {
            // Arrange
            var model = new ChatRoomViewModel
            {
                RoomId = 101,
                CreateAt = new DateTime(2024, 1, 1)
            };

            var capturedParams = new DynamicParameters();

            _dapperContextMock
                .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure))
                .Callback<string, DynamicParameters, CommandType>((_, p, __) =>
                {
                    capturedParams = p;

                    // Set simulated output values
                    p.Add(DbParams.ChatRoomId, 2001, direction: ParameterDirection.Output);
                    p.Add(DbParams.ResponseCode, 1, direction: ParameterDirection.Output);
                    p.Add(DbParams.Msg, "Room created successfully", direction: ParameterDirection.Output);
                    p.Add(DbParams.RoomStatus, "Active", direction: ParameterDirection.Output);
                })
                .ReturnsAsync(1);

            // Manually simulate output values since Moq won't populate them automatically
            var parametersToInject = new DynamicParameters();
            parametersToInject.Add(DbParams.ChatRoomId, 2001);
            parametersToInject.Add(DbParams.ResponseCode, 1);
            parametersToInject.Add(DbParams.Msg, "Room created successfully");
            parametersToInject.Add(DbParams.RoomStatus, "Active");

            // Act
            var result = await _chatService.InsertOrUpdateChatRoom(model);

            // Assert
            Assert.Equal(2001, result.Item1); // chatRoomId
            Assert.Equal(1, result.Item2);    // responseCode
            Assert.Equal("Room created successfully", result.Item3); // message
            Assert.Equal("Active", result.Item4); // roomStatus

            _dapperContextMock.Verify(x =>
                x.ExecuteAsync(ProcEntities.spInsertOrUpdateChatRoom,
                               It.IsAny<DynamicParameters>(),
                               CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task UpsertChatRoomMessageAsync_ReturnsExpectedResult()
        {
            // Arrange
            var model = new ChatMessageViewModel
            {
                RoomId = "1001",
                MessageContent = "Test message",
                SendAt = DateTime.UtcNow,
                Type = 1,
                SenderId = 10
            };

            DynamicParameters capturedParams = new();

            _dapperContextMock
                .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure))
                .Callback<string, DynamicParameters, CommandType>((_, p, __) =>
                {
                    capturedParams = p;

                    // Simulate output values
                    p.Add(DbParams.ChatRoomId, 5001, direction: ParameterDirection.Output);
                    p.Add(DbParams.ResponseCode, 1, direction: ParameterDirection.Output);
                    p.Add(DbParams.Msg, "Message saved", direction: ParameterDirection.Output);
                    p.Add(DbParams.WebReadStatus, true, direction: ParameterDirection.Output);
                })
                .ReturnsAsync(1);

            // Simulate parameter return values
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ChatRoomId, 5001);
            parameters.Add(DbParams.ResponseCode, 1);
            parameters.Add(DbParams.Msg, "Message saved");
            parameters.Add(DbParams.WebReadStatus, true);

            // Act
            var result = await _chatService.UpsertChatRoomMessageAsync(model);

            // Assert
            Assert.Equal(5001, result.ChatRoomId);
            Assert.Equal(1, result.ResponseCode);
            Assert.Equal("Message saved", result.Message);
            Assert.True(result.WebReadStatus);

            _dapperContextMock.Verify(x =>
                x.ExecuteAsync(ProcEntities.spUpsertChatRoomMessage,
                               It.IsAny<DynamicParameters>(),
                               CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task GetChatListAsync_ReturnsChatListAndTotalCount()
        {
            // Arrange
            var model = new ChatListViewModel
            {
                SearchQuery = "test",
                RoomId = "101",
                Page = 1,
                Limit = 10,
                ContractorId = 501
            };

            var expectedChatRooms = new List<ChatRoom>
            {
                new ChatRoom { roomId = 101, lastMessage = "Hello", contractorID = 501 },
                new ChatRoom { roomId = 102, lastMessage = "Hi again", contractorID = 501 }
            };

            DynamicParameters capturedParams = new();

            _dapperContextMock
                .Setup(x => x.ExecuteQueryAsync<ChatRoom>(
                    ProcEntities.spGetChatList,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .Callback<string, DynamicParameters, CommandType>((_, p, __) =>
                {
                    capturedParams = p;
                    p.Add(DbParams.TotalCount, 2, direction: ParameterDirection.Output);
                })
                .ReturnsAsync(expectedChatRooms);

            // Simulate returned output manually (since Moq won't update the DynamicParameters after method call)
            capturedParams.Add(DbParams.TotalCount, 2);

            // Act
            var (chatList, totalCount) = await _chatService.GetChatListAsync(model);

            // Assert
            Assert.Equal(2, chatList.Count);
            Assert.Equal(2, totalCount);
            Assert.Equal("Hello", chatList[0].lastMessage);

            _dapperContextMock.Verify(x =>
                x.ExecuteQueryAsync<ChatRoom>(
                    ProcEntities.spGetChatList,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure), Times.Once);
        }

        [Fact]
        public async Task GetChatMessagesAsync_ReturnsMessagesAndTotalCount()
        {
            // Arrange
            var model = new ChatMessageSearchViewModel
            {
                ChatRoomId = 101,
                Page = 1,
                Limit = 10
            };

            var expectedMessages = new List<ChatMessage>
    {
        new ChatMessage { MessageId = 1, ChatRoomId = 101, MessageContent = "Hi!" },
        new ChatMessage { MessageId = 2, ChatRoomId = 101, MessageContent = "Hello again!" }
    };

            DynamicParameters capturedParams = new();

            _dapperContextMock
                .Setup(x => x.ExecuteQueryAsync<ChatMessage>(
                    ProcEntities.spGetChatMessages,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure))
                .Callback<string, DynamicParameters, CommandType>((_, p, __) =>
                {
                    capturedParams = p;
                    p.Add(DbParams.TotalCount, 2, direction: ParameterDirection.Output);
                })
                .ReturnsAsync(expectedMessages);

            // Manually simulate output for TotalCount
            capturedParams.Add(DbParams.TotalCount, 2);

            // Act
            var (messages, totalCount) = await _chatService.GetChatMessagesAsync(model);

            // Assert
            Assert.Equal(2, messages.Count);
            Assert.Equal(2, totalCount);
            Assert.Equal("Hi!", messages[0].MessageContent);

            _dapperContextMock.Verify(x =>
                x.ExecuteQueryAsync<ChatMessage>(
                    ProcEntities.spGetChatMessages,
                    It.IsAny<DynamicParameters>(),
                    CommandType.StoredProcedure), Times.Once);
        }

    }
}
