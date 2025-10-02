using JNJServices.API.Controllers.v1.Web;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.CommonModels;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Helper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1.Web
{
    public class WebChatControllerTests
    {
        private readonly Mock<IChatService> _chatServiceMock;
        private readonly Mock<ITimeZoneConverter> _timeZoneConverterMock;
        private readonly WebChatController _controller;

        public WebChatControllerTests()
        {
            _chatServiceMock = new Mock<IChatService>();
            _timeZoneConverterMock = new Mock<ITimeZoneConverter>();
            _controller = new WebChatController(_chatServiceMock.Object, _timeZoneConverterMock.Object);
        }

        [Fact]
        public async Task GetChatList_ValidModel_ReturnsOkResultWithData()
        {
            // Arrange
            var model = new ChatListViewModel
            {
                Page = 1,
                Limit = 10,
                ContractorId = 123
            };

            var mockChatRooms = new List<ChatRoom>
        {
            new ChatRoom
            {
                chatRoomId = 1,
                roomId = 1001,
                lastMessage = "Hello",
                lastMessageAt = DateTime.UtcNow,
                createdAt = DateTime.UtcNow.AddHours(-2),
                isInactive = false,
                contractorID = 123
            }
        };

            _chatServiceMock.Setup(service => service.GetChatListAsync(model))
                            .ReturnsAsync((mockChatRooms, mockChatRooms.Count));

            // Act
            var result = await _controller.GetChatList(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);

            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(mockChatRooms.Count, response.totalData);
            Assert.Equal(mockChatRooms, response.data);
        }

        [Theory]
        [InlineData(0, 10)] // Invalid Page
        [InlineData(1, 0)]  // Invalid Limit
        public async Task GetChatList_InvalidModel_ReturnsBadRequest(int page, int limit)
        {
            // Arrange
            var model = new ChatListViewModel
            {
                Page = page,
                Limit = limit
            };

            // Act
            var result = await _controller.GetChatList(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(badRequestResult.Value);

            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.INVALID_INPUT_PARAMS, response.statusMessage);
        }

        [Fact]
        public async Task GetChatMessages_ValidInput_ReturnsOkWithExpectedChatMessages()
        {
            // Arrange
            var model = new ChatMessageSearchViewModel
            {
                ChatRoomId = 1,
                Page = 1,
                Limit = 10
            };

            var messages = new List<ChatMessage>
        {
            new ChatMessage
            {
                MessageId = 1,
                ChatRoomId = 1,
                MessageContent = "Test message 1",
                SentAt = DateTime.UtcNow,
                Type = 1,
                SenderId = 101,
                SenderInitials = "JD",
                SenderFullName = "John Doe"
            },
            new ChatMessage
            {
                MessageId = 2,
                ChatRoomId = 1,
                MessageContent = "Test message 2",
                SentAt = DateTime.UtcNow.AddMinutes(-5),
                Type = 2,
                SenderId = 102,
                SenderInitials = "AB",
                SenderFullName = "Alice Brown"
            }
        };

            _chatServiceMock
                .Setup(s => s.GetChatMessagesAsync(model))
                .ReturnsAsync((messages, messages.Count));

            // Act
            var result = await _controller.GetChatMessages(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);

            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(messages.Count, response.totalData);

            var returnedMessages = Assert.IsType<List<ChatMessage>>(response.data);
            Assert.Equal(messages[0].MessageId, returnedMessages[0].MessageId);
            Assert.Equal(messages[0].MessageContent, returnedMessages[0].MessageContent);
            Assert.Equal(messages[0].SenderFullName, returnedMessages[0].SenderFullName);
        }

        [Theory]
        [InlineData(0, 1, 10)]  // Invalid ChatRoomId
        [InlineData(1, 0, 10)]  // Invalid Page
        [InlineData(1, 1, 0)]   // Invalid Limit
        public async Task GetChatMessages_InvalidModel_ReturnsBadRequest(int chatRoomId, int page, int limit)
        {
            // Arrange
            var model = new ChatMessageSearchViewModel
            {
                ChatRoomId = chatRoomId,
                Page = page,
                Limit = limit
            };

            // Act
            var result = await _controller.GetChatMessages(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(badRequestResult.Value);

            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.INVALID_INPUT_PARAMS, response.statusMessage);
        }

        [Fact]
        public async Task InsertOrUpdateChatRoom_ValidModel_ReturnsOk()
        {
            // Arrange
            var model = new ChatRoomViewModel
            {
                RoomId = 100
            };

            int expectedChatRoomId = 123;
            int responseCode = 1;
            string message = "Chat Room inserted successfully";

            _chatServiceMock
                .Setup(s => s.InsertOrUpdateChatRoom(It.IsAny<ChatRoomViewModel>()))
                .ReturnsAsync((expectedChatRoomId, responseCode, message, ""));

            // Act
            var result = await _controller.InsertOrUpdateChatRoom(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(message, response.statusMessage);

            // Convert anonymous object to JObject to access properties safely
            var json = JsonConvert.SerializeObject(response.data);
            var data = JsonConvert.DeserializeObject<JObject>(json);

            Assert.NotNull(data);
            Assert.Equal(expectedChatRoomId, (int)data["ChatRoomId"]);
        }


        [Fact]
        public async Task InsertOrUpdateChatRoom_ResponseCodeNotOne_ReturnsBadRequest()
        {
            // Arrange
            var model = new ChatRoomViewModel
            {
                RoomId = 101
            };

            int expectedChatRoomId = 0;
            int responseCode = 0;
            string message = "Insert failed";

            _chatServiceMock
                .Setup(s => s.InsertOrUpdateChatRoom(It.IsAny<ChatRoomViewModel>()))
                .ReturnsAsync((expectedChatRoomId, responseCode, message, ""));

            // Act
            var result = await _controller.InsertOrUpdateChatRoom(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(message, response.statusMessage);
        }

        [Fact]
        public async Task InsertOrUpdateChatRoom_NullModel_ReturnsBadRequest()
        {
            // Arrange
            ChatRoomViewModel? model = null;

            // Act
            var result = await _controller.InsertOrUpdateChatRoom(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.INVALID_INPUT_PARAMS, response.statusMessage);
        }

        [Fact]
        public async Task ActiveChatList_AlwaysReturnsActiveRooms()
        {
            // Arrange
            var model = new ContractorIDViewModel { ContractorID = 5 }; // Will be set to null in controller anyway
            var expectedRooms = new List<ChatRoomsActiveResponse>
            {
                new ChatRoomsActiveResponse { RoomId = "room1" },
                new ChatRoomsActiveResponse { RoomId = "room2" }
            };

            _chatServiceMock
                .Setup(s => s.GetActiveChatRoomsAsync(null))
                .ReturnsAsync(expectedRooms);

            // Act
            var result = await _controller.ActiveChatList(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);

            var rooms = Assert.IsType<List<ChatRoomsActiveResponse>>(response.data);
            Assert.Equal(2, rooms.Count);
            Assert.Equal("room1", rooms[0].RoomId);
            Assert.Equal("room2", rooms[1].RoomId);

            // Ensure the method was called with null as expected
            _chatServiceMock.Verify(s => s.GetActiveChatRoomsAsync(null), Times.Once);
        }

    }
}
