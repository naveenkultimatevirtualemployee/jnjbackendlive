using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class UserLoginWebViewModel
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
        public string? UserDeviceID { get; set; }
        public string? UserFcmToken { get; set; }
    }
}
