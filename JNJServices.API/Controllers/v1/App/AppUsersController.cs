using Asp.Versioning;
using AutoMapper;
using JNJServices.API.Attributes;
using JNJServices.Business.Abstracts;
using JNJServices.Business.Email;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.CommonModels;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Extensions;
using JNJServices.Utility.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.App
{
    [ApiVersion("1.0")]
    [ValidateAuthorize]
    [Route("api-M")]
    [EnableCors("AllowSpecificOrigins")]
    //[Route("api/v{version:apiVersion}/app/users")]
    //[Route("api/app/users")]
    public class AppUsersController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IContractorService _contractorService;
        private readonly IClaimantService _claimantService;
        private readonly IJwtFactory _jwtFactory;
        private readonly ApiVersionOptions _apiVersionOptions;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public AppUsersController(IConfiguration configuration, IUserService userService, IJwtFactory jwtFactory, IContractorService contractorService, IClaimantService claimantService, IMapper mapper, IOptions<ApiVersionOptions> apiVersionOptions, IEmailService emailService)
        {
            _configuration = configuration;
            _userService = userService;
            _jwtFactory = jwtFactory;
            _contractorService = contractorService;
            _claimantService = claimantService;
            _mapper = mapper;
            _apiVersionOptions = apiVersionOptions.Value;
            _emailService = emailService;
        }

        [AllowAnonymous]
        [HttpPost("MobileLogin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UserLoginAppResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginAppViewModel model)
        {
            ResponseModel rm = new ResponseModel();

            string encryptionKey = _configuration.GetValue<string>("EncryptionDecryption:Key") ?? string.Empty;
            if (!string.IsNullOrEmpty(model.LoginPassword))
            {
                model.LoginPassword = Cryptography.Encrypt(model.LoginPassword, encryptionKey);
            }

            var response = await _userService.VerifyUserLoginDetailsApp(model);
            if (response.Item1 != null)
            {
                var userDetails = response.Item1;
                List<Claim> claims = new List<Claim>()
                {
                    new Claim("TokenDevice", "App"), // Fixed device type
					new Claim("UserID", userDetails.UserID?.ToString() ?? string.Empty), // Ensures a valid string
					new Claim("UserName", userDetails.UserName ?? string.Empty), // Avoids null issues
					new Claim("PhoneNo", userDetails.PhoneNo ?? string.Empty), // Avoids null issues
					new Claim("Type", userDetails.Type ?? string.Empty), // Ensures Type is not null
					new Claim("SocketUserID", $"{userDetails.Type}_{userDetails.UserID?.ToString() ?? string.Empty}"), // Generates a unique identifier
                    new Claim(ClaimTypes.NameIdentifier, $"{userDetails.Type}_{userDetails.UserID?.ToString() ?? string.Empty}") // Generates a unique identifier
				};

                string tokenString = _jwtFactory.GenerateToken(claims);

                if (!string.IsNullOrEmpty(tokenString))
                {
                    await _userService.UpdateUserToken(tokenString, model.UserID, model.Type);

                    userDetails.AuthToken = tokenString;

                    if (!string.IsNullOrEmpty(model.FcmToken))
                    {
                        userDetails.IsFcmUpdated = 1;
                    }
                    else
                    {
                        userDetails.IsFcmUpdated = 0;
                    }

                    rm.status = ResponseStatus.TRUE;
                    rm.statusMessage = response.Item2;
                    rm.data = userDetails;
                    return Ok(rm);
                }
            }

            rm.status = ResponseStatus.FALSE;
            rm.statusMessage = response.Item2;
            return BadRequest(rm);
        }

        [AllowAnonymous]
        [HttpPost("MobileResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] AppForgotPasswordViewModel model)
        {
            ResponseModel rm = new ResponseModel();

            string encryptionKey = _configuration.GetValue<string>("EncryptionDecryption:Key") ?? string.Empty;
            if (!string.IsNullOrEmpty(model.Password))
            {
                model.Password = Cryptography.Encrypt(model.Password, encryptionKey);
            }

            var response = await _userService.UserForgotPasswordApp(model);
            if (response.Item1 != 0)
            {
                rm.status = ResponseStatus.TRUE;
                rm.statusMessage = response.Item2;
                rm.data = response.Item2;
                return Ok(rm);
            }

            rm.status = ResponseStatus.FALSE;
            rm.statusMessage = response.Item2;
            return BadRequest(rm);
        }

        [AllowAnonymous]
        [HttpPost("MobileCheckPhoneNo")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(VerifyPhoneNumberResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> VerifyPhoneNumber([FromBody] VerifyPhoneNumberViewModel model)
        {
            ResponseModel rm = new ResponseModel();
            var response = await _userService.VerifyPhoneNumberApp(model);
            if (response.Item1 != 0)
            {
                rm.status = ResponseStatus.TRUE;
                rm.statusMessage = response.Item2;
                rm.data = response.Item2;
                return Ok(rm);
            }
            rm.status = ResponseStatus.FALSE;
            rm.statusMessage = response.Item2;
            return BadRequest(rm);
        }

        [HttpGet("MobileLogout")]
        public async Task<IActionResult> Logout()
        {
            ResponseModel rm = new ResponseModel();
            var tokenClaims = User.Identity.GetAppClaims();

            // Extract token from Authorization header
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            var token = authHeader?.StartsWith("Bearer ") == true
                ? authHeader.Substring("Bearer ".Length).Trim()
                : string.Empty;

            // Extract claims
            var userType = tokenClaims.Type ?? 0;
            var userId = tokenClaims.UserID ?? 0;

            var response = await _userService.LogoutUserApp(tokenClaims.Type.HasValue ? tokenClaims.Type.Value : 0, tokenClaims.UserID.HasValue ? tokenClaims.UserID.Value : 0);
            if (response != 0)
            {
                // Build the same cache key used in ValidateAuthorizeAttribute
                var cacheKey = $"auth_{userType}_{userId}_{token}";

                // Resolve IMemoryCache and remove the entry
                var cache = HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                cache.Remove(cacheKey);
                rm.status = ResponseStatus.TRUE;
                rm.statusMessage = ResponseMessage.LOGOUT_SUCCESSFUL;
                rm.data = response;
                return Ok(rm);
            }
            rm.status = ResponseStatus.FALSE;
            rm.statusMessage = string.Empty;
            return BadRequest(rm);
        }

        [HttpPost("MobileFcmUpdate")]
        public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenAppViewModel model)
        {
            ResponseModel rm = new ResponseModel();
            var response = await _userService.UpdateFcmTokenApp(model);
            if (response != 0)
            {
                rm.status = ResponseStatus.TRUE;
                rm.statusMessage = ResponseMessage.SUCCESS;
                return Ok(rm);
            }
            rm.status = ResponseStatus.FALSE;
            rm.statusMessage = ResponseMessage.ERROR;
            return BadRequest(rm);

        }

        [HttpPost("CheckVersion")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseModel), StatusCodes.Status200OK)]
        public IActionResult CheckVersion([FromBody] AppVersionViewModel request)
        {
            var minimumVersion = request.Platform.ToLower() switch
            {
                "ios" => new Version(_apiVersionOptions.MinimumApiVersioniOS),
                "android" => new Version(_apiVersionOptions.MinimumApiVersionAndroid),
                _ => null
            };

            if (minimumVersion is null)
            {
                return BadRequest(new ResponseModel { status = false, statusMessage = "Invalid platform. Use 'ios' or 'android'." });
            }

            var providedVersion = new Version(request.CurrentVersion);

            return Ok(new ResponseModel
            {
                status = providedVersion >= minimumVersion,
                statusMessage = providedVersion >= minimumVersion
                    ? $"Provided API version {request.CurrentVersion} meets the minimum required version {minimumVersion} for {request.Platform}."
                    : $"Provided API version {request.CurrentVersion} is lower than the required minimum version {minimumVersion} for {request.Platform}.",
            });
        }

        [HttpGet("UserProfile")]
        public async Task<IActionResult> UserProfile()
        {
            ResponseModel response = new ResponseModel();
            var tokenClaims = User.Identity.GetAppClaims();
            if (tokenClaims.Type == (int)Utility.Enums.Type.Contractor)
            {
                ContractorSearchViewModel contractorSearchViewModel = new ContractorSearchViewModel();
                contractorSearchViewModel.ContractorID = tokenClaims.UserID;
                contractorSearchViewModel.inactiveflag = 0;

                var contractors = await _contractorService.ContractorSearch(contractorSearchViewModel);
                List<ContractorSearchAppResponseModel> responseModel = new List<ContractorSearchAppResponseModel>();

                if (contractors.Any())
                {
                    response.status = ResponseStatus.TRUE;
                    response.statusMessage = ResponseMessage.SUCCESS;
                    response.data = _mapper.Map(contractors.ToList(), responseModel);
                }
                else
                {
                    response.status = ResponseStatus.FALSE;
                    response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
                }

                return Ok(response);
            }
            else if (tokenClaims.Type == (int)Utility.Enums.Type.Claimant)
            {
                ClaimantSearchViewModel claimantSearchViewModel = new ClaimantSearchViewModel();
                claimantSearchViewModel.ClaimantID = tokenClaims.UserID;
                claimantSearchViewModel.Inactive = 0;

                var claimantsList = await _claimantService.ClaimantSearch(claimantSearchViewModel);

                List<ClaimantSearchAppResponseModel> responseModel = new List<ClaimantSearchAppResponseModel>();

                if (claimantsList.Any())
                {
                    response.status = ResponseStatus.TRUE;
                    response.statusMessage = ResponseMessage.SUCCESS;
                    response.data = _mapper.Map(claimantsList.ToList(), responseModel);
                }
                else
                {
                    response.status = ResponseStatus.FALSE;
                    response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
                }
                return Ok(response);
            }
            else
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.DATA_NOT_FOUND;
                return BadRequest(response);
            }
        }

        [HttpPost("RaiseTicket")]
        public async Task<IActionResult> RaiseTicket([FromBody] RaiseTicketsViewModel model)
        {
            ResponseModel response = new ResponseModel();
            var tokenClaims = User.Identity.GetAppClaims();

            var result = await _emailService.RaiseTicketApp(model, tokenClaims);

            if (result.EmailResponse != ResponseMessage.EMAILSEND)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.EMAILNOTSEND;
                return BadRequest(response);
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.EMAILSEND;
                return Ok(response);
            }
        }

        [HttpPost("MobileNotificationAndFcmUpdate")]
        public async Task<IActionResult> UpdateFcmTokenAndNotification([FromBody] TokenAndNotificationUpdateRequest model)
        {
            ResponseModel rm = new ResponseModel();
            var tokenClaims = User.Identity.GetAppClaims();
            model.UserId = tokenClaims.UserID ?? 0;
            model.Type = tokenClaims.Type ?? 0;
            var response = await _userService.UpdateNotificationAndToken(model);

            if (response.Item1 != 0)
            {
                rm.status = ResponseStatus.TRUE;
                rm.statusMessage = response.Item2;
                return Ok(rm);
            }

            rm.status = ResponseStatus.FALSE;
            rm.statusMessage = response.Item2;
            return BadRequest(rm);
        }

        [HttpGet("MobileGetNotificationStatus")]
        public async Task<IActionResult> GetNotificationStatus()
        {
            NotificationAppViewModel model = new NotificationAppViewModel();
            ResponseModel rm = new ResponseModel();
            var tokenClaims = User.Identity.GetAppClaims();
            model.UserId = tokenClaims.UserID ?? 0;
            model.Type = tokenClaims.Type ?? 0;
            var response = await _userService.GetNotificationStatusAsync(model.UserId, model.Type);

            if (response.Item1 != 0)
            {
                rm.status = ResponseStatus.TRUE;
                rm.statusMessage = response.Item2;
                rm.data = new NotificationResponseModel
                {
                    NotificationStatus = response.Item3,
                };
                return Ok(rm);
            }

            rm.status = ResponseStatus.FALSE;
            rm.statusMessage = response.Item2;
            return BadRequest(rm);
        }
    }

}
