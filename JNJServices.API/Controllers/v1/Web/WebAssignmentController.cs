using JNJServices.API.Helper;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Helper;
using Microsoft.AspNetCore.Mvc;
using static JNJServices.Utility.ApiConstants.NotificationConstants;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.Web
{
    //[Route("api/web/assignment")]
    //[Route("api/v{version:apiVersion}/web/assignment")]
    [Route("api")]
    [ApiController]
    public class WebAssignmentController : BaseController
    {
        private readonly IReservationAssignmentsService _reservationAssignmentsService;
        private readonly IConfiguration _configuration;
        private readonly IAssignmentMetricsService _assignmentMetricsService;
        private readonly NotificationHelper _notificationHelper;
        public WebAssignmentController(IReservationAssignmentsService reservationAssignmentsService, IConfiguration configuration, IAssignmentMetricsService assignmentMetricsService, NotificationHelper notificationHelper)
        {
            _reservationAssignmentsService = reservationAssignmentsService;
            _configuration = configuration;
            _assignmentMetricsService = assignmentMetricsService;
            _notificationHelper = notificationHelper;
        }

        [HttpGet("ResAssgnType")]
        public async Task<IActionResult> ReservationAssignmentType()
        {
            ResponseModel response = new ResponseModel();
            var result = await _reservationAssignmentsService.ReservationAssignmentType();
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

        [HttpPost("ReservationAssignments")]
        public async Task<IActionResult> ReservationAssignmentSearch([FromBody] ReservationAssignmentWebViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            var result = await _reservationAssignmentsService.ReservationAssignmentSearch(model);
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

        [HttpPost("ResAssignContractor")]
        public async Task<IActionResult> ResAssignContractor([FromBody] AssignAssignmentContractorWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _reservationAssignmentsService.AssignAssignmentContractor(model);
            if (result != null && result.Any())
            {
                ReservationNotificationWebViewModel reservationNotification = new ReservationNotificationWebViewModel();
                reservationNotification.Type = 1;
                reservationNotification.ReservationAssignmentID = model.reservationassignmentid;

                if (model.AssignType == ButtonStatus.REQUEST)
                {
                    reservationNotification.ButtonStatus = ButtonStatus.REQUEST;
                    reservationNotification.NotificationTitle = NotificationTitle.NEW_ASSIGNMENT_REQUEST;
                    await _notificationHelper.SendNotificationWebForceRequest(reservationNotification);
                }
                if (model.AssignType == ButtonStatus.FORCED)
                {
                    reservationNotification.ContractorID = model.contractorid.HasValue ? model.contractorid.Value : 0;
                    reservationNotification.ButtonStatus = ButtonStatus.FORCED;
                    reservationNotification.NotificationTitle = NotificationTitle.NEW_ASSIGNMENT_ASSIGNED;
                    await _notificationHelper.SendNotificationWebForceRequest(reservationNotification);
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

        [HttpPost("TrackAssignment")]
        public async Task<IActionResult> TrackAssignmentByID([FromBody] AssignmentIDWebViewModel model)
        {
            ResponseModel response = new ResponseModel();
            string imageUrl = _configuration.GetValue<string>("BaseUrl:Images") ?? string.Empty;

            var assignmentTrackingData = await _reservationAssignmentsService.WebTrackAssignmentByID(model.ReservationsAssignmentsID.HasValue ? model.ReservationsAssignmentsID.Value : 0);


            foreach (var item in assignmentTrackingData.Where(x => (x.ImageUrl != null || x.DeadMileImageUrl != null)))
            {
                item.ImageUrl = BaseUrlHelper.EnsureUrlHost(item.ImageUrl ?? string.Empty, imageUrl);
                item.DeadMileImageUrl = BaseUrlHelper.EnsureUrlHost(item.DeadMileImageUrl ?? string.Empty, imageUrl);
            }

            if (assignmentTrackingData.Any())
            {
                var result = await _reservationAssignmentsService.RetrieveAssignmentAndContractorDetails(assignmentTrackingData.ToList());
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

        [HttpPost("GetLiveCoordinates")]
        public async Task<IActionResult> TrakingLive([FromBody] TrackingAssignmentIDViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var liveCoordinates = _reservationAssignmentsService.GetLiveTrackingCoordinates(model);
            var googleCoordinates = _reservationAssignmentsService.GetGooglePathCoordinates(model.ReservationsAssignmentsID);

            var result = new LiveTrackingMapResponseModel
            {
                GooglePath = await googleCoordinates,
                ReservationsAssignmentsID = model.ReservationsAssignmentsID,
                LatitudeLongitude = await liveCoordinates
            };

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;
            response.data = result;

            return Ok(response);
        }

        [HttpPost("ViewUserFeedback")]
        public async Task<IActionResult> ViewUserFeedback([FromBody] AssignmentIDWebViewModel feedback)
        {
            ResponseModel response = new ResponseModel();
            AssignmentIDAppViewModel viewModel = new AssignmentIDAppViewModel();
            viewModel.ReservationAssignmentID = feedback.ReservationsAssignmentsID.HasValue ? feedback.ReservationsAssignmentsID.Value : 0;
            var result = await _reservationAssignmentsService.ViewUserFeedback(viewModel);

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;
            response.data = result;

            return Ok(response);
        }

        [HttpPost("AssignmnetMetricsDetails")]
        public async Task<IActionResult> AssignmentMetricsDetails([FromBody] AssignmentIDWebViewModel model)
        {
            ResponseModel response = new ResponseModel();
            WebMetricsResponseModel webMetrics = new WebMetricsResponseModel();
            webMetrics.metricsResponse = await _assignmentMetricsService.FetchAssignmentMetrics(model.ReservationsAssignmentsID.HasValue ? model.ReservationsAssignmentsID.Value : 0);

            webMetrics.MetricsUploadedDocuments = await _assignmentMetricsService.FetchMetricsUploadedDocuments(model.ReservationsAssignmentsID.HasValue ? model.ReservationsAssignmentsID.Value : 0);
            if (webMetrics.metricsResponse.Count > 0)
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = webMetrics;
            }
            else
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }

            return Ok(response);
        }

        [HttpPost("AssignmentMetricSearch")]
        public async Task<IActionResult> ReservationAssignmentMetricSearch([FromBody] AssignmentMetricsSearchWebViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            var result = await _assignmentMetricsService.MetricsSearch(model);
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

        [HttpPost("AssignmentContractorTimeSlot")]
        public async Task<IActionResult> CheckAssignmentContractorTimeSlot([FromBody] ReservationCheckTimeSlotViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _reservationAssignmentsService.CheckReservationTimeSlotAsync(model);
            if (result.ResponseCode == 1)
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = result.Message;
                response.data = result.Item3;

            }
            else
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = result.Message;
                response.data = result.Item3;
            }

            return Ok(response);
        }

        [HttpPost("ReservationAssignmentsList")]
        public async Task<IActionResult> ReservationAssignmentListSearch([FromBody] ReservationAssignmentWebViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            var result = await _reservationAssignmentsService.ReservationAssignmentListSearch(model);
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

        [HttpPost("AssignmentMetricListSearch")]
        public async Task<IActionResult> ReservationAssignmentMetricListSearch([FromBody] AssignmentMetricsSearchWebViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            var result = await _assignmentMetricsService.MetricsListSearch(model);
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
