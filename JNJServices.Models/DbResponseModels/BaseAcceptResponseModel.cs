namespace JNJServices.Models.DbResponseModels
{
    public class BaseAcceptResponseModel : BaseAssignmentTrackingAcceptResponseModel
    {
        public string ResTripType { get; set; } = string.Empty;
        public string ASSGNCode { get; set; } = string.Empty;
        public string RSVATTCode { get; set; } = string.Empty;
    }

    public class BaseAssignmentTrackingAcceptResponseModel
    {
        public string ResAsgnCode { get; set; } = string.Empty;
        public string AssgnNum { get; set; } = string.Empty;
        public int ClaimantID { get; set; }
        public string PUAddress1 { get; set; } = string.Empty;
        public string PUAddress2 { get; set; } = string.Empty;
        public string DOAddress1 { get; set; } = string.Empty;
        public string DOAddress2 { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public DateTime ReservationTime { get; set; }
        public DateTime? PickupTime { get; set; }
    }
}
