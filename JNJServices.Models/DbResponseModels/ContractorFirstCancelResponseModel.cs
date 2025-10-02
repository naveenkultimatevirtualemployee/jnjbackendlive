namespace JNJServices.Models.DbResponseModels
{
    public class ContractorFirstAcceptCancelResponseModel
    {
        public int ReservationsAssignmentsID { get; set; }
        public string ResAsgnCode { get; set; } = string.Empty;
        public string AssgnNum { get; set; } = string.Empty;
        public string ResTripType { get; set; } = string.Empty;
        public int ContractorID { get; set; }
        public int ClaimantID { get; set; }
        public string ASSGNCode { get; set; } = string.Empty;
        public string RSVATTCode { get; set; } = string.Empty;
        public string PUAddress1 { get; set; } = string.Empty;
        public string PUAddress2 { get; set; } = string.Empty;
        public string DOAddress1 { get; set; } = string.Empty;
        public string DOAddress2 { get; set; } = string.Empty;
        public string FcmToken { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public DateTime ReservationTime { get; set; }
        public DateTime? PickupTime { get; set; }
        public DateTime NotificationDateTime { get; set; }
        public string NotificationType { get; set; } = string.Empty;
    }
}
