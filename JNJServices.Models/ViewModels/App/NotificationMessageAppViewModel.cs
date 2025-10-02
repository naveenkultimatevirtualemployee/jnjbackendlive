namespace JNJServices.Models.ViewModels.App
{
    public class NotificationMessageAppViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string FcmToken { get; set; } = string.Empty;
        public int ReservationsAssignmentsID { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public DateTime ReservationTime { get; set; }
    }
}
