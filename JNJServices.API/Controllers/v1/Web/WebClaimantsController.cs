using AutoMapper;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.ViewModels.Web;
using Microsoft.AspNetCore.Mvc;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.Web
{
    //[Route("api/web/claimants")]
    //[Route("api/v{version:apiVersion}/web/claimants")]
    [Route("api")]
    [ApiController]
    public class WebClaimantsController : BaseController
    {
        private readonly IClaimantService _claimantService;
        private readonly IMapper _mapper;
        public WebClaimantsController(IClaimantService claimantService, IMapper mapper)
        {
            _claimantService = claimantService;
            _mapper = mapper;
        }

        [HttpPost("ClaimantSearch")]
        public async Task<IActionResult> ClaimantSearch([FromBody] ClaimantSearchViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            if ((model.ZipCode == null || model.ZipCode == 0) && (model.Miles != null && model.Miles != 0))
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = "Zipcode Required";
                return BadRequest(response);
            }

            if ((model.Miles == null || model.Miles == 0) && (model.ZipCode != null && model.ZipCode != 0))
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = "Miles Required";
                return BadRequest(response);
            }

            var result = await _claimantService.ClaimantSearch(model);


            if (result != null && result.Any())
            {
                if (model.CustomerID != null && model.CustomerID > 0)
                {
                    var finalresult = await _claimantService.AssignCustomerInfoToClaimants(result, model.CustomerID.Value);

                    response.status = ResponseStatus.TRUE;
                    response.statusMessage = ResponseMessage.SUCCESS;
                    response.data = result;
                    response.totalData = finalresult.Select(s => s.TotalCount).First();
                    return Ok(response);

                }
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

        [HttpPost("ClaimantSelDetail")]
        public async Task<IActionResult> ClaimantSelDetail([FromBody] ClaimantIDWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            ClaimantSearchViewModel claimantSearch = new ClaimantSearchViewModel();
            claimantSearch.ClaimantID = model.ClaimantID;
            var result = await _claimantService.ClaimantSearch(claimantSearch);

            List<ClaimantFullNameResponseModel> claimants = new List<ClaimantFullNameResponseModel>();

            if (result != null && result.Any())
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = _mapper.Map(result.ToList(), claimants);
            }
            else
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            return Ok(response);
        }

        [HttpPost("AllClaimant")]
        public async Task<IActionResult> AllClaimant(ClaimantDynamicSearchWebViewModel dynamicSearch)
        {
            ResponseModel response = new ResponseModel();

            var result = await _claimantService.AllClaimant(dynamicSearch);
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

        [HttpPost("ClaimantListSearch")]
        public async Task<IActionResult> ClaimantListSearch([FromBody] ClaimantSearchViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            if ((model.ZipCode == null || model.ZipCode == 0) && (model.Miles != null && model.Miles != 0))
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = "Zipcode Required";
                return BadRequest(response);
            }

            if ((model.Miles == null || model.Miles == 0) && (model.ZipCode != null && model.ZipCode != 0))
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = "Miles Required";
                return BadRequest(response);
            }

            var result = await _claimantService.ClaimantListSearch(model);


            if (result != null && result.Any())
            {
                if (model.CustomerID != null && model.CustomerID > 0)
                {

                    response.status = ResponseStatus.TRUE;
                    response.statusMessage = ResponseMessage.SUCCESS;
                    response.data = result;
                    response.totalData = result.Select(s => s.TotalCount).First();
                    return Ok(response);

                }
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
