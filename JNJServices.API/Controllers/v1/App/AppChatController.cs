using JNJServices.API.Attributes;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.CommonModels;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Extensions;
using JNJServices.Utility.Helper;
using Microsoft.AspNetCore.Mvc;
using static JNJServices.Utility.UserResponses;
using Type = JNJServices.Utility.Enums.Type;

namespace JNJServices.API.Controllers.v1.App
{
    [Route("api-M")]
    //[Route("api/v{version:apiVersion}/app/contractor")]
    [ApiController]
    [ValidateAuthorize]
    public class AppChatController : BaseController
    {
        private readonly IChatService _chatService;
        private readonly ITimeZoneConverter _timeZoneConverter;
        public AppChatController(IChatService chatService, ITimeZoneConverter timeZoneConverter)
        {
            _chatService = chatService;
            _timeZoneConverter = timeZoneConverter;
        }

        [HttpPost("MobileGetChatRoomList")]
        public async Task<IActionResult> GetChatList([FromBody] ChatListViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            if (model == null || model.Page <= 0 || model.Limit <= 0)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.INVALID_INPUT_PARAMS;
                return BadRequest(response);
            }
            var tokenClaims = User.Identity.GetAppClaims();
            if (tokenClaims.Type == (int)Type.Contractor)
            {
                model.ContractorId = tokenClaims.UserID;
            }
            if (tokenClaims.Type == (int)Type.Claimant)
            {
                model.ContractorId = 0;
            }
            (List<ChatRoom>, int TotalCount) result = await _chatService.GetChatListAsync(model);

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;
            response.data = result.Item1;
            response.totalData = result.TotalCount;

            return Ok(response);
        }

        [HttpPost("MobileGetChatMessages")]
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

        [HttpPost("MobileCreateChatRoom")]
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

        [HttpPost("MobileActiveChatRoomList")]
        public async Task<IActionResult> ActiveChatList([FromBody] ContractorIDViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var tokenClaims = User.Identity.GetAppClaims();
            if (tokenClaims.Type == (int)Type.Contractor)
            {
                model.ContractorID = tokenClaims.UserID;
            }
            List<ChatRoomsActiveResponse> result = await _chatService.GetActiveChatRoomsAsync(model.ContractorID);

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;
            response.data = result;

            return Ok(response);
        }

    }
}
