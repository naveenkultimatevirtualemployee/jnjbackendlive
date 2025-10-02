using JNJServices.API.Helper;

namespace JNJServices.API.Hubs
{
    public class NotificationWorker : BackgroundService
    {
        private readonly INotificationQueue _notificationQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationWorker> _logger;

        public NotificationWorker(INotificationQueue queue, IServiceScopeFactory scopeFactory, ILogger<NotificationWorker> logger)
        {
            _notificationQueue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Worker running...");

            while (!stoppingToken.IsCancellationRequested)
            {
                var (webmessage, appMessage) = await _notificationQueue.DequeueAsync(stoppingToken);

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var notificationHelper = scope.ServiceProvider.GetRequiredService<INotificationHelper>();

                    await notificationHelper.SendWebPushNotifications(webmessage);
                    await notificationHelper.SendAppPushNotifications(appMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send push notifications.");
                }
            }
        }
    }
}