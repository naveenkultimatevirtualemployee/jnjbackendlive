using JNJServices.API.Hubs;
using JNJServices.Business.Abstracts;
using JNJServices.Utility.Helper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace JNJServices.Tests.Hubs
{
    public class TestableNotificationHub : NotificationHub
    {
        public TestableNotificationHub(
        IReservationAssignmentsService reservationAssignmentService,
        ITimeZoneConverter timeZoneConverter,
        ILogger<NotificationHub> logger,
        IChatService chatService,
        INotificationQueue notificationQueue,
        IRoomConnectionMapping roomConnection,
        IConnectionMapping connectionMapping)
        : base(reservationAssignmentService, timeZoneConverter, logger, chatService, notificationQueue, roomConnection, connectionMapping)
        {
        }

        public void SetClients(IHubCallerClients clients) => base.Clients = clients;
        public void SetContext(HubCallerContext context) => base.Context = context;
    }
}
