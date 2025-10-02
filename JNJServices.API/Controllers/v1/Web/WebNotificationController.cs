using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Utility.Extensions;
using Microsoft.AspNetCore.Mvc;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.Web
{
	//[Route("api/v{version:apiVersion}/web/notification")]
	[Route("api")]
	[ApiController]
	public class WebNotificationController : BaseController
	{
		private readonly INotificationService _notificationService;

		public WebNotificationController(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		[HttpGet("NotificationsHistory")]
		public async Task<ActionResult> GetWebNotifications([FromQuery] int? page = null, [FromQuery] int? limit = null)
		{
			PaginatedResponseModel response = new PaginatedResponseModel();

			var userId = User.Identity.GetLoginUserId();
			if (string.IsNullOrEmpty(userId))
			{
				response.status = ResponseStatus.FALSE;
				response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
			}

			var notifications = await _notificationService.GetUserWebNotifications(userId, page, limit);

			if (notifications != null && notifications.Any())
			{
				response.status = ResponseStatus.TRUE;
				response.statusMessage = ResponseMessage.SUCCESS;
				response.data = notifications;
				response.totalData = notifications.FirstOrDefault()?.TotalCount ?? 0;
			}
			else
			{
				response.status = ResponseStatus.FALSE;
				response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
				response.totalData = 0;
			}

			return Ok(response);
		}

		[HttpGet("DeleteNotificationsHistory")]
		public async Task<ActionResult> DeleteWebNotifications()
		{
			ResponseModel response = new ResponseModel();

			var userId = User.Identity.GetLoginUserId();
			if (string.IsNullOrEmpty(userId))
			{
				response.status = ResponseStatus.FALSE;
				response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
				return Ok(response);
			}

			await _notificationService.DeleteUserWebNotifications(userId);

			response.status = ResponseStatus.TRUE;
			response.statusMessage = ResponseMessage.SUCCESS;
			response.data = ResponseMessage.NOTIFICATION_DELETED;

			return Ok(response);
		}
	}
}
