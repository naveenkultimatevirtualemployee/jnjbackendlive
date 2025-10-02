namespace JNJServices.Models.CommonModels
{
    public class InsertNotificationLog
    {
        public InsertNotificationLog()
        {
            data = new Dictionary<string, string>();
        }

        public int UserID { get; set; }
        public int UserType { get; set; }
        public int ReservationsAssignmentsID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public Dictionary<string, string> data { get; set; }
        public DateTime NotificationDateTime { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public string FcmToken { get; set; } = string.Empty;
        public string webFcmToken { get; set; } = string.Empty;
    }
}
