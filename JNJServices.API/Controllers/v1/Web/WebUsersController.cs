using AutoMapper;
using JNJServices.Business.Abstracts;
using JNJServices.Business.Email;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Helper;
using JNJServices.Utility.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Controllers.v1.Web
{
    //[Route("api/v{version:apiVersion}/web/users")]
    //[Route("api/web/users")]
    [Route("api")]
    [EnableCors("AllowSpecificOrigins")]
    [ApiController]
    public class WebUsersController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IJwtFactory _jwtFactory;
        private readonly IMapper _mapper;
        private readonly TimeZoneConverter _timeZoneConverter;
        private readonly IEmailService _emailService;
        private readonly string _encryptionKey;

        public WebUsersController(IConfiguration configuration, IUserService userService, IJwtFactory jwtFactory, IMapper mapper, TimeZoneConverter timeZoneConverter, IEmailService emailService)
        {
            _configuration = configuration;
            _userService = userService;
            _jwtFactory = jwtFactory;
            _mapper = mapper;
            _timeZoneConverter = timeZoneConverter;
            _emailService = emailService;
            _encryptionKey = _configuration.GetValue<string>("EncryptionDecryption:Key") ?? string.Empty;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginWebViewModel model)
        {
            ResponseModel rm = new ResponseModel();
            if (model == null)
            {
                rm.statusMessage = ResponseMessage.DATA_NOT_FOUND;
                return BadRequest(rm);
            }

            if (!string.IsNullOrEmpty(model.Password))
            {
                model.Password = Cryptography.Encrypt(model.Password, _encryptionKey);
            }

            var response = await _userService.VerifyUserLoginDetailsWeb(model);

            if (response != null)
            {
                if (response.inactiveflag != 0)
                {
                    rm.status = ResponseStatus.FALSE;
                    rm.statusMessage = ResponseMessage.USER_INACTIVE;
                    return Ok(rm);
                }
                var allowedGroupNums = new[] { 1, 2, 3, 4 };

                if (!allowedGroupNums.Contains(response.groupnum))
                {
                    rm.status = ResponseStatus.FALSE;
                    rm.statusMessage = ResponseMessage.USER_ROLE_NOT_DEFINED;
                    return Ok(rm);
                }
                var userDetails = response;
                List<Claim> claims = new List<Claim>()
                {
                    new Claim("TokenDevice", "Web"), // Fixed device type
					new Claim("UserID", userDetails.UserID ?? string.Empty), // Ensure a valid string
					new Claim(JwtRegisteredClaimNames.Email, userDetails.email ?? string.Empty), // Avoids null issues
					new Claim(JwtRegisteredClaimNames.Name, userDetails.UserName ?? string.Empty), // Avoids null issues
					new Claim(ClaimTypes.Role, userDetails.GroupID?.ToString() ?? string.Empty), // Ensure Role is a string
					new Claim("SocketUserID", userDetails.UserID ?? string.Empty), // Consistent identifier
					new Claim(ClaimTypes.NameIdentifier, userDetails.UserID ?? string.Empty) // Consistent identifier
				};

                string tokenString = _jwtFactory.GenerateToken(claims);

                if (!string.IsNullOrEmpty(tokenString))
                {
                    await _userService.UpdateUserToken(tokenString, userDetails.UserNum, null);

                    userDetails.AuthToken = tokenString;

                    UserLoginWebResponseModel responseModel = new UserLoginWebResponseModel();

                    if (!string.IsNullOrEmpty(model.UserFcmToken))
                    {
                        responseModel.IsFcmUpdated = 1;
                    }
                    else
                    {
                        responseModel.IsFcmUpdated = 0;
                    }

                    var result = _mapper.Map(userDetails, responseModel);

                    rm.status = ResponseStatus.TRUE;
                    rm.statusMessage = ResponseMessage.SUCCESS;
                    rm.data = result;
                    return Ok(rm);
                }
            }

            rm.status = ResponseStatus.FALSE;
            rm.statusMessage = ResponseMessage.USER_INVALID_CREDENTIALS;
            return Ok(rm);
        }

        [HttpPost("UpdatePassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ForgotPasswordViewModel model)
        {
            ResponseModel response = new ResponseModel();

            if (!string.IsNullOrEmpty(model.oldPassword))
            {
                model.oldPassword = Cryptography.Encrypt(model.oldPassword, _encryptionKey);
            }
            if (!string.IsNullOrEmpty(model.newPassword))
            {
                model.newPassword = Cryptography.Encrypt(model.newPassword, _encryptionKey);
            }

            var result = await _userService.ResetUserPasswordWeb(model);
            if (result.Item1 == 0)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = result.Item2;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = result.Item2;
                response.data = result.Item2;
            }

            return Ok(response);
        }

        [HttpPost("StaffDetails")]
        public async Task<IActionResult> UserDetails([FromBody] UserSearchViewModel model)
        {
            PaginatedResponseModel response = new PaginatedResponseModel();

            var result = await _userService.SearchUsersDetailWeb(model);
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

        [HttpGet("StaffRoles")]
        public async Task<IActionResult> UserRoles()
        {
            ResponseModel response = new ResponseModel();

            var result = await _userService.GetUserRoles();
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

        [HttpPost("ResetClaimantContractorPassword")]
        public async Task<IActionResult> ResetClaimantContractorPassword([FromBody] ContractorClaimantResetPasswordWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            if (!string.IsNullOrEmpty(model.Password))
            {
                model.Password = Cryptography.Encrypt(model.Password, _encryptionKey);
            }
            else
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.PASSWORDNOTFOUND;
                return Ok(response);
            }

            var result = await _userService.ResetContractorClaimantPasswordWeb(model);
            if (result.Item1 == 0)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = result.Item2;
                response.data = result.Item2;
                return Ok(response);
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = result.Item2;
                response.data = result.Item2;
                return Ok(response);
            }
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> LogoutUser([FromBody] LogoutUserWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            var result = await _userService.UserLogoutWeb(model);
            if (result != 2)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.ERROR;
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;
            }

            return Ok(response);
        }

        [HttpPost("webFcmUpdate")]
        public async Task<IActionResult> UpdateFcmToken(FcmUpdateViewModel model)
        {
            ResponseModel response = new ResponseModel();
            var User = await _userService.UpdateFcmTokenWeb(model);
            if (User != 2)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.USER_NOT_UPDATED;
                return BadRequest(response);
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = ResponseMessage.SUCCESS;

                return Ok(response);
            }
        }

        [HttpPost("UsersUpdatePassword")]
        public async Task<IActionResult> UpdateUserLoginPassword([FromBody] UserUpdatePasswordWebViewModel model)
        {
            ResponseModel response = new ResponseModel();

            if (!string.IsNullOrEmpty(model.Password))
            {
                model.Password = Cryptography.Encrypt(model.Password, _encryptionKey);
            }

            var result = await _userService.UpdateUserLoginPasswordWeb(model.UserID, model.Password);
            if (result.Item1 == 0 || result.Item1 == -1)
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = result.Item2;
                response.data = result.Item2;
                return BadRequest(response);
            }
            else
            {
                response.status = ResponseStatus.TRUE;
                response.statusMessage = result.Item2;
                response.data = result.Item2;
                return Ok(response);
            }
        }

        [HttpPost("ForgotpasswordByEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordByEmailWebViewModel mail)
        {
            ResponseModel response = new ResponseModel();
            if (String.IsNullOrEmpty(mail.Email))
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.EMAILNOTFOUND;
                return Ok(response);
            }
            var emailResult = await _userService.GetActiveUserDetailsByEmail(mail);
            if (emailResult != null && !string.IsNullOrEmpty(emailResult.email))
            {

                var result = await _emailService.SendPasswordResetEmailWeb(emailResult);

                if (result == null || result.EmailResponse != ResponseMessage.EMAILSEND)
                {
                    response.status = ResponseStatus.FALSE;
                    response.statusMessage = ResponseMessage.EMAILNOTSEND;
                }
                else
                {
                    response.status = ResponseStatus.TRUE;
                    response.statusMessage = ResponseMessage.EMAILSEND;
                    response.data = result;
                }
            }
            else
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.EMAILNOTFOUND;
                return Ok(response);
            }
            return Ok(response);
        }

        //Forgot password Update Using Unique ID where in ID => USerNum,Time Interval of 10 minutes is there
        [HttpPost("UpdatePasswordByEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdatePassword([FromBody] MailPasswordChangeViewModel model)
        {
            ResponseModel response = new ResponseModel();

            dynamic data = Cryptography.Decrypt(model.EncryptID, _encryptionKey);

            data = JsonSerializer.Deserialize<ForgotPasswordMailTokenWebViewModel>(data);

            string UserID = data.UserID;
            DateTime dt = data.DateTime;

            model.NewPassword = Cryptography.Encrypt(model.NewPassword, _encryptionKey);

            if (dt < _timeZoneConverter.ConvertUtcToConfiguredTimeZone())
            {
                response.status = ResponseStatus.FALSE;
                response.statusMessage = ResponseMessage.TLREACHED;
                return Ok(response);
            }
            var result1 = await _userService.ValidateTokenAndComparePasswordWeb(UserID, model.NewPassword);
            if (result1 == ResponseMessage.SUCCESS)
            {
                var result = await _userService.UpdateUserLoginPasswordWeb(UserID, model.NewPassword);
                if (result.Item1 != 0)
                {
                    response.status = ResponseStatus.TRUE;
                    response.statusMessage = result.Item2;
                    response.data = result.Item2;
                }
                else
                {
                    response.status = ResponseStatus.FALSE;
                    response.statusMessage = result.Item2;
                }
                return Ok(response);
            }

            response.status = ResponseStatus.FALSE;
            response.statusMessage = result1;

            return Ok(response);
        }
    }
}
