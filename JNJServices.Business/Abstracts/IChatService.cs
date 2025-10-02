using JNJServices.Models.CommonModels;

namespace JNJServices.Business.Abstracts
{
    public interface IChatService
    {
        Task<(int, int, string, string)> InsertOrUpdateChatRoom(ChatRoomViewModel model);
        Task<(int ChatRoomId, int ResponseCode, string Message, bool WebReadStatus)> UpsertChatRoomMessageAsync(ChatMessageViewModel model);
        Task<(List<ChatRoom>, int TotalCount)> GetChatListAsync(ChatListViewModel model);
        Task<(List<ChatMessage>, int totalCount)> GetChatMessagesAsync(ChatMessageSearchViewModel model);
        Task<List<string>> GetConnectionKeysForRoom(string roomName);
        Task<List<ChatRoomsActiveResponse>> GetActiveChatRoomsAsync(int? contractorId = null);
        Task<List<ChatFcmTokenConnection>> GetAllConnectionsForRoomFcmTokenAsync(string roomId, int type, string senderId);
    }
}
