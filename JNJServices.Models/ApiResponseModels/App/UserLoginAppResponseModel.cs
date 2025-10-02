using System.Text.Json.Serialization;

namespace JNJServices.Models.ApiResponseModels.App
{
	public class UserLoginAppResponseModel
	{
		public UserLoginAppResponseModel()
		{
			Type = string.Empty;
			PhoneNo = string.Empty;
		}

		public string Type { get; set; }
		public int? UserID { get; set; }
		public string? UserName { get; set; }
		public int isOTPVerified { get; set; }
		public string PhoneNo { get; set; }
		public string? AuthToken { get; set; }
		public int IsFcmUpdated { get; set; }
		public int NotificationStatus { get; set; }
		[JsonIgnore]
		public string? DeviceID { get; set; }
		[JsonIgnore]
		public string? FcmToken { get; set; }
	}
}
