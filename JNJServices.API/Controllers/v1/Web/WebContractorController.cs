using JNJServices.API.Helper;
using JNJServices.Business.Abstracts;
using JNJServices.Business.Email;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.ApiConstants;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.Web
{
    // [Route("api/web/contractor")]
    //[Route("api/v{version:apiVersion}/web/contractor")]
    [Route("api")]
    [ApiController]
    [EnableCors("AllowSpecificOrigins")]
    public class WebContractorController : BaseController
    {
        private readonly IContractorService _contractorService;
        private readonly NotificationHelper _notificationHelper;
        private readonly IEmailService _emailService;
        public WebContractorController(IContractorService contractorService, NotificationHelper notificationHelper, IEmailService emailService)
        {
            _contractorService = contractorService;
            _notificationHelper = notificationHelper;
            _emailService = emailService;
        }

        [HttpGet("ContractorService")]
        public async Task<IActionResult> contractorService()
        {
            ResponseModel response = new ResponseModel();
            var result = await _contractorService.GetContractorService();
            if (result == null || !result.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
            }
            return Ok(response);
        }

        [HttpGet("ContractorStatus")]
        public async Task<IActionResult> contractorstatus()
        {
            ResponseModel response = new ResponseModel();
            var result = await _contractorService.GetContractorStatus();
            if (result == null || !result.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
            }

            return Ok(response);
        }

        [HttpPost("ContractorSearch")]
        public async Task<IActionResult> ContractorSearch([FromBody] ContractorSearchViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();
            if ((model.ZipCode != 0 && model.ZipCode != null) && (model.Miles == null))
            {

                response.status = ResponseStatus.FALSE;
                response.statusMessage = "Miles Required";
                return BadRequest(response);

            }
            if ((model.Miles != 0 && model.Miles != null) && (model.ZipCode == 0 || model.ZipCode == null))
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = "Zipcode Required";
                return BadRequest(response);
            }
            var result = await _contractorService.ContractorSearch(model);

            if (result == null || !result.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
                response.totalData = result.Select(s => s.TotalCount).First();
            }
            return Ok(response);
        }
        [HttpPost("ContServiceLocation")]
        public async Task<IActionResult> ContractorServiceLocation([FromBody] ContractorIDWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _contractorService.ContractorServiceLoc(model);
            if (result == null || !result.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
            }

            return Ok(response);
        }

        [HttpPost("ResConAvlSearch")]
        public async Task<IActionResult> ResConAvlSearch([FromBody] ContractorAvailaleSearchWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = new List<ContractorAvailableSearchWebResponseModel>();

            if (model.unfilteredSearch is true)
            {
                result = (List<ContractorAvailableSearchWebResponseModel>)await _contractorService.UnfilteredContractorSearch(model);
            }
            else
            {
                result = (List<ContractorAvailableSearchWebResponseModel>)await _contractorService.ContractorAvlSearch(model);

            }
            if (!result.Any())
            {
                response.status = ResponseStatus.FALSE;
                if (model.ContractorID is null || model.ContractorID == 0)
                {
                    response.statusMessage = ResponseMessage.CONTRACTOR_NOT_FOUND;
                }
                else
                {
                    response.statusMessage = ResponseMessage.CONTRACTOR_NOT_AVAILABLE;
                }
                response.data = result;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
            }

            return Ok(response);
        }

        [HttpPost("ResApprovedContractorSearch")]
        public async Task<IActionResult> ResApprovedContractorSearch([FromBody] ContractorAvailaleSearchWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _contractorService.ApprovedContractorAvailableSearch(model);
            if (!result.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
            }

            return Ok(response);
        }

        [HttpPost("ContractorRates")]
        public async Task<IActionResult> ContractorRates([FromBody] ContractorRatesSearchViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _contractorService.ContractorRates(model);
            if (!result.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
            }

            return Ok(response);
        }

        [HttpPost("ContractorRateDetails")]
        public async Task<IActionResult> ContractorRateDetails([FromBody] ContractorRatesDetailSearchViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _contractorService.ContractorRateDetails(model);
            if (!result.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
            }

            return Ok(response);
        }

        [HttpPost("ContractorAvailableHours")]
        public async Task<IActionResult> ContractorAvailableHours([FromBody] ContractorIDWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _contractorService.ContractorAvailablehours(model);

            if (!result.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
            }
            return Ok(response);
        }

        [HttpPost("ContractorLanguage")]
        public async Task<IActionResult> ContractorLanguage([FromBody] ContractorIDWebViewModel model)
        {
            ResponseModel response = new ResponseModel();


            var result = await _contractorService.ContractorLang(model);
            if (!result.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
            }

            return Ok(response);
        }

        [HttpPost("ContractorVehicle")]
        public async Task<IActionResult> ContractorVehicle([FromBody] ContractorVehicleSearchWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _contractorService.ContractorVehicle(model);
            if (!result.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
            }

            return Ok(response);
        }

        [HttpPost("ContractorDriver")]
        public async Task<IActionResult> ContractorDriver([FromBody] ContractorDriverSearchViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _contractorService.ContractorDriver(model);
            if (!result.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
            }

            return Ok(response);
        }

        [HttpPost("ContractorSelDetail")]
        public async Task<IActionResult> ContractorSelDetail([FromBody] ContractorIDWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _contractorService.ContractorSelectiveDetails(model);
            if (!result.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
            }
            return Ok(response);
        }

        [HttpPost("ContractorAssignmentJobStatus")]
        public async Task<IActionResult> ContractorAssignmentJobStatus([FromBody] AssignmentIDWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var contractors = await _contractorService.ContractorAssignmentJobStatus(model);
            if (!contractors.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = contractors;
            }

            return Ok(response);
        }

        [HttpPost("AllContractor")]
        public async Task<IActionResult> AllContractor(ContractorDynamicWebViewModel dynamicSearch)
        {
            ResponseModel response = new ResponseModel();
            var customer = await _contractorService.AllContractor(dynamicSearch);
            if (customer == null || !customer.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = customer;
            }
            return Ok(response);
        }

        [HttpPost("ContractorWebJobSearch")]
        public async Task<ActionResult> ContractorWebJobSearch([FromBody] ContractorJobSearchWebViewModel jobSearch)
        {
            ResponseModel response = new ResponseModel();

            var preferredContractor = await _contractorService.preferredContractorSearch(jobSearch);

            if (preferredContractor.Item1 == 2)
            {
                Task.Run(async () =>
               {
                   await _emailService.PreferredContractorNotMatchedMail(jobSearch.ReservationID.HasValue ? jobSearch.ReservationID.Value : 0, preferredContractor.Item3);
               });

                await _notificationHelper.SendPreferredContractorNotFound(jobSearch.ReservationAssignmentsID.HasValue ? jobSearch.ReservationAssignmentsID.Value : 0);
                response.data = new { isNotificationSend = 1 };
            }
            else
            {

                var result = await _contractorService.ContractorWebJobSearch(jobSearch);
                if (result.Item1 == 1)
                {
                    await _notificationHelper.SendContractorWebJobSearch(jobSearch.ReservationAssignmentsID.HasValue ? jobSearch.ReservationAssignmentsID.Value : 0);
                }
                response.data = new { isNotificationSend = 0 };
            }

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;

            return Ok(response);
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("ContractorMediaUpload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> ContractorMediaUpload([FromForm] ContractorMediaViewModel media, [FromForm] List<IFormFile> file)
        {
            if (file == null || file.Count == 0)
            {
                return BadRequest(new ResponseModel
                {
                    status = ResponseStatus.FALSE,
                    statusMessage = "File image must be provided.",
                });
            }

            var uploadedFile = file.FirstOrDefault();
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest(new ResponseModel
                {
                    status = ResponseStatus.FALSE,
                    statusMessage = "Invalid file.",
                });
            }

            // Allowed file types
            var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedContentTypes.Contains(uploadedFile.ContentType.ToLower()))
            {
                return BadRequest(new ResponseModel
                {
                    status = ResponseStatus.FALSE,
                    statusMessage = "Invalid file type. Only JPEG, PNG, and GIF images are allowed.",
                });
            }

            // Check file size (max 1MB)
            long maxFileSize = 1 * 1024 * 1024; // 1MB
            if (uploadedFile.Length > maxFileSize)
            {
                return BadRequest(new ResponseModel
                {
                    status = ResponseStatus.FALSE,
                    statusMessage = "File size exceeds 1MB limit.",
                });
            }
            // Use ContractorID as a folder inside the base directory
            string contractorFolder = Path.Combine(DefaultDirectorySettings.MediaContractorProfileImages, media.ContractorID.ToString());

            // Handle file upload
            media.ProfileImageUrl = await SaveImageAsync(uploadedFile, contractorFolder);
            media.ProfileImageBase64 = await ConvertToBase64Async(uploadedFile);

            var result = await _contractorService.UpsertContractorMedia(media);

            return Ok(new ResponseModel
            {
                status = result.ResponseCode == 1 ? ResponseStatus.TRUE : ResponseStatus.FALSE,
                statusMessage = result.Msg,
                data = new ContractorMediaResponseModel { ContractorId = media.ContractorID, ContractorProfileImage = media.ProfileImageUrl }
            });
        }

        [HttpPost("GetContractorMedia")]
        public async Task<IActionResult> contractorMedia([FromBody] ContractorIDWebViewModel model)
        {
            ResponseModel response = new ResponseModel();
            var result = await _contractorService.GetContractorMediaAsync(model.ContractorID ?? 0);

            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;
            response.data = result;

            return Ok(response);
        }

        [HttpPost("ContractorListSearch")]
        public async Task<IActionResult> ContractorListSearch([FromBody] ContractorSearchViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();
            if ((model.ZipCode != 0 && model.ZipCode != null) && (model.Miles == null))
            {

                response.status = ResponseStatus.FALSE;
                response.statusMessage = "Miles Required";
                return BadRequest(response);

            }
            if ((model.Miles != 0 && model.Miles != null) && (model.ZipCode == 0 || model.ZipCode == null))
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = "Zipcode Required";
                return BadRequest(response);
            }
            var result = await _contractorService.ContractorListSearch(model);

            if (result == null || !result.Any())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
                response.data = result;
                response.totalData = result.Select(s => s.TotalCount).First();
            }
            return Ok(response);
        }
    }
}
