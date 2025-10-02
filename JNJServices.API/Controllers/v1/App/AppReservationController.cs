using JNJServices.API.Attributes;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ViewModels.App;
using JNJServices.Utility.Extensions;
using Microsoft.AspNetCore.Mvc;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.App
{
    [Route("api-M")]
    //[Route("api/v{version:apiVersion}/app/reservation")]
    [ValidateAuthorize]
    [ApiController]
    public class AppReservationController : BaseController
    {
        private readonly IReservationService _reservationService;
        public AppReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost("MobileReservationSearch")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<ReservationAppResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReservations([FromBody] ReservationSearchViewModel model)
        {
            PaginatedResponseModel rm = new PaginatedResponseModel();

            var tokenClaims = User.Identity.GetAppClaims();
            model.Type = tokenClaims.Type;

            if (tokenClaims.Type == (int)Utility.Enums.Type.Claimant)
            {
                model.ClaimantID = tokenClaims.UserID;
            }

            var response = await _reservationService.MobileReservationSearch(model);
            if (response != null && response.Any())
            {
                rm.status = ResponseStatus.TRUE;
                rm.statusMessage = ResponseMessage.SUCCESS;
                rm.data = response;
                rm.totalData = response.Select(s => s.TotalCount).FirstOrDefault();
                return Ok(rm);
            }
            rm.status = ResponseStatus.FALSE;
            rm.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            return Ok(rm);
        }
    }
}
