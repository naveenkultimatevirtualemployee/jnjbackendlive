using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ViewModels.Web;
using Microsoft.AspNetCore.Mvc;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.Web
{
    // [Route("api/web/claims")]
    //[Route("api/v{version:apiVersion}/web/claims")]
    [Route("api")]
    [ApiController]
    public class WebClaimsController : BaseController
    {
        private readonly IClaimsService _claimsService;
        public WebClaimsController(IClaimsService claimsService)
        {
            _claimsService = claimsService;
        }

        [HttpPost("ClaimsSearch")]
        public async Task<IActionResult> ClaimsSearch([FromBody] ClaimsSearchWebViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            var result = await _claimsService.ClaimsSearch(model);

            if (result != null && result.Any())
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
                response.totalData = result.Select(s => s.TotalCount).First();
            }
            else
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }

            return Ok(response);
        }

        [HttpPost("ClaimsFacilities")]
        public async Task<IActionResult> ClaimsFacilities([FromBody] ClaimsIDWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _claimsService.ClaimsFacility(model);

            if (result != null && result.Any())
            {
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

        [HttpPost("ClaimsApprovedContractors")]
        public async Task<IActionResult> ClaimsApprovedContractors([FromBody] ClaimsIDWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _claimsService.ClaimsApprovedContractors(model);
            if (result != null && result.Any())
            {
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

        [HttpPost("ClaimsListSearch")]
        public async Task<IActionResult> ClaimsListSearch([FromBody] ClaimsSearchWebViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            var result = await _claimsService.ClaimsListSearch(model);

            if (result != null && result.Any())
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
                response.totalData = result.Select(s => s.TotalCount).First();
            }
            else
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }

            return Ok(response);
        }
    }
}
