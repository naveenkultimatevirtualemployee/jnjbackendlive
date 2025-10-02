namespace JNJServices.Models.DbResponseModels
{
    public class ContractorAssignedResponseModel : BaseContractorResponseModel
    {
        public int ReservationsAssignmentsID { get; set; }
        public string FcmToken { get; set; } = string.Empty;
        public string DeviceID { get; set; } = string.Empty;
        public DateTime PickupTime { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public int ContractorID { get; set; }
    }
}
