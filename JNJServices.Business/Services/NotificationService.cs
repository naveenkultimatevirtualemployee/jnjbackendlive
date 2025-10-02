using Dapper;
using JNJServices.Business.Abstracts;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.CommonModels;
using JNJServices.Models.DbResponseModels;
using JNJServices.Utility.DbConstants;
using JNJServices.Utility.Helper;
using Newtonsoft.Json;
using System.Data;

namespace JNJServices.Business.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IDapperContext _context;
        private readonly TimeZoneConverter _timeZoneConverter;
        public NotificationService(IDapperContext context, TimeZoneConverter timeZoneConverter)
        {
            _context = context;
            _timeZoneConverter = timeZoneConverter;
        }

        public async Task<IEnumerable<AppNotificationsResponseModel>> GetNotificationsAsync(AppTokenDetails tokenClaims, int? page, int? limit)
        {
            string procedureName = ProcEntities.spGetAndMarkMobileNotifications;

            var parameters = new DynamicParameters();

            parameters.Add(DbParams.UserID, tokenClaims.UserID, DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add(DbParams.FcmToken, tokenClaims.FcmToken, DbType.String, direction: ParameterDirection.Input);
            parameters.Add(DbParams.UserType, tokenClaims.Type, DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add(DbParams.Page, page, DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add(DbParams.Limit, limit, DbType.Int32, direction: ParameterDirection.Input);

            return await _context.ExecuteQueryAsync<AppNotificationsResponseModel>(procedureName, parameters);
        }

        public async Task<int> DeleteNotificationAsync(AppTokenDetails tokenClaims)
        {
            string query = "DELETE FROM NotificationLog WHERE UserID = @UserID and UserType = @UserType";

            var parameters = new DynamicParameters();

            parameters.Add(DbParams.UserID, tokenClaims.UserID, DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add(DbParams.UserType, tokenClaims.Type, DbType.Int32, direction: ParameterDirection.Input);

            return await _context.ExecuteAsync(query, parameters, CommandType.Text);
        }

        public async Task<IEnumerable<WebNotificationResponseModel>> GetUserWebNotifications(string claimValue, int? Page, int? Limit)
        {
            string procedureName = ProcEntities.spGetAndMarkWebNotifications;
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.UserID, claimValue, DbType.String);
            parameters.Add(DbParams.Page, Page, DbType.Int32);
            parameters.Add(DbParams.Limit, Limit, DbType.Int32);

            return await _context.ExecuteQueryAsync<WebNotificationResponseModel>(procedureName, parameters, CommandType.StoredProcedure);
        }

        public async Task<int> DeleteUserWebNotifications(string UserID)
        {
            var query = "DELETE FROM NotificationWebLog where  WebUsers Like '%' + @UserID + '%' ";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.UserID, UserID, DbType.String);

            return await _context.ExecuteAsync(query, parameters, CommandType.Text);
        }

        //Data Fetch for notification Claimant accept
        public async Task<IEnumerable<ContractorFirstAcceptCancelResponseModel>> ClaimantAcceptCancel(int ReservationID)
        {
            var parameter = new DynamicParameters();
            parameter.Add(DbParams.ReservationID, ReservationID, DbType.Int32);

            return await _context.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(ProcEntities.spNotificationClaimantAcceptLogic, parameter, CommandType.StoredProcedure);
        }

        //Fetch Data when Claimant Not found
        public async Task<IEnumerable<ContractorFirstAcceptCancelResponseModel>> ClaimantAcceptContractornotfoundLogic(int ReservationID)
        {
            var parameter = new DynamicParameters();
            string query = @"
                SELECT a.ReservationsAssignmentsID,a.AssgnNum,  a.ResAsgnCode, a.ResTripType, 
                       a.ClaimantID, a.ASSGNCode, a.RSVATTCode, a.PUAddress1, a.PUAddress2, 
                       a.DOAddress1, a.DOAddress2, a.ReservationDate, a.ReservationTime, a.PickupTime
                FROM vwReservationAssignSearch a 
                WHERE a.ReservationID = @ReservationID";
            parameter.Add(DbParams.ReservationID, ReservationID);

            return await _context.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(query, parameter, CommandType.Text);
        }

        public async Task<IEnumerable<ContractorFirstAcceptCancelResponseModel>> ContractorFirstCancelLogic(int ReservationAssignmentID)
        {
            var parameter = new DynamicParameters();
            string query = @"
            SELECT a.ReservationsAssignmentsID, con.FcmToken, con.DeviceID, a.ResAsgnCode, a.ResTripType, 
                   a.ClaimantID, c.ContractorID, a.ASSGNCode, a.RSVATTCode, a.PUAddress1, a.PUAddress2, 
                   a.DOAddress1, a.DOAddress2, a.ReservationDate,a.AssgnNum,  a.ReservationTime, a.PickupTime, 
                   c.NotificationDateTime
            FROM vwReservationAssignSearch a 
            JOIN ContractorJobSearch c ON a.ReservationsAssignmentsID = c.ReservationsAssignmentsID 
            JOIN Contractors con ON c.ContractorId = con.ContractorID  
            WHERE a.ReservationsAssignmentsID = @ReservationsAssignmentsID AND c.JobStatus = 0";

            parameter.Add(DbParams.ReservationsAssignmentsID, ReservationAssignmentID);

            return await _context.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(query, parameter, CommandType.Text);
        }

        public async Task<IEnumerable<ContractorFirstAcceptCancelResponseModel>> ContractorFirstCancelDataNotFound(int ReservationAssignmentID)
        {
            var query = @"
                SELECT Top(1)  a.ReservationsAssignmentsID,  a.ResAsgnCode,a.AssgnNum,  a.ResTripType, 
                       a.ClaimantID,  a.ASSGNCode, a.RSVATTCode, a.PUAddress1, a.PUAddress2, 
                       a.DOAddress1, a.DOAddress2, a.ReservationDate, a.ReservationTime, a.PickupTime, 
                       GetDate() As NotificationDateTime,'ASSIGNMENT_NEED_ATTENTION' As NotificationType
                FROM vwReservationAssignSearch a 
                WHERE a.ReservationsAssignmentsID = @ReservationsAssignmentsID  "
              ;
            var parameter = new DynamicParameters();

            parameter.Add(DbParams.ReservationsAssignmentsID, ReservationAssignmentID);

            return await _context.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(query, parameter, CommandType.Text);
        }

        public async Task<IEnumerable<ContractorFirstAcceptCancelResponseModel>> ContractorAcceptLogic(int ReservationAssignmentID)
        {

            var parameter = new DynamicParameters();
            string query = @"
            SELECT a.ReservationsAssignmentsID, con.FcmToken, con.DeviceID,a.AssgnNum,  a.ResAsgnCode, a.ResTripType, 
                   a.ClaimantID, c.ContractorID, a.ASSGNCode, a.RSVATTCode, a.PUAddress1, a.PUAddress2, 
                   a.DOAddress1, a.DOAddress2, a.ReservationDate, a.ReservationTime, a.PickupTime, 
                   getdate() as NotificationDateTime
            FROM vwReservationAssignSearch a 
            JOIN ContractorJobSearch c ON a.ReservationsAssignmentsID = c.ReservationsAssignmentsID 
            JOIN Contractors con ON a.ContractorId = con.ContractorID  
            WHERE a.ReservationsAssignmentsID = @ReservationsAssignmentsID AND c.JobStatus = 1 and a.ContractorID is not null";

            parameter.Add(DbParams.ReservationsAssignmentsID, ReservationAssignmentID);
            return await _context.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(query, parameter, CommandType.Text);

        }

        public async Task<IEnumerable<ContractorFirstAcceptCancelResponseModel>> ContractorCancelLogic(int ReservationAssignmentID)
        {

            var parameter = new DynamicParameters();
            string query = @"
            SELECT a.ReservationsAssignmentsID, con.FcmToken, con.DeviceID, a.ResAsgnCode, a.ResTripType, 
               a.ClaimantID, a.ContractorID, a.ASSGNCode, a.RSVATTCode, a.PUAddress1, a.PUAddress2, 
               a.DOAddress1, a.DOAddress2, a.ReservationDate, a.ReservationTime, a.PickupTime, 
               getdate() as NotificationDateTime
            FROM vwReservationAssignSearch a 
            JOIN Contractors con ON a.ContractorId = con.ContractorID  
            WHERE a.ReservationsAssignmentsID = @ReservationsAssignmentsID and a.inactiveflag = -1";

            parameter.Add(DbParams.ReservationsAssignmentsID, ReservationAssignmentID);
            return await _context.ExecuteQueryAsync<ContractorFirstAcceptCancelResponseModel>(query, parameter, CommandType.Text);
        }

        public async Task<IEnumerable<ClaimantCancelResponseModel>> ClaimantCancelLogic(int ReservationID)
        {

            var parameter = new DynamicParameters();
            string query = @"
            SELECT a.ReservationsAssignmentsID, con.FcmToken, con.DeviceID, a.ResAsgnCode, a.ResTripType, 
               a.ClaimantID, a.ContractorID, a.ASSGNCode, a.RSVATTCode, a.PUAddress1, a.PUAddress2, 
               a.DOAddress1, a.DOAddress2, a.ReservationDate, a.ReservationTime, a.PickupTime, 
               getdate() as NotificationDateTime
            FROM vwReservationAssignSearch a 
            JOIN Contractors con ON a.ContractorId = con.ContractorID  
            WHERE a.ReservationID = @ReservationID and a.RSVACCode = 'ResCancel'  and a.inactiveflag = 0";

            parameter.Add(DbParams.ReservationID, ReservationID);

            return await _context.ExecuteQueryAsync<ClaimantCancelResponseModel>(query, parameter, CommandType.Text);
        }

        public async Task<IEnumerable<ClaimantCancelResponseModel>> ClaimantCancelNodataFound(int ReservationID)
        {
            var parameter = new DynamicParameters();

            var query = @"SELECT a.ReservationsAssignmentsID, a.ResAsgnCode, a.ResTripType, 
                       a.ClaimantID, a.ContractorID, a.ASSGNCode, a.RSVATTCode, a.PUAddress1, a.PUAddress2, 
                       a.DOAddress1, a.DOAddress2, a.ReservationDate, a.ReservationTime, a.PickupTime, 
                       getdate() as NotificationDateTime
                    FROM vwReservationAssignSearch a 
                    WHERE a.ReservationID = @ReservationID and a.RSVACCode = 'ResCancel' and a.inactiveflag = 0";

            parameter.Add(DbParams.ReservationID, ReservationID);

            return await _context.ExecuteQueryAsync<ClaimantCancelResponseModel>(query, parameter, CommandType.Text);
        }

        public async Task<IEnumerable<ContractorNotificationResponseModel>> ClaimantTraking(int ReservationAssignmentID)
        {
            var parameters = new DynamicParameters();
            string query = @"
            SELECT a.ReservationsAssignmentsID,a.ReservationID, cl.FcmToken, cl.DeviceID, a.ResAsgnCode, a.ResTripType, 
               a.ClaimantID, a.ContractorID,a.Contractor, a.ASSGNCode, a.RSVATTCode, a.PUAddress1, a.PUAddress2, 
               a.DOAddress1, a.DOAddress2, a.ReservationDate, a.ReservationTime, a.PickupTime, 
               getdate() as NotificationDateTime,a.RSVPRCode
            FROM vwReservationAssignSearch a 
            JOIN Claimants cl ON a.ClaimantID = cl.ClaimantID  
            where a.ReservationsAssignmentsID = @ReservationsAssignmentsID";

            parameters.Add(DbParams.ReservationsAssignmentsID, ReservationAssignmentID);
            return await _context.ExecuteQueryAsync<ContractorNotificationResponseModel>(query, parameters, CommandType.Text);
        }

        public async Task<IEnumerable<ContractorNotAssignedResponseModel>> WebContractorNotAssignedLogic()
        {

            string query = ProcEntities.spContractorNotAssignedWebNotification;

            return await _context.ExecuteQueryAsync<ContractorNotAssignedResponseModel>(query, CommandType.StoredProcedure);

        }

        public async Task<IEnumerable<ContractorAssignedResponseModel>> WebContractorAssignedLogic()
        {

            string query = ProcEntities.spContractorAssignmentWebNotification;
            return await _context.ExecuteQueryAsync<ContractorAssignedResponseModel>(query, commandType: CommandType.StoredProcedure);

        }

        public async Task<IEnumerable<AssignmentJobRequestResponseModel>> AssignmentjobRequestaAndNotification()
        {
            string query = ProcEntities.spAssignmentJobSearchNotificationCronJob;

            return await _context.ExecuteQueryAsync<AssignmentJobRequestResponseModel>(query, CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ContractorWebJobSearchRespnseModel>> SendContractorWebJobSearchLogic(int AssignmentID)
        {
            var parameter = new DynamicParameters();
            string query = @"
                SELECT a.ReservationsAssignmentsID, con.FcmToken, con.DeviceID,a.AssgnNum,  a.ResAsgnCode, a.ResTripType, 
                       a.ClaimantID, c.ContractorID, a.ASSGNCode, a.RSVATTCode, a.PUAddress1, a.PUAddress2, 
                       a.DOAddress1, a.DOAddress2, a.ReservationDate, a.ReservationTime, a.PickupTime, 
                       c.NotificationDateTime,'NEW_ASSIGNMENT_REQUEST' as NotificationType
                FROM vwReservationAssignSearch a 
                JOIN ContractorJobSearch c ON a.ReservationsAssignmentsID = c.ReservationsAssignmentsID 
                JOIN Contractors con ON c.ContractorId = con.ContractorID  
                WHERE a.ReservationsAssignmentsID = @ReservationsAssignmentsID AND c.JobStatus = 0";

            parameter.Add(DbParams.ReservationsAssignmentsID, AssignmentID);
            return await _context.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(query, parameter, CommandType.Text);

        }

        public async Task<IEnumerable<ContractorWebJobSearchRespnseModel>> SendContractorWebJobSearchLogicNoDataFound(int AssignmentID)
        {
            var parameter = new DynamicParameters();
            var query = @"SELECT a.ReservationsAssignmentsID,a.ResAsgnCode, a.ResTripType, 
                   a.ClaimantID,  a.ASSGNCode, a.RSVATTCode, a.PUAddress1, a.PUAddress2, a.assgnnum,
                   a.DOAddress1, a.DOAddress2, a.ReservationDate, a.ReservationTime, a.PickupTime,GetDate() As NotificationDateTime,'ASSIGNMENT_NEED_ATTENTION' as NotificationType
                    FROM vwReservationAssignSearch a 
                    WHERE a.ReservationsAssignmentsID = @ReservationsAssignmentsID ";

            parameter.Add(DbParams.ReservationsAssignmentsID, AssignmentID);
            return await _context.ExecuteQueryAsync<ContractorWebJobSearchRespnseModel>(query, parameter, CommandType.Text);
        }

        //Mobile Notification insert
        public async Task InsertNotificationLog(InsertNotificationLog item)
        {

            var query = @"
                INSERT INTO NotificationLog (
                    ReferenceID, UserID, UserType,ReservationsAssignmentsID,Title, Body, Data, SentDate, ReadStatus, 
                    NotificationType, CreatedBy, CreatedDate,FcmToken
                ) VALUES (
                    @ReferenceID, @UserID,@UserType,@ReservationsAssignmentsID, @Title, @Body, @Data, @SentDate, @ReadStatus, 
                    @NotificationType, @CreatedBy, @CreatedDate,@FcmToken
                )";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReferenceID, Guid.NewGuid().ToString(), DbType.String);
            parameters.Add(DbParams.UserID, item.UserID, DbType.Int32);
            parameters.Add(DbParams.UserType, item.UserType, DbType.Int32);
            parameters.Add(DbParams.ReservationsAssignmentsID, item.ReservationsAssignmentsID, DbType.Int32);
            parameters.Add(DbParams.Title, item.Title, DbType.String);
            parameters.Add(DbParams.Body, item.Body, DbType.String);
            parameters.Add(DbParams.Data, JsonConvert.SerializeObject(item.data), DbType.String);
            parameters.Add(DbParams.SentDate, item.NotificationDateTime, DbType.DateTime);
            parameters.Add(DbParams.ReadStatus, 0, DbType.Int32);
            parameters.Add(DbParams.NotificationType, item.NotificationType, DbType.String);
            parameters.Add(DbParams.CreatedBy, item.CreatedBy, DbType.Int32);
            parameters.Add(DbParams.CreatedDate, _timeZoneConverter.ConvertUtcToConfiguredTimeZone(), DbType.DateTime);
            parameters.Add(DbParams.FcmToken, item.FcmToken, DbType.String);

            await _context.ExecuteAsync(query, parameters, commandType: CommandType.Text);

        }

        //Web Notification insert
        public async Task InsertNotificationLogWeb(InsertNotificationLog item)
        {
            var UserIds = await GetDistinctUserUserIDAsync();
            string UseridList = string.Join(",", UserIds);

            var WebFcmToken = await GetDistinctUserFcmTokensAsync();
            string WebFcmTokenList = string.Join(",", WebFcmToken);

            var query = @"
                INSERT INTO NotificationWebLog (
                    ReferenceID,ReservationsAssignmentsID, UserID, Title, Body, Data, SentDate,  
                    NotificationType, CreatedBy, CreatedDate,FcmToken,WebUsers
                ) VALUES (
                    @ReferenceID,@ReservationsAssignmentsID, @UserID, @Title, @Body, @Data, @SentDate,  
                    @NotificationType, @CreatedBy, @CreatedDate,@FcmToken,@WebUsers
                )";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReferenceID, Guid.NewGuid().ToString(), DbType.String);
            parameters.Add(DbParams.UserID, item.UserID, DbType.Int32);
            parameters.Add(DbParams.ReservationsAssignmentsID, item.ReservationsAssignmentsID, DbType.Int32);
            parameters.Add(DbParams.Title, item.Title, DbType.String);
            parameters.Add(DbParams.Body, item.Body, DbType.String);
            parameters.Add(DbParams.Data, JsonConvert.SerializeObject(item.data), DbType.String);
            parameters.Add(DbParams.SentDate, item.NotificationDateTime, DbType.DateTime);
            parameters.Add(DbParams.NotificationType, item.NotificationType, DbType.String);
            parameters.Add(DbParams.CreatedBy, item.CreatedBy, DbType.Int32);
            parameters.Add(DbParams.CreatedDate, _timeZoneConverter.ConvertUtcToConfiguredTimeZone(), DbType.DateTime);
            parameters.Add(DbParams.FcmToken, WebFcmTokenList, DbType.String);
            parameters.Add(DbParams.WebUsers, UseridList, DbType.String);

            await _context.ExecuteAsync(query, parameters, CommandType.Text);

        }

        // Web Trigger insert
        public async Task InsertNotificationTriggerLogWeb(InsertNotificationLog item)
        {

            var UserIds = await GetDistinctUserUserIDAsync();
            string UseridList = string.Join(",", UserIds);

            var query = @"
                INSERT INTO NotificationWebLog (
                    ReferenceID,ReservationsAssignmentsID, Title, Body, Data, SentDate,  
                    NotificationType, CreatedBy, CreatedDate,FcmToken,WebUsers
                ) VALUES (
                    @ReferenceID,@ReservationsAssignmentsID,   @Title, @Body, @Data, @SentDate,  
                    @NotificationType, @CreatedBy, @CreatedDate,@FcmToken,@WebUsers
                )";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReferenceID, Guid.NewGuid().ToString(), DbType.String);
            parameters.Add(DbParams.ReservationsAssignmentsID, item.ReservationsAssignmentsID, DbType.Int32);
            parameters.Add(DbParams.Title, item.Title, DbType.String);
            parameters.Add(DbParams.Body, item.Body, DbType.String);
            parameters.Add(DbParams.Data, JsonConvert.SerializeObject(item.data), DbType.String);
            parameters.Add(DbParams.SentDate, item.NotificationDateTime, DbType.DateTime);
            parameters.Add(DbParams.NotificationType, item.NotificationType, DbType.String);
            parameters.Add(DbParams.CreatedBy, item.CreatedBy, DbType.Int32);
            parameters.Add(DbParams.CreatedDate, _timeZoneConverter.ConvertUtcToConfiguredTimeZone(), DbType.DateTime);
            parameters.Add(DbParams.FcmToken, item.webFcmToken, DbType.String);
            parameters.Add(DbParams.WebUsers, UseridList, DbType.String);

            await _context.ExecuteAsync(query, parameters, CommandType.Text);

        }

        //Get Admin,schedular FCM token
        public async Task<List<string>> GetDistinctUserFcmTokensAsync()
        {

            var query = @"
                SELECT DISTINCT userfcmToken  
                FROM DBUsers 
                WHERE userfcmToken IS NOT NULL and UserFcmToken <> '' and inactiveflag = 0 and groupnum != 3;";

            var result = await _context.ExecuteQueryAsync<string>(query, CommandType.Text);

            return result.ToList();

        }

        //Get Admin,schedular UserID
        public async Task<List<string>> GetDistinctUserUserIDAsync()
        {

            var query = @"
                SELECT DISTINCT UserID  
                FROM DBUsers 
                WHERE userfcmToken IS NOT NULL and UserFcmToken <> '' and inactiveflag = 0 and groupnum != 3;";

            var result = await _context.ExecuteQueryAsync<string>(query, CommandType.Text);

            return result.ToList();

        }

        public async Task DeleteInactiveChatRoomsAsync()
        {
            var query = "spDeleteOldInactiveChatRooms";
            await _context.ExecuteAsync(query, new DynamicParameters(), CommandType.StoredProcedure);
        }
        public async Task DeleteNotifications()
        {
            var query = "spCroneDeleteNotificationsWebApp";
            await _context.ExecuteAsync(query, new DynamicParameters(), CommandType.StoredProcedure);
        }

        public async Task DeleteOldLiveCoordinates()
        {
            var query = "spDeleteOldLiveTrackingMapRecords";
            await _context.ExecuteAsync(query, new DynamicParameters(), CommandType.StoredProcedure);
        }
    }
}
