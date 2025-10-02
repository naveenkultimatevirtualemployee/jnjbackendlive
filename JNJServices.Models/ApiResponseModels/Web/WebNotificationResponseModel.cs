namespace JNJServices.Models.ApiResponseModels.Web
{
	public class WebNotificationResponseModel
	{
		public WebNotificationResponseModel()
		{
			Title = string.Empty;
			Body = string.Empty;
			Data = string.Empty;
			NotificationType = string.Empty;
			AssgnNum = string.Empty;
		}
		public int NotificationID { get; set; }
		public string? ReferenceID { get; set; }
		public int ReservationsAssignmentsID { get; set; }
		public int ReservationID { get; set; }
		public string AssgnNum { get; set; }
		public int UserID { get; set; }
		public string Title { get; set; }
		public string Body { get; set; }
		public string Data { get; set; }
		public DateTime SentDate { get; set; }
		public int WebReadStatus { get; set; }
		public string NotificationType { get; set; }
		public int CreatedBy { get; set; }
		public DateTime CreatedDate { get; set; }
		public int TotalCount { get; set; }

	}
}
