using JNJServices.Models.ApiResponseModels;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Helper
{
    public class CronHelper
    {
        private readonly NotificationHelper _notificationHelper;
        public CronHelper(IServiceProvider serviceProvider)
        {
            _notificationHelper = CronServiceScopeHelper.CreateScope(serviceProvider).ServiceProvider.GetRequiredService<NotificationHelper>();
        }

        public ResponseModel NotificationContractorAssignment()
        {
            _notificationHelper.SendContractorNotification().ConfigureAwait(false);

            ResponseModel response = new ResponseModel();
            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;

            return response;
        }

        public ResponseModel NotificationContractorNotAssigned()
        {
            _notificationHelper.SendWebNotificationContractorNotAssigned().ConfigureAwait(false);

            ResponseModel response = new ResponseModel();
            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;

            return response;
        }

        //every 1 hour this will run and and automatically Assign Request to contractor and Send Reminder
        public ResponseModel ContractorAssignmentjobRequestaAndNotification()
        {
            _notificationHelper.SendAssignmentjobRequestAndNotification().ConfigureAwait(false);

            ResponseModel response = new ResponseModel();
            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;

            return response;
        }

        public ResponseModel CronDeleteInactiveChatRooms()
        {
            _notificationHelper.DeleteInactiveChatRooms().ConfigureAwait(false);

            ResponseModel response = new ResponseModel();
            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;

            return response;
        }

        public ResponseModel CroneDeleteNotification()
        {
            _notificationHelper.DeleteNotificationCrone().ConfigureAwait(false);

            ResponseModel response = new ResponseModel();
            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;

            return response;
        }
        public ResponseModel CroneDeleteLiveCoordinates()
        {
            _notificationHelper.DeleteOldLiveCoordinatesCrone().ConfigureAwait(false);

            ResponseModel response = new ResponseModel();
            response.status = ResponseStatus.TRUE;
            response.statusMessage = ResponseMessage.SUCCESS;

            return response;
        }
    }

    public static class CronServiceScopeHelper
    {
        public static IServiceScope CreateScope(this IServiceProvider provider)
        {
            return provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }
    }
}
