namespace JNJServices.Models.CommonModels
{
    public class WebNotificationModel
    {
        public WebNotificationModel()
        {
            FcmToken = new List<string>();
            data = new Dictionary<string, string>();
        }

        public List<string> FcmToken { get; set; }
        public Dictionary<string, string> data { get; set; }
    }
}
