using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;

namespace JNJServices.Business.Abstracts
{
	public interface IUserService
	{
		Task<(UserLoginAppResponseModel?, string)> VerifyUserLoginDetailsApp(UserLoginAppViewModel model);
		Task UpdateUserToken(string token, int recordId, int? type);
		Task<vwDBUsers?> VerifyUserLoginDetailsWeb(UserLoginWebViewModel model);
		Task<(int, string)> UserForgotPasswordApp(AppForgotPasswordViewModel model);
		Task<(int, string)> VerifyPhoneNumberApp(VerifyPhoneNumberViewModel model);
		Task<int> LogoutUserApp(int type, int userID);
		Task<int> UpdateFcmTokenApp(UpdateFcmTokenAppViewModel model);
		Task<int> ValidateUserAuthorization(int type, int userId, string token);
		Task<IEnumerable<vwDBUsers>> SearchUsersDetailWeb(UserSearchViewModel model);
		Task<(int, string)> ResetUserPasswordWeb(ForgotPasswordViewModel model);
		Task<IEnumerable<UserRoles>> GetUserRoles();
		Task<vwDBUsers?> GetActiveUserDetailsByEmail(ForgotPasswordByEmailWebViewModel model);
		Task<string> ValidateTokenAndComparePasswordWeb(string UserID, string newPasswordEncrypted);
		Task<int> UpdateFcmTokenWeb(FcmUpdateViewModel model);
		Task<(int, string)> UpdateUserLoginPasswordWeb(string userId, string password);
		Task<(int, string)> ResetContractorClaimantPasswordWeb(ContractorClaimantResetPasswordWebViewModel model);
		Task<int> UserLogoutWeb(LogoutUserWebViewModel logout);
		Task<(int, string)> UpdateNotificationAndToken(TokenAndNotificationUpdateRequest model);
		Task<(int, string, int)> GetNotificationStatusAsync(int userId, int type);
	}
}
