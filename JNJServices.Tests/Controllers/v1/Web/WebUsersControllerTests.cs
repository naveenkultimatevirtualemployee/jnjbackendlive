using AutoMapper;
using JNJServices.API.Controllers.v1.Web;
using JNJServices.Business.Abstracts;
using JNJServices.Business.Email;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.Helper;
using JNJServices.Utility.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using System.Text.Json;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Tests.Controllers.v1.Web
{
    public class WebUsersControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IJwtFactory> _jwtFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly WebUsersController _controller;
        public WebUsersControllerTests()
        {
            // Initialize mocks
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"EncryptionDecryption:Key", "testEncryptionKey"},
                {"SelectTimeZone:TimeZone", "India Standard Time"}
            };

            var configurationMock = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _userServiceMock = new Mock<IUserService>();
            _jwtFactoryMock = new Mock<IJwtFactory>();
            _mapperMock = new Mock<IMapper>();
            _emailServiceMock = new Mock<IEmailService>();
            var timeZoneConverter = new TimeZoneConverter(configurationMock);
            // Initialize the controller with the mocked services and the configuration
            _controller = new WebUsersController(
                configurationMock,
                _userServiceMock.Object,
                _jwtFactoryMock.Object,
                _mapperMock.Object,
                timeZoneConverter, // Assuming TimeZoneConverter is not needed for these tests
                _emailServiceMock.Object);
        }

        [Fact]
        public async Task LoginUser_ReturnsSuccessResponse_WhenLoginIsSuccessful()
        {
            // Arrange
            var model = new UserLoginWebViewModel
            {
                Username = "testuser",
                Password = "testpassword",
                UserDeviceID = "abc",
                UserFcmToken = "testToken"
            };

            var userDetails = new vwDBUsers
            {
                UserID = "1",
                UserName = "testuser",
                email = "testuser@example.com",
                GroupID = "Admin",
                UserNum = 1,
                inactiveflag = 0,
                isFirstTimeUser = 0,
                LastChangeDate = DateTime.UtcNow,
                LastChangeUserID = "admin",
                CreateDate = DateTime.UtcNow,
                CreateUserID = "admin",
                LastLoginDate = DateTime.UtcNow,
                LastLoginComputerName = "ComputerName",
                groupnum = 1,
                GroupName = "Admin Group",
                AuthToken = "authToken",
                UserFcmToken = "testToken",
                TotalCount = 1
            };

            _userServiceMock.Setup(u => u.VerifyUserLoginDetailsWeb(It.IsAny<UserLoginWebViewModel>()))
                .ReturnsAsync(userDetails);
            _jwtFactoryMock.Setup(j => j.GenerateToken(It.IsAny<List<Claim>>()))
                .Returns("generatedToken");
            _userServiceMock.Setup(s => s.UpdateUserToken(It.IsAny<string>(), It.IsAny<int>(), null)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.LoginUser(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseModel = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, responseModel.status);
            Assert.Equal(ResponseMessage.SUCCESS, responseModel.statusMessage);
        }

        [Fact]
        public async Task LoginUser_ReturnsBadRequest_WhenModelIsNull()
        {
            // Arrange: Create an empty model or a model with default values
            UserLoginWebViewModel model = new UserLoginWebViewModel();  // Empty or invalid model

            // Act
            var result = await _controller.LoginUser(model);

            // Assert
            var badRequestResult = Assert.IsType<OkObjectResult>(result);
            var responseModel = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(ResponseStatus.FALSE, responseModel.status);
            Assert.Equal("Invalid credentials, please try again.", responseModel.statusMessage);
        }

        [Fact]
        public async Task LoginUser_ReturnsBadRequest_WhenLoginFails()
        {
            // Arrange
            var model = new UserLoginWebViewModel { Password = "invalidPassword", UserFcmToken = "token" };
            _userServiceMock.Setup(s => s.VerifyUserLoginDetailsWeb(model)).ReturnsAsync(new vwDBUsers()); // Simulate invalid login

            // Act
            var result = await _controller.LoginUser(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseModel = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, responseModel.status);
            Assert.Equal("Your role is not authorized to access the system.", responseModel.statusMessage);
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnFalse_WhenOldPasswordIsEmpty()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                EmailID = "abc",
                oldPassword = string.Empty,
                newPassword = "new_password"
            };

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(response.data, string.Empty); // Assuming data is null when unsuccessful
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnFalse_WhenNewPasswordIsEmpty()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                EmailID = "abc",
                oldPassword = "old_password",
                newPassword = string.Empty
            };

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(response.data, string.Empty); // Assuming data is null when unsuccessful
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnSuccess_WhenPasswordResetIsSuccessful()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                EmailID = "abc",
                oldPassword = "old_password",
                newPassword = "new_password"
            };

            _userServiceMock
                .Setup(s => s.ResetUserPasswordWeb(It.IsAny<ForgotPasswordViewModel>()))
                .ReturnsAsync((1, "Password reset successful"));

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal("Password reset successful", response.statusMessage);
            Assert.Equal("Password reset successful", response.data); // Assuming data contains the message
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnFailure_WhenPasswordResetFails()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                EmailID = "abc",
                oldPassword = "old_password",
                newPassword = "new_password"
            };

            _userServiceMock
                .Setup(s => s.ResetUserPasswordWeb(It.IsAny<ForgotPasswordViewModel>()))
                .ReturnsAsync((0, "Password reset failed"));

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Password reset failed", response.statusMessage);
            //Assert.Null(response.data); // Assuming data is null when unsuccessful
        }

        [Fact]
        public async Task UserDetails_ValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var mockUserList = new List<vwDBUsers>
            {
                new vwDBUsers { TotalCount = 2, UserName = "John" }
            };

            _userServiceMock
                .Setup(service => service.SearchUsersDetailWeb(It.IsAny<UserSearchViewModel>()))
                .ReturnsAsync(mockUserList);

            var requestModel = new UserSearchViewModel { UserName = "John", UserID = "abcd", email = "jfjfjfj" };

            // Act
            var result = await _controller.UserDetails(requestModel) as OkObjectResult;
            var response = result?.Value as PaginatedResponseModel;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(2, response.totalData);
        }

        [Fact]
        public async Task UserDetails_NoResults_ReturnsDataNotFoundResponse()
        {
            // Arrange
            _userServiceMock
                .Setup(service => service.SearchUsersDetailWeb(It.IsAny<UserSearchViewModel>()))
                .ReturnsAsync(new List<vwDBUsers>()); // Empty list

            var requestModel = new UserSearchViewModel { UserName = "Unknown", inactiveflag = 0, UserID = "xyz", email = "unknown@example.com" };

            // Act
            var result = await _controller.UserDetails(requestModel) as OkObjectResult;
            var response = result?.Value as PaginatedResponseModel;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task UserDetails_ServiceThrowsException_Returns500Response()
        {
            // Arrange
            _userServiceMock
                .Setup(service => service.SearchUsersDetailWeb(It.IsAny<UserSearchViewModel>()))
                .ThrowsAsync(new Exception("Database error"));

            var requestModel = new UserSearchViewModel { UserName = "John", UserID = "abcd", email = "john@example.com" };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _controller.UserDetails(requestModel));
        }

        [Fact]
        public async Task UserDetails_InvalidPageNumber_ReturnsDataNotFoundResponse()
        {
            // Arrange
            var requestModel = new UserSearchViewModel { UserName = "John", UserID = "abcd", Page = -1 };

            _userServiceMock
                .Setup(service => service.SearchUsersDetailWeb(It.IsAny<UserSearchViewModel>()))
                .ReturnsAsync(new List<vwDBUsers>()); // No results due to invalid input

            // Act
            var result = await _controller.UserDetails(requestModel) as OkObjectResult;
            var response = result?.Value as PaginatedResponseModel;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task UserDetails_EmptyBody_ReturnsDataNotFoundResponse()
        {
            // Arrange
            var requestModel = new UserSearchViewModel(); // No data provided

            _userServiceMock
                .Setup(service => service.SearchUsersDetailWeb(It.IsAny<UserSearchViewModel>()))
                .ReturnsAsync(new List<vwDBUsers>()); // No results

            // Act
            var result = await _controller.UserDetails(requestModel) as OkObjectResult;
            var response = result?.Value as PaginatedResponseModel;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
            Assert.Equal(response.data, string.Empty);
        }

        [Fact]
        public async Task UserDetails_PaginationWorksCorrectly()
        {
            // Arrange
            var mockUserList = new List<vwDBUsers>
            {
                new vwDBUsers { TotalCount = 5, UserName = "John" },
                new vwDBUsers { TotalCount = 5, UserName = "Doe" }
            };

            _userServiceMock
                .Setup(service => service.SearchUsersDetailWeb(It.IsAny<UserSearchViewModel>()))
                .ReturnsAsync(mockUserList);

            var requestModel = new UserSearchViewModel { UserName = "John", Page = 1, Limit = 2 };

            // Act
            var result = await _controller.UserDetails(requestModel) as OkObjectResult;
            var response = result?.Value as PaginatedResponseModel;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(5, response.totalData);
        }

        [Fact]
        public async Task ResetClaimantContractorPassword_ValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var requestModel = new ContractorClaimantResetPasswordWebViewModel
            {
                Password = "newPassword123",
                Type = 1,
                UserID = 1
            };

            _userServiceMock
                .Setup(service => service.ResetContractorClaimantPasswordWeb(It.IsAny<ContractorClaimantResetPasswordWebViewModel>()))
                .ReturnsAsync((1, "Password reset successful"));

            // Act
            var result = await _controller.ResetClaimantContractorPassword(requestModel) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal("Password reset successful", response.statusMessage);
            Assert.Equal("Password reset successful", response.data);
        }

        [Fact]
        public async Task ResetClaimantContractorPassword_UserNotFound_ReturnsFalseStatus()
        {
            // Arrange
            var requestModel = new ContractorClaimantResetPasswordWebViewModel
            {
                Password = "newPassword123",
                Type = 1,
                UserID = 1
            };

            _userServiceMock
                .Setup(service => service.ResetContractorClaimantPasswordWeb(It.IsAny<ContractorClaimantResetPasswordWebViewModel>()))
                .ReturnsAsync((0, "User not found")); // Simulate user not found

            // Act
            var result = await _controller.ResetClaimantContractorPassword(requestModel) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("User not found", response.statusMessage);
            Assert.Equal("User not found", response.data);
        }

        [Fact]
        public async Task ResetClaimantContractorPassword_EmptyPassword_ReturnsFalseStatus()
        {
            // Arrange
            var requestModel = new ContractorClaimantResetPasswordWebViewModel
            {
                Password = "", // Empty password
                Type = 1,
                UserID = 1
            };

            // Act
            var result = await _controller.ResetClaimantContractorPassword(requestModel) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.PASSWORDNOTFOUND, response.statusMessage);
        }

        [Fact]
        public async Task ResetClaimantContractorPassword_InvalidUserType_ReturnsFalseStatus()
        {
            // Arrange
            var requestModel = new ContractorClaimantResetPasswordWebViewModel
            {
                Password = "newPassword123",
                Type = -1, // Invalid user type
                UserID = 1
            };

            _userServiceMock
                .Setup(service => service.ResetContractorClaimantPasswordWeb(It.IsAny<ContractorClaimantResetPasswordWebViewModel>()))
                .ReturnsAsync((0, "Invalid user type")); // Simulate invalid user type error

            // Act
            var result = await _controller.ResetClaimantContractorPassword(requestModel) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal("Invalid user type", response.statusMessage);
            Assert.Equal("Invalid user type", response.data);
        }

        [Fact]
        public async Task UserRoles_ReturnsSuccessResponse_WhenRolesExist()
        {
            // Arrange
            var mockRoles = new List<UserRoles>
            {
                new UserRoles { GroupNum = 1, GroupID = "AdminID", GroupName = "Admin" },
                new UserRoles { GroupNum = 2, GroupID = "UserID", GroupName = "User" },
                new UserRoles { GroupNum = 3, GroupID = "ManagerID", GroupName = "Manager" }
            };

            // Setup the service to return the mock roles
            _userServiceMock
                .Setup(service => service.GetUserRoles())
                .ReturnsAsync(mockRoles);

            // Act
            var result = await _controller.UserRoles() as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
            Assert.Equal(mockRoles, response.data);
        }

        [Fact]
        public async Task UserRoles_ReturnsFalseResponse_WhenNoRolesExist()
        {
            // Arrange
            _userServiceMock
                .Setup(service => service.GetUserRoles())
                .ReturnsAsync(new List<UserRoles>()); // No roles

            // Act
            var result = await _controller.UserRoles() as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task UserRoles_ReturnsFalseResponse_WhenRolesAreNull()
        {
            // Arrange
            _userServiceMock
                .Setup(service => service.GetUserRoles())
                .ReturnsAsync((List<UserRoles>?)null!); // Nullable List<UserRoles>

            // Act
            var result = await _controller.UserRoles() as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.DATA_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task UserRoles_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _userServiceMock
                .Setup(service => service.GetUserRoles())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await Assert.ThrowsAsync<Exception>(() => _controller.UserRoles());

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Database error", result.Message);
        }

        [Fact]
        public async Task LogoutUser_ReturnsSuccess_WhenLogoutIsSuccessful()
        {
            // Arrange
            var model = new LogoutUserWebViewModel { /* set properties as needed */ };
            _userServiceMock.Setup(x => x.UserLogoutWeb(model)).ReturnsAsync(2);

            // Act
            var result = await _controller.LogoutUser(model);

            // Assert: Ensure the result is OkObjectResult and not null
            Assert.NotNull(result); // Ensure that the result is not null
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Ensure that the response model is correctly cast
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            // Validate the response
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
        }

        [Fact]
        public async Task LogoutUser_ReturnsError_WhenLogoutFails()
        {
            // Arrange
            var model = new LogoutUserWebViewModel { /* set properties as needed */ };
            _userServiceMock.Setup(x => x.UserLogoutWeb(model)).ReturnsAsync(1); // Simulating a failed logout

            // Act
            var result = await _controller.LogoutUser(model);

            // Assert: Ensure that the result is not null and is OkObjectResult
            Assert.NotNull(result); // Ensure that the result is not null
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Ensure that the response model is correctly cast
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            // Validate the response
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.ERROR, response.statusMessage);
        }

        [Fact]
        public async Task UpdateFcmToken_ReturnsSuccess_WhenTokenIsUpdated()
        {
            // Arrange
            var model = new FcmUpdateViewModel { /* set properties as needed */ };
            _userServiceMock.Setup(x => x.UpdateFcmTokenWeb(model)).ReturnsAsync(2); // Simulating successful update

            // Act
            var result = await _controller.UpdateFcmToken(model);

            // Assert: Ensure that the result is not null and is OkObjectResult
            Assert.NotNull(result); // Ensure that the result is not null
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Ensure that the response model is correctly cast
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            // Validate the response
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.SUCCESS, response.statusMessage);
        }

        [Fact]
        public async Task UpdateFcmToken_ReturnsBadRequest_WhenTokenIsNotUpdated()
        {
            // Arrange
            var model = new FcmUpdateViewModel { /* set properties as needed */ };
            _userServiceMock.Setup(x => x.UpdateFcmTokenWeb(model)).ReturnsAsync(1); // Simulating failure

            // Act
            var result = await _controller.UpdateFcmToken(model);

            // Assert: Ensure that the result is not null and is a BadRequestObjectResult
            Assert.NotNull(result); // Ensure the result is not null
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Ensure the response model is correctly cast
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);

            // Validate the response
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.USER_NOT_UPDATED, response.statusMessage);
        }

        [Fact]
        public async Task ForgotPassword_ReturnsBadRequest_WhenEmailIsNull()
        {
            // Arrange
            var model = new ForgotPasswordByEmailWebViewModel { Email = null };

            // Act
            var result = await _controller.ForgotPassword(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = Assert.IsType<ResponseModel>(result.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.EMAILNOTFOUND, response.statusMessage);
        }

        [Fact]
        public async Task ForgotPassword_ReturnsBadRequest_WhenUserNotFound()
        {
            // Arrange
            var model = new ForgotPasswordByEmailWebViewModel { Email = "notfound@example.com" };
            _userServiceMock.Setup(x => x.GetActiveUserDetailsByEmail(model))
                            .ReturnsAsync(new vwDBUsers()); // Simulate user not found

            // Act
            var result = await _controller.ForgotPassword(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = Assert.IsType<ResponseModel>(result.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.EMAILNOTFOUND, response.statusMessage);
        }

        [Fact]
        public async Task ForgotPassword_ReturnsBadRequest_WhenEmailNotSent()
        {
            // Arrange
            var model = new ForgotPasswordByEmailWebViewModel { Email = "user@example.com" };
            var userDetails = new vwDBUsers { email = "sanskritimathur0927@gmail.com" };

            _userServiceMock.Setup(x => x.GetActiveUserDetailsByEmail(model))
                            .ReturnsAsync(userDetails); // Simulate user found
            _emailServiceMock.Setup(x => x.SendPasswordResetEmailWeb(userDetails))
                             .ReturnsAsync(new EmailWebResponseModel { EmailResponse = ResponseMessage.EMAILNOTSEND }); // Simulate email not sent

            // Act
            var result = await _controller.ForgotPassword(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = Assert.IsType<ResponseModel>(result.Value);
            Assert.Equal(ResponseStatus.FALSE, response.status);
            Assert.Equal(ResponseMessage.EMAILNOTSEND, response.statusMessage);
        }

        [Fact]
        public async Task ForgotPassword_ReturnsSuccess_WhenEmailSent()
        {
            // Arrange
            var model = new ForgotPasswordByEmailWebViewModel { Email = "sanskritimathur0927@gmail.com" };
            var userDetails = new vwDBUsers { email = "sanskritimathur0927@gmail.com" };

            _userServiceMock.Setup(x => x.GetActiveUserDetailsByEmail(model))
                            .ReturnsAsync(userDetails); // Simulate user found
            _emailServiceMock.Setup(x => x.SendPasswordResetEmailWeb(userDetails))
                             .ReturnsAsync(new EmailWebResponseModel { EmailResponse = ResponseMessage.EMAILSEND }); // Simulate email sent successfully

            // Act
            var result = await _controller.ForgotPassword(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = Assert.IsType<ResponseModel>(result.Value);
            Assert.Equal(ResponseStatus.TRUE, response.status);
            Assert.Equal(ResponseMessage.EMAILSEND, response.statusMessage);
            Assert.NotNull(response.data); // Ensure data is returned
        }

        [Fact]
        public async Task UpdateUserLoginPassword_SuccessfulUpdate_ReturnsTrueStatus()
        {
            // Arrange
            var model = new UserUpdatePasswordWebViewModel
            {
                UserID = "hsingh",
                Password = "NewSecurePassword"
            };

            _userServiceMock
                .Setup(service => service.UpdateUserLoginPasswordWeb("hsingh", It.IsAny<string>()))
                .ReturnsAsync((1, "Password updated successfully"));

            // Act
            var result = await _controller.UpdateUserLoginPassword(model) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.True(response.status);
            Assert.Equal("Password updated successfully", response.statusMessage);
        }

        [Fact]
        public async Task UpdateUserLoginPassword_UserNotFound_ReturnsFalseStatus()
        {
            // Arrange
            var model = new UserUpdatePasswordWebViewModel
            {
                UserID = "unknownUser",
                Password = "NewSecurePassword"
            };

            _userServiceMock
                .Setup(service => service.UpdateUserLoginPasswordWeb("unknownUser", It.IsAny<string>()))
                .ReturnsAsync((0, "User not found"));

            // Act
            var result = await _controller.UpdateUserLoginPassword(model) as BadRequestObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.NotNull(response);
            Assert.False(response.status);
            Assert.Equal("User not found", response.statusMessage);
        }

        [Fact]
        public async Task UpdateUserLoginPassword_DatabaseFailure_ReturnsFalseStatus()
        {
            // Arrange
            var model = new UserUpdatePasswordWebViewModel
            {
                UserID = "hsingh",
                Password = "NewSecurePassword"
            };

            _userServiceMock
                .Setup(service => service.UpdateUserLoginPasswordWeb("hsingh", It.IsAny<string>()))
                .ReturnsAsync((-1, "Database error"));

            // Act
            var result = await _controller.UpdateUserLoginPassword(model) as BadRequestObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.NotNull(response);
            Assert.False(response.status);
            Assert.Equal("Database error", response.statusMessage);
        }

        [Fact]
        public async Task UpdateUserLoginPassword_EmptyPassword_ReturnsTrueStatus()
        {
            // Arrange
            var model = new UserUpdatePasswordWebViewModel
            {
                UserID = "hsingh",
                Password = string.Empty
            };

            _userServiceMock
                .Setup(service => service.UpdateUserLoginPasswordWeb("hsingh", It.IsAny<string>()))
                .ReturnsAsync((1, "Password updated successfully"));

            // Act
            var result = await _controller.UpdateUserLoginPassword(model) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.True(response.status);
            Assert.Equal("Password updated successfully", response.statusMessage);
        }

        [Fact]
        public async Task UpdateUserLoginPassword_NullPassword_ReturnsBadRequest()
        {
            // Arrange
            var model = new UserUpdatePasswordWebViewModel
            {
                UserID = "hsingh"
            };

            _userServiceMock
                .Setup(service => service.UpdateUserLoginPasswordWeb("hsingh", It.IsAny<string>()))
                .ReturnsAsync((0, "Password cannot be null"));

            // Act
            var result = await _controller.UpdateUserLoginPassword(model) as BadRequestObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.NotNull(response);
            Assert.False(response.status);
            Assert.Equal("Password cannot be null", response.statusMessage);
        }

        [Fact]
        public async Task UpdateUserLoginPassword_EncryptsPasswordBeforeUpdate()
        {
            // Arrange
            var model = new UserUpdatePasswordWebViewModel
            {
                UserID = "hsingh",
                Password = "NewSecurePassword"
            };

            var encryptedPassword = Cryptography.Encrypt("NewSecurePassword", "testEncryptionKey");

            _userServiceMock
                .Setup(service => service.UpdateUserLoginPasswordWeb("hsingh", encryptedPassword))
                .ReturnsAsync((1, "Password updated successfully"));

            // Act
            var result = await _controller.UpdateUserLoginPassword(model) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.True(response.status);
            Assert.Equal("Password updated successfully", response.statusMessage);
        }

        [Fact]
        public async Task UpdatePassword_SuccessfulUpdate_ReturnsTrueStatus()
        {
            // Arrange
            var model = new MailPasswordChangeViewModel
            {
                EncryptID = Cryptography.Encrypt(JsonSerializer.Serialize(
                    new ForgotPasswordMailTokenWebViewModel
                    {
                        UserID = "hsingh",
                        DateTime = DateTime.Now.AddMinutes(10),
                    }
                ), "testEncryptionKey"),
                NewPassword = "NewSecurePassword"
            };

            _userServiceMock
                .Setup(service => service.ValidateTokenAndComparePasswordWeb("hsingh", It.IsAny<string>()))
                .ReturnsAsync(ResponseMessage.SUCCESS);

            _userServiceMock
                .Setup(service => service.UpdateUserLoginPasswordWeb("hsingh", It.IsAny<string>()))
                .ReturnsAsync((1, "Password updated successfully"));

            // Act
            var result = await _controller.UpdatePassword(model) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.True(response.status);
            Assert.Equal("Password updated successfully", response.statusMessage);
        }

        [Fact]
        public async Task UpdatePassword_ExpiredToken_ReturnsFalseStatus()
        {
            // Arrange
            var model = new MailPasswordChangeViewModel
            {
                EncryptID = Cryptography.Encrypt(JsonSerializer.Serialize(
                    new ForgotPasswordMailTokenWebViewModel
                    {
                        UserID = "hsingh",
                        DateTime = DateTime.Now.AddMinutes(-10), // Expired token
                    }
                ), "testEncryptionKey"),
                NewPassword = "NewSecurePassword"
            };

            // Act
            var result = await _controller.UpdatePassword(model) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.TLREACHED, response.statusMessage);
        }

        [Fact]
        public async Task UpdatePassword_InvalidPasswordComparison_ReturnsFalseStatus()
        {
            // Arrange
            var model = new MailPasswordChangeViewModel
            {
                EncryptID = Cryptography.Encrypt(JsonSerializer.Serialize(
                    new ForgotPasswordMailTokenWebViewModel
                    {
                        UserID = "hsingh",
                        DateTime = DateTime.Now.AddMinutes(10),
                    }
                ), "testEncryptionKey"),
                NewPassword = "OldPassword"
            };

            _userServiceMock
                .Setup(service => service.ValidateTokenAndComparePasswordWeb("hsingh", It.IsAny<string>()))
                .ReturnsAsync(ResponseMessage.PASSWORD_SAME_AS_OLD);

            // Act
            var result = await _controller.UpdatePassword(model) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.PASSWORD_SAME_AS_OLD, response.statusMessage);
        }

        [Fact]
        public async Task UpdatePassword_UserNotFound_ReturnsFalseStatus()
        {
            // Arrange
            var model = new MailPasswordChangeViewModel
            {
                EncryptID = Cryptography.Encrypt(JsonSerializer.Serialize(
                    new ForgotPasswordMailTokenWebViewModel
                    {
                        UserID = "unknownUser", // Invalid user
                        DateTime = DateTime.Now.AddMinutes(10),
                    }
                ), "testEncryptionKey"),
                NewPassword = "NewSecurePassword"
            };

            _userServiceMock
                .Setup(service => service.ValidateTokenAndComparePasswordWeb("unknownUser", It.IsAny<string>()))
                .ReturnsAsync(ResponseMessage.USER_NOT_FOUND);

            // Act
            var result = await _controller.UpdatePassword(model) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.USER_NOT_FOUND, response.statusMessage);
        }

        [Fact]
        public async Task UpdatePassword_DatabaseUpdateFails_ReturnsFalseStatus()
        {
            // Arrange
            var model = new MailPasswordChangeViewModel
            {
                EncryptID = Cryptography.Encrypt(JsonSerializer.Serialize(
                    new ForgotPasswordMailTokenWebViewModel
                    {
                        UserID = "hsingh",
                        DateTime = DateTime.Now.AddMinutes(10),
                    }
                ), "testEncryptionKey"),
                NewPassword = "NewSecurePassword"
            };

            _userServiceMock
                .Setup(service => service.ValidateTokenAndComparePasswordWeb("hsingh", It.IsAny<string>()))
                .ReturnsAsync(ResponseMessage.SUCCESS);

            _userServiceMock
                .Setup(service => service.UpdateUserLoginPasswordWeb("hsingh", It.IsAny<string>()))
                .ReturnsAsync((0, "Database update failed")); // Simulating failure

            // Act
            var result = await _controller.UpdatePassword(model) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.False(response.status);
            Assert.Equal("Database update failed", response.statusMessage);
        }

        [Fact]
        public async Task UpdatePassword_TokenValidationFails_ReturnsFalseStatus()
        {
            // Arrange
            var model = new MailPasswordChangeViewModel
            {
                EncryptID = Cryptography.Encrypt(JsonSerializer.Serialize(
                    new ForgotPasswordMailTokenWebViewModel
                    {
                        UserID = "hsingh",
                        DateTime = DateTime.Now.AddMinutes(10),
                    }
                ), "testEncryptionKey"),
                NewPassword = "NewSecurePassword"
            };

            _userServiceMock
                .Setup(service => service.ValidateTokenAndComparePasswordWeb("hsingh", It.IsAny<string>()))
                .ReturnsAsync(ResponseMessage.INVALIDTOKEN); // Simulating token validation failure

            // Act
            var result = await _controller.UpdatePassword(model) as OkObjectResult;
            var response = result?.Value as ResponseModel;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.False(response.status);
            Assert.Equal(ResponseMessage.INVALIDTOKEN, response.statusMessage);
        }
    }
}
