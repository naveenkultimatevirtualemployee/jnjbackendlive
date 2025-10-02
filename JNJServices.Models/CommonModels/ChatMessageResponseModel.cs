namespace JNJServices.Models.CommonModels
{
    public class ChatMessageResponseModel
    {
        public ChatMessageResponseModel()
        {
            ChatRoomDetails = new ChatRoom();
        }
        public string? RoomId { get; set; }
        public int Type { get; set; }
        public int SenderId { get; set; }
        public string SenderInitials { get; set; } = string.Empty;
        public string SenderFullName { get; set; } = string.Empty;
        public string MessageContent { get; set; } = string.Empty;
        public ChatRoom? ChatRoomDetails { get; set; }
        public DateTime SentAt { get; set; }
        public int ChatRoomId { get; set; }
    }
}
