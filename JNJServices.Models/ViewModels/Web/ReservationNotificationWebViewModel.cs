namespace JNJServices.Models.ViewModels.Web
{
    public class ReservationNotificationWebViewModel
    {
        public int? Type { get; set; }
        public string? ButtonStatus { get; set; }
        public int? ReservationID { get; set; }
        public int ContractorID { get; set; }
        public int? ReservationAssignmentID { get; set; }
        public string? NotificationTitle { get; set; }
    }
}
