namespace JNJServices.Models.ApiResponseModels.App
{
	public class AppNotificationsResponseModel
	{
		public int NotificationID { get; set; }
		public string? ReferenceID { get; set; }
		public string? AssgnNum { get; set; }
		public int UserID { get; set; }
		public string? Title { get; set; }
		public string? Body { get; set; }
		public string? Data { get; set; }
		public DateTime SentDate { get; set; }
		public int ReadStatus { get; set; }
		public string? NotificationType { get; set; }
		public int CreatedBy { get; set; }
		public DateTime CreatedDate { get; set; }
		public int TotalCount { get; set; }
	}
}
