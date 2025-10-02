namespace JNJServices.Models.CommonModels
{
	public class ChatMessageViewModel
	{
		public string? RoomId { get; set; }
		public int Type { get; set; }
		public int SenderId { get; set; }
		public string MessageContent { get; set; } = string.Empty;
		public DateTime SendAt { get; set; }
	}
}
