using JNJServices.Models.CommonModels;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;

namespace JNJServices.API.Helper
{
    public interface INotificationHelper
    {
        Task SendNotificationClaimantContractorAccept(ReservationNotificationAppViewModel notificationResponse);

        Task SendNotificationContractorCancel(ReservationNotificationAppViewModel model);

        Task SendNotificationDriverTraking(ClaimantTrakingAppViewModel notificationResponse);

        Task SendWebNotificationContractorNotAssigned();

        Task SendContractorNotification();

        Task SendAssignmentjobRequestAndNotification();

        Task SendContractorWebJobSearch(int AssignmentID);

        Task SendNotificationWebForceRequest(ReservationNotificationWebViewModel model);

        Task SendNotificationAsync(NotificationMessageAppViewModel model);

        Task SendAppPushNotifications(AppNotificationModel inputValues);

        Task SendWebPushNotifications(WebNotificationModel inputValues);

        Task SendPreferredContractorNotFound(int AssignmentID);

    }
}
