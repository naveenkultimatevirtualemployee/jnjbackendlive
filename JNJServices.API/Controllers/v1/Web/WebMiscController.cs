using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ViewModels.Web;
using Microsoft.AspNetCore.Mvc;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.Web
{
    //[Route("api/web/misc")]
    //[Route("api/v{version:apiVersion}/web/misc")]
    [Route("api")]
    [ApiController]
    public class WebMiscController : BaseController
    {
        private readonly IMiscellaneousService _miscellaneousService;
        private readonly ICustomerService _customerService;
        private readonly IClaimantService _claimantService;
        public WebMiscController(IMiscellaneousService miscellaneousService, ICustomerService customerService, IClaimantService claimantService)
        {
            _miscellaneousService = miscellaneousService;
            _customerService = customerService;
            _claimantService = claimantService;

        }

        [HttpGet("VehicleList")]
        public async Task<IActionResult> VehicleList()
        {
            ResponseModel response = new ResponseModel();

            var result = await _miscellaneousService.VehicleList();
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

        [HttpGet("LanguageList")]
        public async Task<IActionResult> LanguageList()
        {
            ResponseModel response = new ResponseModel();

            var result = await _miscellaneousService.LanguageList();
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

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            ResponseModel response = new ResponseModel();

            var result = await _miscellaneousService.GetDashboardData();
            if (result != null)
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

        [HttpGet("State")]
        public async Task<IActionResult> GetStates()
        {
            ResponseModel response = new ResponseModel();

            var result = await _miscellaneousService.GetStates();
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

        [HttpPost("CustomerClaimantSearch")]
        public async Task<IActionResult> ClaimantCustomerSearch([FromBody] ClaimantCustomerSearchWebViewModel model)
        {
            ResponseModel response = new ResponseModel();
            if (model.CustomerID > 0 && model.CustomerID != null)
            {
                var result = await _claimantService.ClaimantByCustomerSearch(model);
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
            }
            else if (model.ClaimantID > 0 && model.CustomerID != null)
            {
                var result = await _customerService.CustomerByClaimantSearch(model);
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
