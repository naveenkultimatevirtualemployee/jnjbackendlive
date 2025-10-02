using JNJServices.API.Attributes;
using JNJServices.API.Helper;
using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.Entities;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.ApiConstants;
using JNJServices.Utility.Extensions;
using JNJServices.Utility.Helper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations;
using static JNJServices.Utility.ApiConstants.NotificationConstants;
using static JNJServices.Utility.UserResponses;
using Type = JNJServices.Utility.Enums.Type;

namespace JNJServices.API.Controllers.v1.App
{
    [Route("api-M")]
    //[Route("api/v{version:apiVersion}/app/assignment")]
    [ApiController]
    [ValidateAuthorize]
    public class AppAssignmentController : BaseController
    {
        private readonly IReservationAssignmentsService _reservationAssignmentsService;
        private readonly IAssignmentMetricsService _assignmentMetrics;
        private readonly ITimeZoneConverter _timeZoneService;
        private readonly IConfiguration _configuration;
        private readonly INotificationHelper _notificationHelper;
        private readonly ILogger<AppAssignmentController> _logger;

        public AppAssignmentController(IReservationAssignmentsService reservationAssignmentsService, ITimeZoneConverter timeZoneService, IAssignmentMetricsService assignmentMetrics, IConfiguration configuration, INotificationHelper notificationHelper, ILogger<AppAssignmentController> logger)
        {
            _reservationAssignmentsService = reservationAssignmentsService;
            _timeZoneService = timeZoneService;
            _assignmentMetrics = assignmentMetrics;
            _configuration = configuration;
            _notificationHelper = notificationHelper;
            _logger = logger;
        }

        [HttpPost("MobileAssignmentSearch")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<ReservationAssignmentAppResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAssignments([FromBody] AppReservationAssignmentSearchViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            var tokenClaims = User.Identity.GetAppClaims();
            model.Type = tokenClaims.Type;
            if (tokenClaims.Type == (int)Type.Contractor)
            {
                model.ContractorID = tokenClaims.UserID;
            }
            else if (tokenClaims.Type == (int)Type.Claimant)
            {
                model.ClaimantID = tokenClaims.UserID;
            }

            var reservationList = await _reservationAssignmentsService.MobileReservationAssignmentSearch(model);
            if (reservationList.Any())
            {
                var result = await _reservationAssignmentsService.ProcessAssignmentsAsync(reservationList.ToList());
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
                response.totalData = reservationList.Select(s => s.TotalCount).First();
            }
            else
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }

            return Ok(response);
        }

        [HttpPost("MobileAssignmentAcceptCancel")]
        public async Task<IActionResult> AcceptCancelAssignment([FromBody] ReservationAssignmentAcceptRejectAppViewModel model)
        {
            ResponseModel response = new ResponseModel();
            var tokenClaims = User.Identity.GetAppClaims();
            if (tokenClaims.Type == 2 && (model.ReservationID == null || model.ReservationID <= 0))
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = "ReservationID Not Found";
                return BadRequest(response);
            }
            model.Type = tokenClaims.Type;

            if (tokenClaims.Type == 1)
            {
                model.ContractorID = tokenClaims.UserID;
            }
            var result = await _reservationAssignmentsService.MobileAcceptRejectAssignment(model);

            if (result.Item1 != 1)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = result.Item2;
                response.data = result.Item2;
                return BadRequest(response);
            }
            else
            {
                ReservationNotificationAppViewModel reservation = new ReservationNotificationAppViewModel();
                reservation.Type = model.Type;
                reservation.ButtonStatus = model.ButtonStatus;

                if ((model.Type == 2 && model.ButtonStatus == ButtonStatus.ACCEPT) || (model.Type == 1 && model.ButtonStatus == ButtonStatus.CANCEL))
                {
                    reservation.ReservationID = model.ReservationID;
                    reservation.ReservationAssignmentID = model.AssignmentID;
                    reservation.NotificationTitle = NotificationTitle.NEW_ASSIGNMENT_REQUEST;
                    await _notificationHelper.SendNotificationClaimantContractorAccept(reservation);
                }
                else if (model.Type == 1 && model.ButtonStatus == ButtonStatus.ACCEPT)
                {
                    reservation.ReservationAssignmentID = model.AssignmentID;
                    reservation.ContractorID = model.ContractorID.HasValue ? model.ContractorID.Value : 0;
                    reservation.NotificationTitle = NotificationTitle.NEW_ASSIGNMENT_ASSIGNED;
                    await _notificationHelper.SendNotificationClaimantContractorAccept(reservation);
                }

                response.status = ResponseStatus.TRUE;
                response.statusMessage = result.Item2;
                response.data = result.Item2;
                return Ok(response);
            }
        }

        [HttpPost("MobileAssignmentCancel")]
        public async Task<IActionResult> MobileAssignmentCancelAfterAccept([FromBody] UpdateAssignmentStatusViewModel model)
        {
            ResponseModel response = new ResponseModel();
            var tokenClaims = User.Identity.GetAppClaims();

            model.Type = tokenClaims.Type;
            model.CancelBy = tokenClaims.UserID;
            model.CancelDate = _timeZoneService.ConvertUtcToConfiguredTimeZone();
            model.CancelTime = _timeZoneService.ConvertUtcToConfiguredTimeZone();
            if (tokenClaims.Type == (int)Type.Claimant && (model.ReservationID == null || model.ReservationID <= 0))
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = "ReservationID not Found";
                return BadRequest(response);
            }

            var result = await _reservationAssignmentsService.MobileCancelAfterAccept(model);
            if (result.Item1 != 1)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
                response.data = result.Item2;
                return BadRequest(response);
            }
            else
            {
                if (model.Type == 1 && model.ButtonStatus == ButtonStatus.CANCEL)
                {
                    ReservationNotificationAppViewModel reservation = new ReservationNotificationAppViewModel();
                    reservation.ReservationAssignmentID = model.AssignmentID;
                    reservation.NotificationTitle = NotificationTitle.ASSIGNMENT_CANCELLED;
                    await _notificationHelper.SendNotificationContractorCancel(reservation);
                }
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result.Item2;
                return Ok(response);
            }
        }

        [HttpGet("MobileTrackAssignmentByID/{ReservationAssignmentID}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<TrackAssignmentByIDResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> TrackAssignment(int ReservationAssignmentID)
        {
            ResponseModel response = new ResponseModel();

            var assignmentTrackingData = await _reservationAssignmentsService.TrackAssignmentByID(ReservationAssignmentID);

            string imageUrl = _configuration.GetValue<string>("BaseUrl:Images") ?? string.Empty;

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
                return Ok(response);
            }

            response.status = ResponseStatus.FALSE;
            response.data = assignmentTrackingData;
            return Ok(response);
        }

        [HttpPost("MobileAssignmentDetail")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<ReservationAssignmentAppResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAssignmentDetails([FromBody][Required] AssignmentMetricsViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();
            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;

            AppReservationAssignmentSearchViewModel assignmentSearchViewModel = new AppReservationAssignmentSearchViewModel();
            assignmentSearchViewModel.ReservationsAssignmentsID = model.ReservationsAssignmentsID;
            assignmentSearchViewModel.IsContractorPendingflag = 0;

            var result = await _reservationAssignmentsService.MobileReservationAssignmentSearch(assignmentSearchViewModel);
            if (result != null && result.Any())
            {
                result = await _reservationAssignmentsService.ProcessAssignmentsAsync(result.ToList());
                response.data = result;
                response.totalData = result.Select(s => s.TotalCount).First();
            }

            return Ok(response);
        }

        [HttpGet("MobilePastTrackAssignmentByID/{ReservationAssignmentID}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<ReservationAssignmentAppResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PastTrackAssignmentByID(int ReservationAssignmentID)
        {
            ResponseModel response = new ResponseModel();

            var result = await _reservationAssignmentsService.TrackAssignmentByID(ReservationAssignmentID);
            response.status = ResponseStatus.TRUE;
            response.data = result;

            return Ok(response);
        }

        [HttpPost("MobileAssignmentTracking")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<AssignmentTrackingResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignmentTracking([FromBody] ReservationAssignmentTrackingViewModel model)
        {
            ResponseModel response = new ResponseModel();
            if ((model.ButtonID == 0) || (model.ButtonID == 1 && (model.ClaimantID == null || model.ClaimantID == 0)))
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.INVALID_INPUT_PARAMS;
                return BadRequest(response);
            }

            var tokenClaims = User.Identity.GetAppClaims();
            if (tokenClaims.Type == (int)Type.Contractor)
            {
                model.ContractorID = tokenClaims.UserID;
                model.CreateUserID = Convert.ToInt32(tokenClaims.UserID);
                model.CreateDate = _timeZoneService.ConvertUtcToConfiguredTimeZone();
            }

            model.TravelledDateandTime = _timeZoneService.ConvertUtcToConfiguredTimeZone();

            TrackingAssignmentIDViewModel assignmentID = new TrackingAssignmentIDViewModel();
            assignmentID.ReservationsAssignmentsID = model.ReservationAssignmentID.HasValue ? model.ReservationAssignmentID.Value : 0;
            assignmentID.AssignmentTrackingID = model.AssignmentTrackingID.HasValue ? model.AssignmentTrackingID.Value : 0;

            if (model.ButtonID == 2)
            {

                assignmentID.isDeadMile = 1;
                List<Coordinate> assignmentCoordinates = await _reservationAssignmentsService.GetCoordinatesFromDatabase(assignmentID);
                if (assignmentCoordinates != null && assignmentCoordinates.Count >= 2)
                {
                    model.DeadMiles = (decimal)DistanceCalculator.CalculateTotalDistance(assignmentCoordinates);
                }
            }
            if (model.ButtonID == 4)
            {
                assignmentID.isDeadMile = 0;
                List<Coordinate> assignmentCoordinates = await _reservationAssignmentsService.GetCoordinatesFromDatabase(assignmentID);
                if (assignmentCoordinates != null && assignmentCoordinates.Count >= 2)
                {
                    model.TravellingMiles = (decimal)DistanceCalculator.CalculateTotalDistance(assignmentCoordinates);
                }
            }

            var result = await _reservationAssignmentsService.AssignmentTracking(model);
            if (result.Item2 == 0)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = result.Item3;
                response.data = result.Item1;
                return BadRequest(response);
            }
            else
            {
                if (model.ButtonID != 5)
                {
                    ClaimantTrakingAppViewModel reservation = new ClaimantTrakingAppViewModel();
                    reservation.ReservationAssignmentID = model.ReservationAssignmentID;
                    reservation.CurrentButtonID = model.ButtonID;
                    reservation.ButtonStatus = model.ButtonStatus;
                    await _notificationHelper.SendNotificationDriverTraking(reservation);
                }

                var resultFinal = await _reservationAssignmentsService.FetchOtherRecordsForTracking(result.Item1.ToList());
                response.status = ResponseStatus.TRUE;
                response.statusMessage = result.Item3;
                response.data = resultFinal;
                return Ok(response);
            }
        }

        [HttpPost("MobileSendLiveCoordinates")]
        public async Task<IActionResult> SendLiveCoordinates([FromBody] SaveLiveTrackingCoordinatesViewModel model)
        {
            ResponseModel response = new ResponseModel();

            model.TrackingDateTime = _timeZoneService.ConvertUtcToConfiguredTimeZone();
            await _reservationAssignmentsService.LiveTraking(model);

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;
            return Ok(response);
        }

        [HttpPost("MobileGetLiveCoordinates")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<LiveTrackingMapResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLiveCoordinates([FromBody] TrackingAssignmentIDViewModel model)
        {
            ResponseModel response = new ResponseModel();
            var liveCoordinates = await _reservationAssignmentsService.GetLiveTrackingCoordinates(model);

            // Fetch Google path coordinates
            var googleCoordinates = await _reservationAssignmentsService.GetGooglePathCoordinates(model.ReservationsAssignmentsID);

            // Construct the response model
            var result = new LiveTrackingMapResponseModel
            {
                GooglePath = googleCoordinates,
                ReservationsAssignmentsID = model.ReservationsAssignmentsID,
                LatitudeLongitude = liveCoordinates
            };

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;
            response.data = result;

            return Ok(response);
        }

        [HttpPost("MobileAssignmentTrackingOtherRecord")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<AssignmentTrackOtherRecordResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignmentTrackWaitingRecords([FromBody] ReservationAssignmentTrackOtherRecordsViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var tokenClaims = User.Identity.GetAppClaims();

            if (tokenClaims.Type == (int)Type.Contractor)
            {
                model.ContractorID = tokenClaims.UserID.HasValue ? tokenClaims.UserID.Value : 0;
                model.CreatedUserID = tokenClaims.UserID.HasValue ? tokenClaims.UserID.Value : 0;
                model.CreatedDate = _timeZoneService.ConvertUtcToConfiguredTimeZone();
            }

            model.WaitingDateandTime = _timeZoneService.ConvertUtcToConfiguredTimeZone();
            var result = await _reservationAssignmentsService.AssignmentTrackingOtherRecords(model);
            if (result.Item1 == 0)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = result.Item2;
                response.data = result.Item3;
                return BadRequest(response);
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = result.Item2;
                response.data = result.Item3;
                return Ok(response);
            }
        }

        [HttpGet("MobileOngoingAssignment")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<OngoingAssignmentTrackingResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MobileOngoingAssignment()
        {
            ResponseModel response = new ResponseModel();

            var tokenClaims = User.Identity.GetAppClaims();

            var result = await _reservationAssignmentsService.OnGoingAssignment(tokenClaims);

            response.status = ResponseStatus.TRUE;
            response.data = result;
            return Ok(response);
        }

        [HttpGet("MobileReservationAssignmentCancelList")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<ReservationCancelDetails>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReservationAssignmentCancelStatus()
        {
            ResponseModel response = new ResponseModel();
            var tokenClaims = User.Identity.GetAppClaims();

            var result = await _reservationAssignmentsService.ReservationAssignmentCancelStatus(tokenClaims.Type.HasValue ? tokenClaims.Type.Value : 0);
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

        //Metrics
        [HttpPost("MobileAssignmnetMetricsDetails")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<MobileAssignmentMetricsResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignmentMetricsDetails([FromBody][Required] AssignmentMetricsViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var assignmentMetrics = _assignmentMetrics.FetchAssignmentMetrics(model.ReservationsAssignmentsID);

            var waitingRecordList = _assignmentMetrics.FetchWaitingRecords(model.ReservationsAssignmentsID);

            var MetricsUploadList = _assignmentMetrics.FetchMetricsUploadedDocuments(model.ReservationsAssignmentsID);

            MobileAssignmentMetricsResponseModel metricsResponseModel = new MobileAssignmentMetricsResponseModel();
            metricsResponseModel.metricsResponse = await assignmentMetrics;
            metricsResponseModel.WaitingRecordsList = await waitingRecordList;
            metricsResponseModel.MetricsUploadedDocuments = await MetricsUploadList;

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;
            response.data = metricsResponseModel;

            return Ok(response);
        }

        [HttpPost("MobileSubmitAssignmentMetrics")]
        public async Task<IActionResult> SaveAssignmentMetrics([FromBody] List<SaveAssignmentMetricsViewModel> assignmentMetrics)
        {
            ResponseModel response = new ResponseModel();
            _logger.LogInformation("SaveAssignmentMetrics called with metrics: {MetricsJson}", JsonConvert.SerializeObject(assignmentMetrics));
            await _assignmentMetrics.InsertAssignmentMetricsFromJsonAsync(assignmentMetrics);

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;
            response.data = ResponseMessage.INSERT_ASSIGNMENT_METRICS_SUCCESS;

            return Ok(response);
        }

        [HttpPost("InsertAssignmentWaitingRecordMetrics")]
        public async Task<IActionResult> SaveAssignmentTrackRecordFromMetrics([FromBody] SaveAssignmentTrackRecordViewModel assignmentMetrics)
        {

            ResponseModel response = new ResponseModel();
            var tokenClaims = User.Identity.GetAppClaims();

            assignmentMetrics.ContractorID = tokenClaims.UserID.HasValue ? tokenClaims.UserID.Value : 0;
            var result = await _assignmentMetrics.InsertReservationAssignmentTrackRecordMetrics(assignmentMetrics);

            var assignmentMetricsdata = await _assignmentMetrics.FetchAssignmentMetrics(assignmentMetrics.ReservationsAssignmentsID);

            var waitingRecordList = await _assignmentMetrics.FetchWaitingRecords(assignmentMetrics.ReservationsAssignmentsID);

            MobileAssignmentMetricsResponseModel metricsResponseModel = new MobileAssignmentMetricsResponseModel();
            metricsResponseModel.metricsResponse = assignmentMetricsdata;
            metricsResponseModel.WaitingRecordsList = waitingRecordList;
            if (result == 0)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_INSERTED;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = metricsResponseModel;
            }
            return Ok(response);
        }

        [HttpGet("DeleteAssignmentWaitingRecordMetrics/{WaitingID}/{ReservationAssignmentID}")]
        public async Task<IActionResult> DeleteReservationAssignmentTrackRecordMetrics(int WaitingID, int ReservationAssignmentID)
        {
            var response = new ResponseModel();

            var result = await _assignmentMetrics.DeleteReservationAssignmentTrackRecordMetrics(WaitingID);
            var assignmentMetricsdata = await _assignmentMetrics.FetchAssignmentMetrics(ReservationAssignmentID);

            var waitingRecordList = await _assignmentMetrics.FetchWaitingRecords(ReservationAssignmentID);

            MobileAssignmentMetricsResponseModel metricsResponseModel = new MobileAssignmentMetricsResponseModel();
            metricsResponseModel.metricsResponse = assignmentMetricsdata;
            metricsResponseModel.WaitingRecordsList = waitingRecordList;
            response.status = result == 0 ? ResponseStatus.FALSE : ResponseStatus.TRUE;
            response.statusMessage = result == 0 ? ResponseMessage.DATA_NOT_DELETED : ResponseMessage.SUCCESS;
            response.data = metricsResponseModel;
            return Ok(response);
        }

        [HttpPost("MobileUsersFeedback")]
        public async Task<IActionResult> UsersFeedback([FromBody] UserFeedbackAppViewModel model)
        {
            ResponseModel response = new ResponseModel();
            var tokenClaims = User.Identity.GetAppClaims();

            if (model.CreatedDate == null)
            {
                model.CreatedDate = _timeZoneService.ConvertUtcToConfiguredTimeZone();
            }
            model.CreatedUserID = tokenClaims.UserID;

            var result = await _reservationAssignmentsService.UserFeedback(model);
            if (result.Item1 == 0)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = result.Item2;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = result.Item2;
            }
            return Ok(response);
        }

        [HttpPost("MobileViewUsersFeedback")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<UserFeedbackAppResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ViewUsersFeedback([FromBody] AssignmentIDAppViewModel feedback)
        {
            ResponseModel response = new ResponseModel();

            var result = await _reservationAssignmentsService.ViewUserFeedback(feedback);

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;
            response.data = result;

            return Ok(response);
        }

        [HttpPost("MobileAssignmentAssgnStatus")]
        public async Task<IActionResult> AssignmentAssgnStatus([FromBody] AssignmentAssgnStatusAppViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _reservationAssignmentsService.CheckReservationAssignmentAsync(model.ReservationAssignmentID, model.Notes);
            if (result.Item1 == 0)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = result.Item2;
                response.data = response.data = new
                {
                    Message = result.Item2,
                    ShowNotesBox = result.Item3
                };
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = result.Item2;
                response.data = new
                {
                    Message = result.Item2,
                    ShowNotesBox = result.Item3
                };
            }
            return Ok(response);
        }

        [HttpPost("MobileRelatedAssignment")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<ReservationAssignmentAppResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRelatedAssignmentDetails([FromBody][Required] ReservationIDAppViewModel model)
        {
            ResponseModel response = new ResponseModel();
            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;

            var result = await _reservationAssignmentsService.GetAllRelatedAssignmentsAsync(model.ReservationID);
            response.data = result.Item1;

            return Ok(response);
        }

        [HttpPost("AssignmentsMetricsUpload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AssignmentsMetricsUpload([FromForm] AssignmentIDWebViewModel model, [FromForm] List<IFormFile> files)
        {

            var allowedContentTypes = new[]
            {
				// Images
				"image/jpeg", "image/jpg", "image/png", 

				// PDF
				"application/pdf",                      

				// Word Documents
				"application/vnd.openxmlformats-officedocument.wordprocessingml.document", // DOCX
				"application/msword",                    // DOC (older Word format)

				// Plain Text
				"text/plain",                            // TXT

				// Excel Files
				"application/vnd.ms-excel",              // XLS (older Excel format)
				"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // XLSX (new Excel format)

				// CSV File
				"text/csv",                              

				// PowerPoint Files
				"application/vnd.ms-powerpoint",         // PPT (older PowerPoint format)
				"application/vnd.openxmlformats-officedocument.presentationml.presentation" // PPTX (new PowerPoint format)
			};


            long maxFileSize = 10 * 1024 * 1024; // 10MB max file size

            if (files == null || files.Count == 0)
            {
                return BadRequest(new ResponseModel
                {
                    status = ResponseStatus.FALSE,
                    statusMessage = "No files uploaded."
                });
            }

            string assignmentFolder = Path.Combine(DefaultDirectorySettings.MediaAssignmentMetricsDocuments, model.ReservationsAssignmentsID.ToString() ?? "0");

            // **First loop: Validation check**
            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    return BadRequest(new ResponseModel
                    {
                        status = ResponseStatus.FALSE,
                        statusMessage = "One or more files are empty.",
                    });
                }

                if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new ResponseModel
                    {
                        status = ResponseStatus.FALSE,
                        statusMessage = $"Invalid file type: {file.FileName}. Only JPEG, PNG images are allowed.",
                    });
                }

                if (file.Length > maxFileSize)
                {
                    return BadRequest(new ResponseModel
                    {
                        status = ResponseStatus.FALSE,
                        statusMessage = $"File size exceeds 10MB: {file.FileName}",
                    });
                }
            }
            List<UploadedFileInfo> uploadedFiles = new List<UploadedFileInfo>();

            // **Process, upload, and save each file one by one**
            foreach (var file in files)
            {

                // **Upload the file**
                UploadedFileInfo uploadedFile = await SaveDocumentAsync(file, assignmentFolder);

                // **Save to database immediately after uploading**
                bool isSaved = await _reservationAssignmentsService.SaveAssignmentDocument(model.ReservationsAssignmentsID ?? 0, uploadedFile);

                if (!isSaved)
                {
                    return StatusCode(500, new ResponseModel
                    {
                        status = ResponseStatus.FALSE,
                        statusMessage = $"Failed to save file {uploadedFile.FileName} in the database."
                    });
                }

                uploadedFiles.Add(uploadedFile);
            }

            return Ok(new ResponseModel
            {
                status = ResponseStatus.TRUE,
                statusMessage = "Files uploaded and saved successfully.",
                data = uploadedFiles
            });
        }

        [HttpPost("AssignmentMilesInfo")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMilesInfo([FromBody] AppReservationAssignmentSearchViewModel model)
        {
            var tokenClaims = User.Identity.GetAppClaims();
            model.Type = tokenClaims.Type;
            if (tokenClaims.Type == (int)Type.Contractor)
                model.ContractorID = tokenClaims.UserID;
            else if (tokenClaims.Type == (int)Type.Claimant)
                model.ClaimantID = tokenClaims.UserID;

            var result = await _reservationAssignmentsService.AssignmentMilesRecord(model);

            return Ok(new
            {
                status = ResponseStatus.TRUE,
                statusMessage = ResponseMessage.SUCCESS,
                data = result
            });
        }

    }
}
