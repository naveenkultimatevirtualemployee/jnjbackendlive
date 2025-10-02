using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ViewModels.Web;
using Microsoft.AspNetCore.Mvc;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.Web
{
    //[Route("api/v{version:apiVersion}/web/reservation")]
    [Route("api")]
    [ApiController]
    public class WebReservationController : BaseController
    {
        private readonly IReservationService _reservationService;

        public WebReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;

        }

        [HttpGet("resactioncode")]
        public async Task<IActionResult> ReservationActionCode()
        {
            ResponseModel response = new ResponseModel();

            var result = await _reservationService.ReservationActionCode();
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

        [HttpGet("resservice")]
        public async Task<IActionResult> ReservationService()
        {
            ResponseModel response = new ResponseModel();

            var result = await _reservationService.ReservationServices();
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

        [HttpGet("restriptype")]
        public async Task<IActionResult> ReservationTripType()
        {
            ResponseModel response = new ResponseModel();

            var result = await _reservationService.ReservationTripType();
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

        [HttpGet("restranstype")]
        public async Task<IActionResult> ReservationTransportType()
        {
            ResponseModel response = new ResponseModel();

            var result = await _reservationService.ReservationTransportType();
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

        [HttpPost("ReservationSearch")]
        public async Task<IActionResult> ReservationSearch([FromBody] ReservationSearchWebViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            var result = await _reservationService.ReservationSearch(model);
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

        [HttpPost("ReservationListSearch")]
        public async Task<IActionResult> ReservationListSearch([FromBody] ReservationSearchWebViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            var result = await _reservationService.ReservationListSearch(model);
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
