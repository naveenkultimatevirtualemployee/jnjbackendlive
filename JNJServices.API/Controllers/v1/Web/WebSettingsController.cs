using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Helper;
using Microsoft.AspNetCore.Mvc;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.Web
{
	//[Route("api/v{version:apiVersion}/web/settings")]
	[Route("api")]
	[ApiController]
	public class WebSettingsController : BaseController
	{
		private readonly ISettingsService _settingsService;
		private readonly IConfiguration _configuration;
		private readonly char[] _seperaters = new[] { ';' };

		public WebSettingsController(ISettingsService settingsService, IConfiguration configuration)
		{
			_settingsService = settingsService;
			_configuration = configuration;
		}

		[HttpGet("Settings")]
		public async Task<IActionResult> Settings()
		{
			ResponseModel response = new ResponseModel();

			var result = await _settingsService.SettingsAsync();
			if (result != null && result.Any())
			{
				string imageUrl = _configuration.GetValue<string>("BaseUrl:Images") ?? string.Empty;
				foreach (var setting in result)
				{
					//if (setting.InputType == "file")
					//{
					//	setting.SettingValue = BaseUrlHelper.EnsureUrlHost(setting.SettingValue, imageUrl);
					//}
					setting.InputOptionsList = !string.IsNullOrEmpty(setting.InputOptions)
						? setting.InputOptions.Split(_seperaters, StringSplitOptions.RemoveEmptyEntries)
											  .Select(opt => opt.Trim())
											  .ToList()
						: new List<string>();
				}
				response.status = ResponseStatus.TRUE;
				response.statusMessage = ResponseMessage.SUCCESS;
				response.data = result;
			}
			else
			{
				response.status = ResponseStatus.FALSE;
				response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
			}

			return Ok(response);
		}

		[HttpPost("UpdateSettings")]
		public async Task<IActionResult> Update([FromBody] List<SettingWebViewModel> settings)
		{
			if (settings != null && settings.Count > 0)
			{
				var updateResult = await _settingsService.UpdateSettingsAsync(settings);

				if (updateResult.Item1 != 0)
				{
					var result = new ResponseModel
					{
						statusMessage = ResponseMessage.SUCCESS,
						data = updateResult.Item2,
						status = ResponseStatus.TRUE
					};
					return Ok(result);
				}
				else
				{
					var result = new ResponseModel
					{
						statusMessage = ResponseMessage.NOTUPDATED,
						data = updateResult.Item2,
						status = ResponseStatus.FALSE
					};
					return BadRequest(result);
				}
			}
			else
			{
				var result = new ResponseModel
				{
					statusMessage = ResponseMessage.DATA_NOT_FOUND,
					data = new object(),
					status = ResponseStatus.FALSE
				};
				return BadRequest(result);
			}
		}

		[HttpPost("SettingValueByKey")]
		public async Task<IActionResult> ShowSettingValue([FromBody] SettingKeyViewModel model)
		{

			var responseModel = await _settingsService.GetSettingByKeyAsync(model);

			if (!string.IsNullOrEmpty(responseModel.SettingValue))
			{
				var result = new ResponseModel
				{
					statusMessage = ResponseMessage.SUCCESS,
					data = responseModel,
					status = ResponseStatus.TRUE
				};
				return Ok(result);
			}
			else
			{
				var result = new ResponseModel
				{
					statusMessage = ResponseMessage.DATA_NOT_FOUND,
					data = responseModel,
					status = ResponseStatus.FALSE
				};
				return BadRequest(result);
			}
		}

	}
}
