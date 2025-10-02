namespace JNJServices.Models.ApiResponseModels.Web
{
	public class UserLoginWebResponseModel
	{
		public string? UserID { get; set; }
		public int UserNum { get; set; }
		public string? UserName { get; set; }
		public int isFirstTimeUser { get; set; }
		public int groupnum { get; set; }
		public string? GroupName { get; set; }
		public string? AuthToken { get; set; }
		public int IsFcmUpdated { get; set; }
	}
}
