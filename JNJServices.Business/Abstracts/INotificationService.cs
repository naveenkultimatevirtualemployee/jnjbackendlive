using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.CommonModels;
using JNJServices.Models.DbResponseModels;

namespace JNJServices.Business.Abstracts
{
    public interface INotificationService
    {
        Task<IEnumerable<AppNotificationsResponseModel>> GetNotificationsAsync(AppTokenDetails tokenClaims, int? page, int? limit);
        Task<int> DeleteNotificationAsync(AppTokenDetails tokenClaims);
        Task<IEnumerable<WebNotificationResponseModel>> GetUserWebNotifications(string claimValue, int? Page, int? Limit);
        Task<int> DeleteUserWebNotifications(string UserID);
        Task<IEnumerable<ContractorFirstAcceptCancelResponseModel>> ClaimantAcceptCancel(int ReservationID);
        Task<IEnumerable<ContractorFirstAcceptCancelResponseModel>> ClaimantAcceptContractornotfoundLogic(int ReservationID);
        Task<IEnumerable<ContractorFirstAcceptCancelResponseModel>> ContractorFirstCancelLogic(int ReservationAssignmentID);
        Task<IEnumerable<ContractorFirstAcceptCancelResponseModel>> ContractorFirstCancelDataNotFound(int ReservationAssignmentID);
        Task<IEnumerable<ContractorFirstAcceptCancelResponseModel>> ContractorAcceptLogic(int ReservationAssignmentID);
        Task<IEnumerable<ContractorFirstAcceptCancelResponseModel>> ContractorCancelLogic(int ReservationAssignmentID);
        Task<IEnumerable<ClaimantCancelResponseModel>> ClaimantCancelLogic(int ReservationID);
        Task<IEnumerable<ClaimantCancelResponseModel>> ClaimantCancelNodataFound(int ReservationID);
        Task<IEnumerable<ContractorNotificationResponseModel>> ClaimantTraking(int ReservationAssignmentID);
        Task<IEnumerable<ContractorNotAssignedResponseModel>> WebContractorNotAssignedLogic();
        Task<IEnumerable<ContractorAssignedResponseModel>> WebContractorAssignedLogic();
        Task<IEnumerable<AssignmentJobRequestResponseModel>> AssignmentjobRequestaAndNotification();
        Task<IEnumerable<ContractorWebJobSearchRespnseModel>> SendContractorWebJobSearchLogic(int AssignmentID);
        Task<IEnumerable<ContractorWebJobSearchRespnseModel>> SendContractorWebJobSearchLogicNoDataFound(int AssignmentID);
        Task InsertNotificationLog(InsertNotificationLog item);
        Task InsertNotificationLogWeb(InsertNotificationLog item);
        Task InsertNotificationTriggerLogWeb(InsertNotificationLog item);
        Task<List<string>> GetDistinctUserFcmTokensAsync();
        Task<List<string>> GetDistinctUserUserIDAsync();
        Task DeleteInactiveChatRoomsAsync();
        Task DeleteNotifications();
        Task DeleteOldLiveCoordinates();
    }
}
