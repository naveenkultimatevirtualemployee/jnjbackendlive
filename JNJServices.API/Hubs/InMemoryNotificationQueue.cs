using JNJServices.Models.CommonModels;
using System.Threading.Channels;

namespace JNJServices.API.Hubs
{
    public interface INotificationQueue
    {
        ValueTask EnqueueAsync(WebNotificationModel message, AppNotificationModel appNotification);
        ValueTask<(WebNotificationModel, AppNotificationModel)> DequeueAsync(CancellationToken cancellationToken);
    }

    public class InMemoryNotificationQueue : INotificationQueue
    {
        private readonly Channel<(WebNotificationModel, AppNotificationModel)> _queue;

        public InMemoryNotificationQueue()
        {
            _queue = Channel.CreateUnbounded<(WebNotificationModel, AppNotificationModel)>();
        }

        public async ValueTask EnqueueAsync(WebNotificationModel message, AppNotificationModel appNotification)
        {
            await _queue.Writer.WriteAsync((message, appNotification));
        }

        public async ValueTask<(WebNotificationModel, AppNotificationModel)> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
