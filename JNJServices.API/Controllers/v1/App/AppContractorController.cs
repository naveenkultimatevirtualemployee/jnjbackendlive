using JNJServices.API.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace JNJServices.API.Controllers.v1.App
{
	[Route("api-M")]
	//[Route("api/v{version:apiVersion}/app/contractor")]
	[ApiController]
	[ValidateAuthorize]
	public class AppContractorController : BaseController
	{
	}
}
