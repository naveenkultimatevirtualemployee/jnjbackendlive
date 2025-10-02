using Microsoft.AspNetCore.SignalR;

namespace JNJServices.API.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("SocketUserID")?.Value;
        }
    }
}
