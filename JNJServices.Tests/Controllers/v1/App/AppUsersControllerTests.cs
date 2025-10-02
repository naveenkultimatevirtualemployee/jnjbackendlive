using AutoMapper;
using JNJServices.API.Controllers.v1.App;
using JNJServices.Business.Abstracts;
using JNJServices.Business.Email;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.CommonModels;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1.App
{
    public class AppUsersControllerTests
    {

        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IContractorService> _contractorServiceMock;
        private readonly Mock<IClaimantService> _claimantServiceMock;
        private readonly Mock<IJwtFactory> _jwtFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly ApiVersionOptions _apiVersionOptions;
        private readonly ClaimsPrincipal _user;
        private readonly Mock<IOptions<ApiVersionOptions>> _apiVersionOptionsMock;
        private readonly AppUsersController _controller;

        public AppUsersControllerTests()
        {
            // Initialize mocks
            var claims = new List<Claim>
            {
                new Claim("Type", "1"),
                new Claim("UserID", "123"),
                new Claim("PhoneNo", "1234567890"),
                new Claim("UserName", "TestUser")
            };

            _user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"));


            _userServiceMock = new Mock<IUserService>();
            _contractorServiceMock = new Mock<IContractorService>();
            _claimantServiceMock = new Mock<IClaimantService>();
            _jwtFactoryMock = new Mock<IJwtFactory>();
            _mapperMock = new Mock<IMapper>();
            _emailServiceMock = new Mock<IEmailService>();

            var inMemorySettings = new Dictionary<string, string?>
            {
                {"EncryptionDecryption:Key", "testEncryptionKey"},
                {"SelectTimeZone:TimeZone", "India Standard Time"}
            };

            var apiVersionOptions = new ApiVersionOptions
            {
                MinimumApiVersioniOS = "1.0.0",
                MinimumApiVersionAndroid = "1.0.0"
            };
            _apiVersionOptionsMock = new Mock<IOptions<ApiVersionOptions>>();
            _apiVersionOptionsMock.Setup(a => a.Value).Returns(apiVersionOptions);

            var configurationMock = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Initialize ApiVersionOptions if needed
            _apiVersionOptions = new ApiVersionOptions();

            // Create a new instance of the controller with mocked dependencies
            _controller = new AppUsersController(
                configurationMock,
                _userServiceMock.Object,
                _jwtFactoryMock.Object,
                _contractorServiceMock.Object,
                _claimantServiceMock.Object,
                _mapperMock.Object,
                _apiVersionOptionsMock.Object,
                _emailServiceMock.Object
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                    {
                        new Claim("UserID", "123"),
                        new Claim("Type", "Admin")
                    }))
                }
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = _user
                }
            };
        }

        [Fact]
        public async Task LoginUser_ReturnsOkResult_WithValidLogin()
        {
            // Arrange
            var model = new UserLoginAppViewModel
            {
                Type = 1,
                UserID = 123,
                PhoneNo = "1234567890",
                LoginPassword = "password123",
                DeviceID = "device123",
                FcmToken = "fcm123"
            };

            var userDetails = new UserLoginAppResponseModel
            {
                UserID = model.UserID,
                UserName = "TestUser",
                Type = "Regular",
                PhoneNo = model.PhoneNo,
                AuthToken = "some_auth_token",
                IsFcmUpdated = 1,
                isOTPVerified = 1
            };



            // Setup user service and JWT factory
            _userServiceMock.Setup(s => s.VerifyUserLoginDetailsApp(model)).ReturnsAsync((userDetails, "Login successful"));
            _jwtFactoryMock.Setup(f => f.GenerateToken(It.IsAny<List<Claim>>())).Returns("some_auth_token");
            _userServiceMock.Setup(s => s.UpdateUserToken(It.IsAny<string>(), model.UserID, model.Type)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.LoginUser(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal("Login successful", response.statusMessage);
            Assert.Equal(userDetails.UserID, ((UserLoginAppResponseModel)response.data).UserID);
            Assert.Equal(userDetails.UserName, ((UserLoginAppResponseModel)response.data).UserName);
            Assert.Equal(userDetails.Type, ((UserLoginAppResponseModel)response.data).Type);
            Assert.Equal(userDetails.PhoneNo, ((UserLoginAppResponseModel)response.data).PhoneNo);
            Assert.Equal(userDetails.AuthToken, ((UserLoginAppResponseModel)response.data).AuthToken);
            Assert.Equal(userDetails.IsFcmUpdated, ((UserLoginAppResponseModel)response.data).IsFcmUpdated);
            Assert.Equal(userDetails.isOTPVerified, ((UserLoginAppResponseModel)response.data).isOTPVerified);
        }

        [Fact]
        public async Task LoginUser_ReturnsBadRequest_WhenLoginFails()
        {
            // Arrange
            var model = new UserLoginAppViewModel
            {
                Type = 1,
                UserID = 123,
                LoginPassword = "wrongpassword"
            };

            _userServiceMock.Setup(s => s.VerifyUserLoginDetailsApp(model)).ReturnsAsync((null, "Invalid login credentials"));

            // Act
            var result = await _controller.LoginUser(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Invalid login credentials", response.statusMessage);
        }

        [Fact]
        public async Task LoginUser_ReturnsBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            var model = new UserLoginAppViewModel
            {
                Type = 1,
                UserID = 999, // Non-existent user ID
                PhoneNo = "1234567890",
                LoginPassword = "password123"
            };

            _userServiceMock.Setup(s => s.VerifyUserLoginDetailsApp(model)).ReturnsAsync((null, "User does not exist"));

            // Act
            var result = await _controller.LoginUser(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("User does not exist", response.statusMessage);
        }

        [Fact]
        public async Task LoginUser_ReturnsBadRequest_WhenDeviceIDIsMissing()
        {
            // Arrange
            var model = new UserLoginAppViewModel
            {
                Type = 1,
                UserID = 123,
                PhoneNo = "1234567890",
                LoginPassword = "password123",
                // DeviceID is missing
                FcmToken = "fcm123"
            };

            // Act
            var result = await _controller.LoginUser(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Null(response.statusMessage); // Assuming you handle this case
        }

        [Fact]
        public async Task LoginUser_ReturnsBadRequest_WhenFcmTokenIsMissing()
        {
            // Arrange
            var model = new UserLoginAppViewModel
            {
                Type = 1,
                UserID = 123,
                PhoneNo = "1234567890",
                LoginPassword = "password123",
                // DeviceID is missing
                DeviceID = "fcm123"
            };

            // Act
            var result = await _controller.LoginUser(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Null(response.statusMessage); // Assuming you handle this case
        }

        [Fact]
        public async Task ResetPassword_ReturnsOkResult_WithValidData()
        {
            // Arrange
            var model = new AppForgotPasswordViewModel
            {
                Type = 1,
                UserID = 123,
                Password = "newPassword123"
            };

            _userServiceMock.Setup(s => s.UserForgotPasswordApp(model))
                .ReturnsAsync((1, "Password reset successful"));

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal("Password reset successful", response.statusMessage);
            Assert.Equal("Password reset successful", response.data);
        }

        [Fact]
        public async Task ResetPassword_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var model = new AppForgotPasswordViewModel
            {
                Type = null, // Missing required fields
                UserID = null,
                Password = null
            };

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
        }

        [Fact]
        public async Task ResetPassword_ReturnsBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            var model = new AppForgotPasswordViewModel
            {
                Type = 1,
                UserID = 999, // Non-existent user ID
                Password = "newPassword123"
            };

            _userServiceMock.Setup(s => s.UserForgotPasswordApp(model))
                .ReturnsAsync((0, "User not found"));

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("User not found", response.statusMessage);
        }

        [Fact]
        public async Task ResetPassword_ReturnsBadRequest_WhenOperationFails()
        {
            // Arrange
            var model = new AppForgotPasswordViewModel
            {
                Type = 1,
                UserID = 123,
                Password = "newPassword123"
            };

            _userServiceMock.Setup(s => s.UserForgotPasswordApp(model))
                .ReturnsAsync((0, "Failed to reset password"));

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Failed to reset password", response.statusMessage);
        }

        [Fact]
        public async Task VerifyPhoneNumber_ReturnsOkResult_WithValidData()
        {
            // Arrange
            var model = new VerifyPhoneNumberViewModel
            {
                Type = 1,
                UserID = 123,
                PhoneNo = "1234567890"
            };

            _userServiceMock.Setup(s => s.VerifyPhoneNumberApp(model))
                .ReturnsAsync((1, "Phone number verified successfully"));

            // Act
            var result = await _controller.VerifyPhoneNumber(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal("Phone number verified successfully", response.statusMessage);
            Assert.Equal("Phone number verified successfully", response.data);
        }

        [Fact]
        public async Task VerifyPhoneNumber_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var model = new VerifyPhoneNumberViewModel
            {
                Type = 1,
                UserID = null, // Missing UserID
                PhoneNo = null // Missing PhoneNo
            };

            // Act
            var result = await _controller.VerifyPhoneNumber(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
        }

        [Fact]
        public async Task VerifyPhoneNumber_ReturnsBadRequest_WhenVerificationFails()
        {
            // Arrange
            var model = new VerifyPhoneNumberViewModel
            {
                Type = 1,
                UserID = 123,
                PhoneNo = "1234567890"
            };

            _userServiceMock.Setup(s => s.VerifyPhoneNumberApp(model))
                .ReturnsAsync((0, "Phone number verification failed"));

            // Act
            var result = await _controller.VerifyPhoneNumber(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Phone number verification failed", response.statusMessage);
        }

        [Fact]
        public async Task Logout_ReturnsOkResult_WhenLogoutIsSuccessful()
        {
            // Arrange
            _userServiceMock.Setup(s => s.LogoutUserApp(1, 123)).ReturnsAsync(1); // Assuming 1 indicates success

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.LOGOUT_SUCCESSFUL, response.statusMessage);
            Assert.Equal(1, response.data);
        }

        [Fact]
        public async Task Logout_ReturnsBadRequest_WhenLogoutFails()
        {
            // Arrange
            _userServiceMock.Setup(s => s.LogoutUserApp(1, 123)).ReturnsAsync(0); // Assuming 0 indicates failure

            // Act
            var result = await _controller.Logout();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);

            // Assert that statusMessage is an empty string when logout fails
            Assert.Equal(string.Empty, response.statusMessage);
        }

        [Fact]
        public async Task UpdateFcmToken_ReturnsOkResult_WhenUpdateIsSuccessful()
        {
            // Arrange
            var model = new UpdateFcmTokenAppViewModel
            {
                Type = 1,
                UserID = 123,
                DeviceID = "device123",
                FcmToken = "fcm123"
            };

            _userServiceMock.Setup(s => s.UpdateFcmTokenApp(model)).ReturnsAsync(1); // Simulate a successful update

            // Act
            var result = await _controller.UpdateFcmToken(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
        }

        [Fact]
        public async Task UpdateFcmToken_ReturnsBadRequest_WhenUpdateFails()
        {
            // Arrange
            var model = new UpdateFcmTokenAppViewModel
            {
                Type = 1,
                UserID = 123,
                DeviceID = "device123",
                FcmToken = "fcm123"
            };

            _userServiceMock.Setup(s => s.UpdateFcmTokenApp(model)).ReturnsAsync(0); // Simulate a failed update

            // Act
            var result = await _controller.UpdateFcmToken(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.ERROR, response.statusMessage);
        }

        [Fact]
        public async Task UpdateFcmToken_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var model = new UpdateFcmTokenAppViewModel
            {
                // Missing Type and UserID to trigger validation failure
                DeviceID = "device123",
                FcmToken = "fcm123"
            };

            _controller.ModelState.AddModelError("Type", "The Type field is required.");
            _controller.ModelState.AddModelError("UserID", "The UserID field is required.");

            // Act
            var result = await _controller.UpdateFcmToken(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.False(badRequestResult.StatusCode == 200); // Ensuring we have a bad request
        }

        [Fact]
        public async Task UpdateFcmToken_ReturnsBadRequest_WhenModelIsNull()
        {
            // Arrange
            UpdateFcmTokenAppViewModel model = new UpdateFcmTokenAppViewModel();  // Nullable model

            // Act
            var result = await _controller.UpdateFcmToken(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.ERROR, response.statusMessage);
        }

        [Fact]
        public void CheckVersion_ReturnsOk_WhenValidVersionForIOS()
        {
            // Arrange
            var request = new AppVersionViewModel
            {
                Platform = "ios",
                CurrentVersion = "1.0.1"
            };

            // Act
            var result = _controller.CheckVersion(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.True(response.status);
            Assert.Equal("Provided API version 1.0.1 meets the minimum required version 1.0.0 for ios.", response.statusMessage);
        }

        [Fact]
        public void CheckVersion_ReturnsOk_WhenValidVersionForAndroid()
        {
            // Arrange
            var request = new AppVersionViewModel
            {
                Platform = "android",
                CurrentVersion = "1.0.1"
            };

            // Act
            var result = _controller.CheckVersion(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.True(response.status);
            Assert.Equal("Provided API version 1.0.1 meets the minimum required version 1.0.0 for android.", response.statusMessage);
        }

        [Fact]
        public void CheckVersion_ReturnsOk_WhenLowerVersionForIOS()
        {
            // Arrange
            var request = new AppVersionViewModel
            {
                Platform = "ios",
                CurrentVersion = "0.9.9"
            };

            // Act
            var result = _controller.CheckVersion(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.False(response.status);
            Assert.Equal("Provided API version 0.9.9 is lower than the required minimum version 1.0.0 for ios.", response.statusMessage);
        }

        [Fact]
        public void CheckVersion_ReturnsOk_WhenLowerVersionForAndroid()
        {
            // Arrange
            var request = new AppVersionViewModel
            {
                Platform = "android",
                CurrentVersion = "0.9.9"
            };

            // Act
            var result = _controller.CheckVersion(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.False(response.status);
            Assert.Equal("Provided API version 0.9.9 is lower than the required minimum version 1.0.0 for android.", response.statusMessage);
        }

        [Fact]
        public void CheckVersion_ReturnsBadRequest_WhenInvalidPlatform()
        {
            // Arrange
            var request = new AppVersionViewModel
            {
                Platform = "windows", // Invalid platform
                CurrentVersion = "1.0.0"
            };

            // Act
            var result = _controller.CheckVersion(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.False(response.status);
            Assert.Equal("Invalid platform. Use 'ios' or 'android'.", response.statusMessage);
        }

        [Fact]
        public void CheckVersion_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var request = new AppVersionViewModel
            {
                Platform = "", // Invalid
                CurrentVersion = "" // Invalid
            };

            // Act
            _controller.ModelState.AddModelError("Platform", "The Platform field is required.");
            _controller.ModelState.AddModelError("CurrentVersion", "The CurrentVersion field is required.");
            var result = _controller.CheckVersion(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.False(badRequestResult.StatusCode == 200); // Ensuring we have a bad request
        }

        [Fact]
        public async Task UserProfile_Returns_Contractor_Data_When_Found()
        {
            // Arrange
            var contractorSearchResults = new List<vwContractorSearch>
            {
                new vwContractorSearch { ContractorID = 123, FirstName = "John Doe" }
            };

            var contractorList = new List<ContractorSearchAppResponseModel>
            {
                new ContractorSearchAppResponseModel
                {
                    ContractorID = 123,
                    fullName = "John Doe",
                    firstName = "John",
                    lastName = "Doe",
                    Address1 = "123 Main St",
                    Address2 = "Suite 5A",
                    CellPhone = "555-123-4567",
                    Company = "Doe Construction",
                    Gender = "Male",
                    City = "Metropolis",
                    email = "john.doe@example.com",
                    BirthDate = new DateTime(1980, 4, 25),
                    mlAddress1 = "123 Main St, Metropolis",
                    homePhone = "555-987-6543"
                },
                new ContractorSearchAppResponseModel
                {
                    ContractorID = 456,
                    fullName = "Jane Smith",
                    firstName = "Jane",
                    lastName = "Smith",
                    Address1 = "456 Elm St",
                    Address2 = "Apt 10B",
                    CellPhone = "555-765-4321",
                    Company = "Smith Builders",
                    Gender = "Female",
                    City = "Gotham",
                    email = "jane.smith@example.com",
                    BirthDate = new DateTime(1985, 7, 15),
                    mlAddress1 = "456 Elm St, Gotham",
                    homePhone = "555-321-6549"
                }
            };

            _contractorServiceMock
                .Setup(s => s.ContractorSearch(It.IsAny<ContractorSearchViewModel>()))
                .ReturnsAsync(contractorSearchResults); // Ensure this matches the actual service return type

            _mapperMock
                .Setup(m => m.Map<List<ContractorSearchAppResponseModel>>(contractorSearchResults))
                .Returns(contractorList); // Map from vwContractorSearch to ContractorSearchAppResponseModel

            // Act
            var result = await _controller.UserProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.True(response.status);
        }

        [Fact]
        public async Task UserProfile_Returns_Claimant_Data_When_Found()
        {
            // Arrange: Set user claims to Claimant type
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
            new Claim("Type", "2"),  // Claimant type
            new Claim("UserID", "456")
                }, "TestAuthentication")
            );

            // Mock the service return with vwClaimantSearch type
            var vwClaimantList = new List<vwClaimantSearch>
            {
                new vwClaimantSearch { ClaimantID = 456, FullName = "Jane Doe" }
            };

            // The mapped response model
            var claimantList = new List<ClaimantSearchAppResponseModel>
            {
                new ClaimantSearchAppResponseModel
                {
                    ClaimantID = 456,
                    FullName = "Jane Doe",
                    firstName = "Jane",
                    lastName = "Doe",
                    hmaddress1 = "123 Main St",
                    HmAddress2 = "Apt 4B",
                    hmcity = "Springfield",
                    Gender = "Female",
                    email = "jane.doe@example.com",
                    BirthDate = new DateTime(1985, 5, 15),
                    HmPhone = "123-456-7890",
                    homePhone = "123-456-7890",
                    CellPhone = "987-654-3210"
                },
                new ClaimantSearchAppResponseModel
                {
                    ClaimantID = 789,
                    FullName = "John Smith",
                    firstName = "John",
                    lastName = "Smith",
                    hmaddress1 = "456 Elm St",
                    HmAddress2 = "Suite 12",
                    hmcity = "Metropolis",
                    Gender = "Male",
                    email = "john.smith@example.com",
                    BirthDate = new DateTime(1990, 8, 20),
                    HmPhone = "555-123-4567",
                    homePhone = "555-123-4567",
                    CellPhone = "555-765-4321"
                }
            };

            _claimantServiceMock
                .Setup(s => s.ClaimantSearch(It.IsAny<ClaimantSearchViewModel>()))
                .ReturnsAsync(vwClaimantList);  // Ensure the correct return type

            _mapperMock
                .Setup(m => m.Map<List<ClaimantSearchAppResponseModel>>(vwClaimantList))
                .Returns(claimantList);  // Map vwClaimantSearch to ClaimantSearchAppResponseModel

            // Act
            var result = await _controller.UserProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.True(response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
        }

        [Fact]
        public async Task UserProfile_Returns_NotFound_When_No_Data()
        {
            // Arrange: Ensure the correct return type (e.g., IEnumerable<vwContractorSearch>)
            var emptyContractorList = Enumerable.Empty<vwContractorSearch>();

            _contractorServiceMock
                .Setup(s => s.ContractorSearch(It.IsAny<ContractorSearchViewModel>()))
                .ReturnsAsync(emptyContractorList);  // Correct return type

            // Mock the mapper to return an empty list of ContractorSearchAppResponseModel
            _mapperMock
                .Setup(m => m.Map<List<ContractorSearchAppResponseModel>>(emptyContractorList))
                .Returns(new List<ContractorSearchAppResponseModel>());

            // Act
            var result = await _controller.UserProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.False(response.status);  // Expecting status to be false
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);  // Verify correct message
        }

        [Fact]
        public async Task UserProfile_Returns_BadRequest_When_Invalid_User_Type()
        {
            // Arrange: Set user claims to an invalid type
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
                new Claim("Type", "3"),  // Invalid type
                new Claim("UserID", "789")
                }, "TestAuthentication")
            );

            // Act
            var result = await _controller.UserProfile();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task RaiseTicket_Returns_Ok_When_Email_Sent_Successfully()
        {
            // Arrange
            var model = new RaiseTicketsViewModel
            {
                UserName = "John Doe",
                EmailID = "johndoe@example.com",
                EmailBody = "This is a test ticket."
            };

            var tokenDetails = new AppTokenDetails
            {
                UserID = 1,
                UserName = "Admin"
            };

            var emailResponse = new EmailWebResponseModel
            {
                EmailResponse = ResponseMessage.EMAILSEND
            };

            _emailServiceMock
                .Setup(s => s.RaiseTicketApp(It.IsAny<RaiseTicketsViewModel>(), It.IsAny<AppTokenDetails>()))
                .ReturnsAsync(emailResponse);

            // Act
            var result = await _controller.RaiseTicket(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.True(response.status);
            Assert.Equal(ResponseMessage.EMAILSEND, response.statusMessage);
        }

        [Fact]
        public async Task RaiseTicket_Returns_BadRequest_When_Email_Fails()
        {
            // Arrange
            var model = new RaiseTicketsViewModel
            {
                UserName = "John Doe",
                EmailID = "johndoe@example.com",
                EmailBody = "This is a test ticket."
            };

            var tokenDetails = new AppTokenDetails
            {
                UserID = 1,
                UserName = "Admin"
            };

            var emailResponse = new EmailWebResponseModel
            {
                EmailResponse = ResponseMessage.EMAILNOTSEND
            };

            _emailServiceMock
                .Setup(s => s.RaiseTicketApp(It.IsAny<RaiseTicketsViewModel>(), It.IsAny<AppTokenDetails>()))
                .ReturnsAsync(emailResponse);

            // Act
            var result = await _controller.RaiseTicket(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.EMAILNOTSEND, response.statusMessage);
        }
    }
}
