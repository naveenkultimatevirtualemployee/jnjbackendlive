namespace JNJServices.Models.DbResponseModels
{
    public class CancelResponseModel : BaseAcceptResponseModel
    {
        public string FcmToken { get; set; } = string.Empty;
        public string DeviceID { get; set; } = string.Empty;
        public int? ContractorID { get; set; }
        public DateTime NotificationDateTime { get; set; }
    }
}
