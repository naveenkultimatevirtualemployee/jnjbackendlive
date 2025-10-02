using JNJServices.API.Attributes;
using JNJServices.API.Helper;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.CommonModels;
using JNJServices.Models.ViewModels.App;
using JNJServices.Utility.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.App
{
    [Route("api-M")]
    //[Route("api/v{version:apiVersion}/app/notification")]
    [ApiController]
    [ValidateAuthorize]
    public class AppNotificationController : BaseController
    {
        private readonly INotificationHelper _notificationHelper;
        private readonly INotificationService _notificationService;
        private readonly IChatService _chatService;
        private readonly ILogger<AppNotificationController> _logger;

        public AppNotificationController(INotificationHelper notificationHelper, INotificationService notificationService, IChatService chatService, ILogger<AppNotificationController> logger)
        {
            _notificationHelper = notificationHelper;
            _notificationService = notificationService;
            _chatService = chatService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("MobileNotification")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationMessageAppViewModel model)
        {
            await _notificationHelper.SendNotificationAsync(model);
            return Ok();
        }

        [HttpGet("MobileNotificationsHistory")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<AppNotificationsResponseModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetNotifications([FromQuery] int? page = null, [FromQuery] int? limit = null)
        {
            PaginatedResponseModel rm = new PaginatedResponseModel();

            var tokenClaims = User.Identity.GetAppClaims();

            var response = await _notificationService.GetNotificationsAsync(tokenClaims, page, limit);

            rm.status = ResponseStatus.TRUE;
            rm.statusMessage = ResponseMessage.SUCCESS;
            rm.data = response;
            rm.totalData = response?.FirstOrDefault()?.TotalCount ?? 0;

            return Ok(rm);
        }

        [HttpGet("MobileDeleteNotificationsHistory")]
        public async Task<ActionResult> DeleteNotifications()
        {
            ResponseModel rm = new ResponseModel();

            var tokenClaims = User.Identity.GetAppClaims();

            await _notificationService.DeleteNotificationAsync(tokenClaims);
            rm.status = ResponseStatus.TRUE;
            rm.statusMessage = ResponseMessage.SUCCESS;

            return Ok(rm);
        }

        [AllowAnonymous]
        [HttpGet("AppandWebSendNotification")]
        public async Task<IActionResult> SendNotificationasync(string roomId, int type, int senderId)
        {
            try
            {
                var fcmTokens = await _chatService.GetAllConnectionsForRoomFcmTokenAsync(roomId, type, senderId.ToString());
                WebNotificationModel notificationModel = new WebNotificationModel
                {
                    FcmToken = fcmTokens.Where(x => x.Type == 3).Select(x => x.FcmToken).ToList(),
                    data = new Dictionary<string, string>
                    {
                        { "roomid", roomId ?? "" },
                        { "type", type.ToString()??"" }, // ✅ Ensure integer is converted to string
                    { "senderid", senderId.ToString() ?? "" } // ✅ Ensure integer is converted to string
                     
                }
                };
                await _notificationHelper.SendWebPushNotifications(notificationModel);

                return Ok(new { status = true, message = "Notification sent." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending push notification.");
                return StatusCode(500, new { status = false, message = "Internal server error" });
            }
        }
    }
}
