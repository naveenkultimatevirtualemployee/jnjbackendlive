namespace JNJServices.Models.DbResponseModels
{
    public class ContractorNotificationResponseModel : BaseAssignmentResponseModel
    {
        public int ReservationsAssignmentsID { get; set; }
        public int ReservationID { get; set; }
        public string Contractor { get; set; } = string.Empty;
        public string RSVPRCode { get; set; } = string.Empty;
    }
}
