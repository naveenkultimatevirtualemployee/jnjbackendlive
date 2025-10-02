using Dapper;
using JNJServices.Business.Abstracts;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.DbConstants;
using JNJServices.Utility.Extensions;
using System.Data;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IDapperContext _context;

        public UserService(IDapperContext context)
        {
            _context = context;
        }

        public async Task<(UserLoginAppResponseModel?, string)> VerifyUserLoginDetailsApp(UserLoginAppViewModel model)
        {
            var procedureName = ProcEntities.spLoginMobile;
            var parameters = new DynamicParameters();

            parameters.Add(DbParams.Type, model.Type, DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add(DbParams.UserID, model.UserID, DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add(DbParams.DeviceID, model.DeviceID, DbType.String, direction: ParameterDirection.Input);
            parameters.Add(DbParams.FcmToken, model.FcmToken, DbType.String, direction: ParameterDirection.Input);
            parameters.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);

            if (!string.IsNullOrEmpty(model.PhoneNo))
            {
                parameters.Add(DbParams.PhoneNo, model.PhoneNo, DbType.String, direction: ParameterDirection.Input);
            }
            else if (!string.IsNullOrEmpty(model.LoginPassword))
            {
                parameters.Add(DbParams.LoginPassword, model.LoginPassword, DbType.String, direction: ParameterDirection.Input);
            }

            var response = await _context.ExecuteQueryFirstOrDefaultAsync<UserLoginAppResponseModel>(procedureName, parameters, commandType: CommandType.StoredProcedure);
            var message = parameters.Get<string?>(DbParams.Msg) ?? string.Empty;

            return (response, message);
        }

        public async Task<vwDBUsers?> VerifyUserLoginDetailsWeb(UserLoginWebViewModel model)
        {
            var procedureName = ProcEntities.spWebLogin;
            var parameters = new DynamicParameters();

            parameters.Add(DbParams.UserName, model.Username, DbType.String, direction: ParameterDirection.Input);
            parameters.Add(DbParams.Password, model.Password, DbType.String, direction: ParameterDirection.Input);
            parameters.Add(DbParams.UserDeviceID, model.UserDeviceID, DbType.String, direction: ParameterDirection.Input);
            parameters.Add(DbParams.UserFcmToken, model.UserFcmToken, DbType.String, direction: ParameterDirection.Input);

            var response = await _context.ExecuteQueryFirstOrDefaultAsync<vwDBUsers>(procedureName, parameters, commandType: CommandType.StoredProcedure);

            return response;
        }

        public async Task UpdateUserToken(string token, int recordId, int? type)
        {
            string query = string.Empty;
            if (type is null)
            {
                query = "Update DBUsers Set UserBearerToken = @token Where UserNum = @recordId";
            }

            if (type.HasValue && type.Value == 1)
            {
                query = "Update Contractors set BearerToken =@token where ContractorID = @recordId";
            }

            if (type.HasValue && type.Value == 2)
            {
                query = "Update Claimants set BearerToken =@token where ClaimantID = @recordId";
            }

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.RecordId, recordId, DbType.Int32);
            parameters.Add(DbParams.Token, token, DbType.String);

            await _context.ExecuteAsync(query, parameters);
        }

        public async Task<(int, string)> UserForgotPasswordApp(AppForgotPasswordViewModel model)
        {
            var procedureName = ProcEntities.spMobilePwdChange;
            var parameters = new DynamicParameters();

            parameters.Add(DbParams.Type, model.Type, DbType.Int32);
            parameters.Add(DbParams.UserID, model.UserID, DbType.Int32);
            parameters.Add(DbParams.NewPwd, model.Password, DbType.String);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);

            await _context.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);

            var responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameters.Get<string?>(DbParams.Msg) ?? "";

            return (responseCode, message);
        }

        public async Task<(int, string)> VerifyPhoneNumberApp(VerifyPhoneNumberViewModel model)
        {
            var procedureName = ProcEntities.spMobileCheckPhone;
            var parameters = new DynamicParameters();

            parameters.Add(DbParams.Type, model.Type, DbType.Int32);
            parameters.Add(DbParams.UserID, model.UserID, DbType.Int32);
            parameters.Add(DbParams.PhoneNo, model.PhoneNo, DbType.String);
            parameters.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _context.ExecuteQueryFirstOrDefaultAsync<VerifyPhoneNumberResponseModel>(procedureName, parameters, commandType: CommandType.StoredProcedure);

            var responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameters.Get<string?>(DbParams.Msg) ?? "";

            return (responseCode, message);
        }

        public async Task<int> LogoutUserApp(int type, int userID)
        {
            string query = string.Empty;

            if (type == (int)Utility.Enums.Type.Contractor)
            {
                query = "Update Contractors set FcmToken = null , DeviceID = null,BearerToken=null where ContractorID = @UserID";
            }
            else if (type == (int)Utility.Enums.Type.Claimant)
            {
                query = "Update Claimants set FcmToken = null , DeviceID = null,BearerToken=null where ClaimantID = @UserID";
            }

            if (!string.IsNullOrEmpty(query))
            {
                var parameter = new DynamicParameters();
                parameter.Add(DbParams.UserID, userID);

                var response = await _context.ExecuteAsync(query, parameter);
                return response;
            }

            return 0;
        }

        public async Task<int> UpdateFcmTokenApp(UpdateFcmTokenAppViewModel model)
        {
            string query = string.Empty;

            if (model.Type == (int)Utility.Enums.Type.Contractor)
            {
                query = "Update Contractors Set FcmToken = @UserFcm , DeviceID = @UserDeviceID where ContractorID = @UserID";
            }
            else if (model.Type == (int)Utility.Enums.Type.Claimant)
            {
                query = "Update Claimants Set FcmToken = @UserFcm , DeviceID = @UserDeviceID where ClaimantID = @UserID";
            }

            if (!string.IsNullOrEmpty(query))
            {
                var parameters = new DynamicParameters();
                parameters.Add(DbParams.UserFcm, model.FcmToken, DbType.String);
                parameters.Add(DbParams.UserDeviceID, model.DeviceID, DbType.String);
                parameters.Add(DbParams.UserID, model.UserID, DbType.Int32);

                var response = await _context.ExecuteAsync(query, parameters);
                return response;
            }

            return 0;
        }

        public async Task<int> ValidateUserAuthorization(int type, int userId, string token)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.Type, type, DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add(DbParams.UserID, userId, DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add(DbParams.Token, token, DbType.String, direction: ParameterDirection.Input);

            return await _context.ExecuteQueryFirstOrDefaultAsync<int>(ProcEntities.spCheckValidUser, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<vwDBUsers>> SearchUsersDetailWeb(UserSearchViewModel model)
        {
            var parameters = new DynamicParameters();

            string query = "SELECT *,Count(1) Over() as TotalCount from vwDBUsers where 1=1 ";
            if (!string.IsNullOrEmpty(model.UserID))
            {
                query += " and UserID = @UserID ";
                parameters.Add("@UserID", model.UserID, DbType.String);
            }
            if (!string.IsNullOrEmpty(model.UserName))
            {
                query += " and UserName Like '%'+ @UserName +'%' ";
                parameters.Add("@UserName", model.UserName, DbType.String);
            }
            if (!string.IsNullOrEmpty(model.email))
            {
                query += " and email Like '%'+ @email +'%' ";
                parameters.Add("@email", model.email, DbType.String);
            }
            if (model.Role.ToValidateIntWithZero())
            {
                query += " and groupnum = @groupnum ";
                parameters.Add("@groupnum", model.Role, DbType.String);
            }
            if (model.inactiveflag.ToValidateInActiveFlag())
            {
                query += " and inactiveflag =  @inactiveflag ";
                parameters.Add("@inactiveflag", Convert.ToInt32(model.inactiveflag), DbType.Int32);
            }

            query += " order By userNum desc offset (@Page - 1)*@Limit rows fetch next @Limit rows only";
            parameters.Add(DbParams.Page, model.Page, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit, DbType.Int32);

            var response = await _context.ExecuteQueryAsync<vwDBUsers>(query, parameters, CommandType.Text);
            return response;

        }

        public async Task<(int, string)> ResetUserPasswordWeb(ForgotPasswordViewModel model)
        {
            string procedureName = ProcEntities.spDBUserLoginUpdate;

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.LoginEmail, model.EmailID, DbType.String);
            parameters.Add(DbParams.LoginPwd, model.oldPassword, DbType.String);
            parameters.Add(DbParams.NewPwd, model.newPassword, DbType.String);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);

            await _context.ExecuteScalerAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
            var responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            var message = parameters.Get<string?>(DbParams.Msg) ?? "";
            return (responseCode, message);

        }

        public async Task<IEnumerable<UserRoles>> GetUserRoles()
        {
            var query = "Select * From DBUserGroups where inactiveflag = 0 ";

            var response = await _context.ExecuteQueryAsync<UserRoles>(query, CommandType.Text);
            return response;

        }

        public async Task<vwDBUsers?> GetActiveUserDetailsByEmail(ForgotPasswordByEmailWebViewModel model)
        {
            string query = "Select  top(1)  * From vwDBUsers where email = @Email and inactiveflag = 0";
            var parameter = new DynamicParameters();
            parameter.Add(DbParams.Email, model.Email, DbType.String);

            var response = await _context.ExecuteQueryFirstOrDefaultAsync<vwDBUsers>(query, parameter, CommandType.Text);

            return response;
        }

        public async Task<string> ValidateTokenAndComparePasswordWeb(string UserID, string newPasswordEncrypted)
        {
            string query = "SELECT loginpwd FROM DBUsers WHERE UserID = @UserID AND inactiveflag = 0";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.UserID, UserID, DbType.String);

            var result = await _context.ExecuteQueryFirstOrDefaultAsync<string>(query, parameters, CommandType.Text);

            if (!string.IsNullOrEmpty(result) && (result == newPasswordEncrypted))
            {
                return ResponseMessage.PASSWORD_SAME_AS_OLD;
            }
            return ResponseMessage.SUCCESS;
        }

        public async Task<int> UpdateFcmTokenWeb(FcmUpdateViewModel model)
        {
            string query = "Update DBUsers Set UserFcmToken = @UserFcm , UserDeviceID = @UserDeviceID where UserID = @UserID";

            var parameter = new DynamicParameters();
            parameter.Add(DbParams.UserFcm, model.UserFcmToken, DbType.String);
            parameter.Add(DbParams.UserDeviceID, model.UserDeviceID, DbType.String);
            parameter.Add(DbParams.UserID, model.UserID, DbType.String);

            var response = await _context.ExecuteAsync(query, parameter);
            return response;

        }

        public async Task<(int, string)> UpdateUserLoginPasswordWeb(string userId, string password)
        {
            string procedureName = ProcEntities.spUpdateDBUserPassword;

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.UserID, userId, DbType.String);
            parameters.Add(DbParams.NewPassword, password, DbType.String);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);

            await _context.ExecuteAsync(procedureName, parameters, CommandType.StoredProcedure);
            var responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            var message = parameters.Get<string?>(DbParams.Msg) ?? "";
            return (responseCode, message);
        }

        public async Task<(int, string)> ResetContractorClaimantPasswordWeb(ContractorClaimantResetPasswordWebViewModel model)
        {
            string procedureName = ProcEntities.spResetClaimantContractorPassword;

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.Type, model.Type, DbType.Int32);
            parameters.Add(DbParams.UserID, model.UserID, DbType.Int32);
            parameters.Add(DbParams.NewPwd, model.Password, DbType.String);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);

            await _context.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
            var responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            var message = parameters.Get<string?>(DbParams.Msg) ?? "";

            return (responseCode, message);
        }

        public async Task<int> UserLogoutWeb(LogoutUserWebViewModel logout)
        {
            var query = "Update DBUsers set UserFcmToken = null , UserDeviceID = null,UserBearerToken = NULL where UserID = @UserID";

            var parameter = new DynamicParameters();
            parameter.Add(DbParams.UserID, logout.UserID, DbType.String);

            var response = await _context.ExecuteAsync(query, parameter);
            return response;
        }

        public async Task<(int, string)> UpdateNotificationAndToken(TokenAndNotificationUpdateRequest model)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.Token, model.Token, DbType.String);
            parameters.Add(DbParams.UserID, model.UserId, DbType.Int32);
            parameters.Add(DbParams.NotificationStatus, model.NotificationStatus, DbType.Int32);
            parameters.Add(DbParams.Type, model.Type, DbType.Int32);
            parameters.Add(DbParams.ResponseCode, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

            await _context.ExecuteAsync(ProcEntities.spUpdateUserTokenAndNotificationStatus, parameters, CommandType.StoredProcedure);

            var responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameters.Get<string?>(DbParams.Msg) ?? "";

            return (responseCode, message);
        }

        public async Task<(int, string, int)> GetNotificationStatusAsync(int userId, int type)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.UserID, userId, DbType.Int32);
            parameters.Add(DbParams.Type, type, DbType.Int32);
            parameters.Add(DbParams.NotificationStatus, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.ResponseCode, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

            await _context.ExecuteAsync(ProcEntities.spGetNotificationStatus, parameters, CommandType.StoredProcedure);

            var responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameters.Get<string?>(DbParams.Msg) ?? "";
            int notificationStatus = parameters.Get<int?>(DbParams.NotificationStatus) ?? 0;

            return (responseCode, message, notificationStatus);
        }
    }
}
