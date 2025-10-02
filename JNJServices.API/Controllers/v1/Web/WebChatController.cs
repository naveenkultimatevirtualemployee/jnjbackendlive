using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.CommonModels;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Helper;
using Microsoft.AspNetCore.Mvc;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.Web
{
    [Route("api")]
    [ApiController]
    public class WebChatController : BaseController
    {
        private readonly IChatService _chatService;
        private readonly ITimeZoneConverter _timeZoneConverter;
        public WebChatController(IChatService chatService, ITimeZoneConverter timeZoneConverter)
        {
            _chatService = chatService;
            _timeZoneConverter = timeZoneConverter;
        }

        [HttpPost("GetChatRoomList")]
        public async Task<IActionResult> GetChatList([FromBody] ChatListViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            if (model == null || model.Page <= 0 || model.Limit <= 0)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.INVALID_INPUT_PARAMS;
                return BadRequest(response);
            }

            (List<ChatRoom>, int TotalCount) result = await _chatService.GetChatListAsync(model);

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;
            response.data = result.Item1;
            response.totalData = result.TotalCount;
            return Ok(response);
        }

        [HttpPost("GetChatMessages")]
        public async Task<IActionResult> GetChatMessages([FromBody] ChatMessageSearchViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            if (model == null || model.ChatRoomId <= 0 || model.Page <= 0 || model.Limit <= 0)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.INVALID_INPUT_PARAMS;
                return BadRequest(response);
            }

            (List<ChatMessage> messages, int TotalCount) = await _chatService.GetChatMessagesAsync(model);

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;
            response.data = messages;
            response.totalData = TotalCount;
            return Ok(response);
        }

        [HttpPost("ChatRoom")]
        public async Task<IActionResult> InsertOrUpdateChatRoom([FromBody] ChatRoomViewModel model)
        {
            ResponseModel response = new ResponseModel();

            if (model == null)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.INVALID_INPUT_PARAMS;
                return BadRequest(response);
            }
            model.CreateAt = _timeZoneConverter.ConvertUtcToConfiguredTimeZone();
            var (chatRoomId, responseCode, message, _) = await _chatService.InsertOrUpdateChatRoom(model);

            response.status = responseCode == 1 ? ResponseStatus.TRUE : ResponseStatus.FALSE;
            response.statusMessage = message;
            response.data = new { ChatRoomId = chatRoomId };

            return responseCode == 1 ? Ok(response) : BadRequest(response);

        }

        [HttpPost("ActiveChatRoomList")]
        public async Task<IActionResult> ActiveChatList([FromBody] ContractorIDViewModel model)
        {
            ResponseModel response = new ResponseModel();
            model.ContractorID = null;

            List<ChatRoomsActiveResponse> result = await _chatService.GetActiveChatRoomsAsync(model.ContractorID);

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;
            response.data = result;

            return Ok(response);
        }
    }
}
