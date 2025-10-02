namespace JNJServices.Models.CommonModels
{
    public class ChatMessage
    {
        public int MessageId { get; set; }
        public int ChatRoomId { get; set; }
        public string MessageContent { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public int Type { get; set; }
        public int SenderId { get; set; }
        public string SenderInitials { get; set; } = string.Empty;
        public string SenderFullName { get; set; } = string.Empty;
    }

}
