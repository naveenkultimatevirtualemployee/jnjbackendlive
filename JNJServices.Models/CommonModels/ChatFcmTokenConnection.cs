namespace JNJServices.Models.CommonModels
{
    public class ChatFcmTokenConnection
    {
        public string FcmToken { get; set; } = string.Empty;
        public string ConnectionID { get; set; } = string.Empty; // or whatever the second column is
        public int Type { get; set; } // or whatever the second column is
    }
}
