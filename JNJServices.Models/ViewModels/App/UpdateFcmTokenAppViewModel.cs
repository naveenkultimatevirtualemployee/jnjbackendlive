using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.App
{
    public class UpdateFcmTokenAppViewModel
    {
        [Required]
        public int? Type { get; set; }
        [Required]
        public int? UserID { get; set; }
        public string? DeviceID { get; set; }
        public string? FcmToken { get; set; }
    }
}
