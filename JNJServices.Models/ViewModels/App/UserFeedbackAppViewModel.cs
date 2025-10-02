using JNJServices.Models.Entities;

namespace JNJServices.Models.ViewModels.App
{
    public class UserFeedbackAppViewModel : UsersFeedback
    {
        public int? oldCreatedUserID { get; set; }
    }
}
