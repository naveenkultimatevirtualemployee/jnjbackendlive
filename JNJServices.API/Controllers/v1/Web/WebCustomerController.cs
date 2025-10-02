using AutoMapper;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.ViewModels.Web;
using Microsoft.AspNetCore.Mvc;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.Web
{
    // [Route("api/web/customer")]
    //[Route("api/v{version:apiVersion}/web/customer")]
    [Route("api")]
    [ApiController]
    public class WebCustomerController : BaseController
    {
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;
        public WebCustomerController(ICustomerService customerService, IMapper mapper)
        {
            _customerService = customerService;
            _mapper = mapper;
        }

        [HttpGet("CustomerCategory")]
        public async Task<IActionResult> Category()
        {
            ResponseModel response = new ResponseModel();

            var result = await _customerService.GetCategory();
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

        [HttpPost("CustomerSearch")]
        public async Task<IActionResult> GetCustomerSearch([FromBody] CustomerSearchWebViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            var result = await _customerService.CustomerSearch(model);
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

        [HttpPost("CustomerSelDetail")]
        public async Task<IActionResult> CustomerSelDetail([FromBody] CustomerIDWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            CustomerSearchWebViewModel customerSearch = new CustomerSearchWebViewModel();
            customerSearch.CustomerID = model.CustomerID;

            var result = await _customerService.CustomerSearch(customerSearch);
            if (result != null && result.Any())
            {
                List<CustomerCompanyNameWebResponseModel> responseModels = new List<CustomerCompanyNameWebResponseModel>();
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = _mapper.Map(result.ToList(), responseModels);
            }
            else
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            return Ok(response);
        }

        [HttpPost("AllCustomer")]
        public async Task<IActionResult> AllCustomer(CustomerDynamicSearchWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _customerService.AllCustomers(model);
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
    }
}
