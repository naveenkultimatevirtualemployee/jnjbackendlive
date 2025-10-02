using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class FcmUpdateViewModel
    {
        [Required]
        public string? UserID { get; set; }
        [Required]
        public string? UserDeviceID { get; set; }
        [Required]
        public string? UserFcmToken { get; set; }
    }
}
