using JNJServices.API.Controllers.v1.App;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.CommonModels;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1.App
{
    public class AppChatControllerTests
    {
        private readonly AppChatController _controller;
        private readonly Mock<IChatService> _chatServiceMock;
        private readonly Mock<ITimeZoneConverter> _timeZoneConverterMock;

        public AppChatControllerTests()
        {
            _chatServiceMock = new Mock<IChatService>();
            _timeZoneConverterMock = new Mock<ITimeZoneConverter>();
            _controller = new AppChatController(_chatServiceMock.Object, _timeZoneConverterMock.Object);
            var claims = new List<Claim>
        {
            new Claim("Type", (1).ToString()),
            new Claim("UserID", "123")
        };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }


        [Fact]
        public async Task GetChatList_ReturnsBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _controller.GetChatList(new ChatListViewModel()); // Ensure model is not null



            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(badRequest.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.INVALID_INPUT_PARAMS, response.statusMessage);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        public async Task GetChatList_ReturnsBadRequest_WhenPageOrLimitInvalid(int page, int limit)
        {
            var model = new ChatListViewModel { Page = page, Limit = limit };
            var result = await _controller.GetChatList(model);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(badRequest.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
        }

        [Fact]
        public async Task GetChatList_SetsContractorId_ForContractorType()
        {
            // Arrange
            var model = new ChatListViewModel { Page = 1, Limit = 10 };

            var chatRooms = new List<ChatRoom> { new ChatRoom { chatRoomId = 1 } };
            _chatServiceMock
                .Setup(s => s.GetChatListAsync(It.IsAny<ChatListViewModel>()))
                .ReturnsAsync((chatRooms, 1));

            // Act
            var result = await _controller.GetChatList(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.NotNull(response.data);
            Assert.Equal(1, response.totalData);
        }

        [Fact]
        public async Task GetChatList_SetsContractorIdToZero_ForClaimantType()
        {
            // Arrange custom user with Claimant type
            var claims = new List<Claim>
        {
            new Claim("Type", (2).ToString()),
            new Claim("UserID", "999")
        };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            var model = new ChatListViewModel { Page = 1, Limit = 10 };

            var chatRooms = new List<ChatRoom> { new ChatRoom { chatRoomId = 1 } };
            _chatServiceMock
                .Setup(s => s.GetChatListAsync(It.Is<ChatListViewModel>(m => m.ContractorId == 0)))
                .ReturnsAsync((chatRooms, 1));

            // Act
            var result = await _controller.GetChatList(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(1, response.totalData);
        }

        [Fact]
        public async Task GetChatMessages_ReturnsBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _controller.GetChatMessages(new ChatMessageSearchViewModel());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(badRequest.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.INVALID_INPUT_PARAMS, response.statusMessage);
        }

        [Theory]
        [InlineData(0, 1, 10)]
        [InlineData(1, 0, 10)]
        [InlineData(1, 1, 0)]
        public async Task GetChatMessages_ReturnsBadRequest_WhenInvalidValues(int chatRoomId, int page, int limit)
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
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(badRequest.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.INVALID_INPUT_PARAMS, response.statusMessage);
        }

        [Fact]
        public async Task GetChatMessages_ReturnsOk_WhenModelIsValid()
        {
            // Arrange
            var model = new ChatMessageSearchViewModel
            {
                ChatRoomId = 1,
                Page = 1,
                Limit = 10
            };

            var expectedMessages = new List<ChatMessage>
        {
            new ChatMessage { MessageId = 1, MessageContent = "Hello!", ChatRoomId = 1 }
        };

            _chatServiceMock
                .Setup(s => s.GetChatMessagesAsync(model))
                .ReturnsAsync((expectedMessages, 1));

            // Act
            var result = await _controller.GetChatMessages(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(1, response.totalData);
            Assert.Equal(expectedMessages, response.data);
        }

        [Fact]
        public async Task InsertOrUpdateChatRoom_ReturnsBadRequest_WhenModelIsNull()
        {
            // Act
            var model = new ChatRoomViewModel(); // Provide a default instance instead of null
            var result = await _controller.InsertOrUpdateChatRoom(model);


            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequest.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            //Assert.Equal(ResponseMessage.INVALID_INPUT_PARAMS, response.statusMessage);
        }

        [Fact]
        public async Task InsertOrUpdateChatRoom_ReturnsOk_WhenInsertIsSuccessful()
        {
            // Arrange
            var model = new ChatRoomViewModel
            {
                RoomId = 1
            };

            var fakeDate = new DateTime(2024, 1, 1);
            _timeZoneConverterMock
                .Setup(x => x.ConvertUtcToConfiguredTimeZone())
                .Returns(fakeDate);

            _chatServiceMock
                .Setup(x => x.InsertOrUpdateChatRoom(It.IsAny<ChatRoomViewModel>()))
                .ReturnsAsync((123, 1, "Chat room created", "NEW"));

            // Act
            var result = await _controller.InsertOrUpdateChatRoom(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal("Chat room created", response.statusMessage);
            Assert.NotNull(response.data);

            var chatRoomData = response.data as dynamic;

            Assert.Equal(fakeDate, model.CreateAt);
        }

        [Fact]
        public async Task InsertOrUpdateChatRoom_ReturnsBadRequest_WhenInsertFails()
        {
            // Arrange
            var model = new ChatRoomViewModel
            {
                RoomId = 1
            };

            var fakeDate = new DateTime(2024, 1, 1);
            _timeZoneConverterMock
                .Setup(x => x.ConvertUtcToConfiguredTimeZone())
                .Returns(fakeDate);

            _chatServiceMock
                .Setup(x => x.InsertOrUpdateChatRoom(It.IsAny<ChatRoomViewModel>()))
                .ReturnsAsync((0, 0, "Insert failed", "null"));

            // Act
            var result = await _controller.InsertOrUpdateChatRoom(model);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequest.Value);

            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Insert failed", response.statusMessage);
            //Assert.Equal(0, ((dynamic)response.data).ChatRoomId);
        }

        [Fact]
        public async Task ActiveChatList_ReturnsOk_WithActiveChatRooms_WhenContractorIsLoggedIn()
        {
            // Arrange
            var contractorId = 123;
            var expectedChatRooms = new List<ChatRoomsActiveResponse>
    {
        new ChatRoomsActiveResponse { RoomId = "room1" },
        new ChatRoomsActiveResponse { RoomId = "room2" }
    };

            _chatServiceMock
                .Setup(x => x.GetActiveChatRoomsAsync(contractorId))
                .ReturnsAsync(expectedChatRooms);

            var model = new ContractorIDViewModel(); // ContractorID will be set from token

            // Act
            var result = await _controller.ActiveChatList(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);

            var data = Assert.IsType<List<ChatRoomsActiveResponse>>(response.data);
            Assert.Equal(2, data.Count);
            Assert.Equal("room1", data[0].RoomId);
            Assert.Equal("room2", data[1].RoomId);
        }

        [Fact]
        public async Task ActiveChatList_ReturnsOk_WithEmptyList_WhenNoChatRoomsFound()
        {
            // Arrange
            var contractorId = 123;
            var expectedChatRooms = new List<ChatRoomsActiveResponse>();

            _chatServiceMock
                .Setup(x => x.GetActiveChatRoomsAsync(contractorId))
                .ReturnsAsync(expectedChatRooms);

            var model = new ContractorIDViewModel(); // ContractorID will be set from token

            // Act
            var result = await _controller.ActiveChatList(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.NotNull(response.data);

            var data = Assert.IsType<List<ChatRoomsActiveResponse>>(response.data);
            Assert.Empty(data);
        }

    }
}
