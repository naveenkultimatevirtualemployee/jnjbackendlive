namespace JNJServices.Models.CommonModels
{
    public class AppNotificationModel
    {
        public AppNotificationModel()
        {
            FcmToken = string.Empty;
            Title = string.Empty;
            Body = string.Empty;
            data = new Dictionary<string, string>();
        }

        public string FcmToken { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> data { get; set; }
    }
}
