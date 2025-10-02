namespace JNJServices.Models.ViewModels.App
{
	public class TokenAndNotificationUpdateRequest
	{
		public string Token { get; set; } = string.Empty;           // New bearer token
		public int UserId { get; set; }             // User ID (Contractor or Claimant)
		public int NotificationStatus { get; set; } // Notification status update
		public int Type { get; set; }              // Type: 1 = Contractor, 2 = Claimant
	}
}
