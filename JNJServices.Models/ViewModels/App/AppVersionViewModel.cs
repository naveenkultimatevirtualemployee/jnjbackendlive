using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.App
{
    public class AppVersionViewModel
    {
        [Required]
        public string Platform { get; set; } = string.Empty;
        [Required]
        public string CurrentVersion { get; set; } = string.Empty;
    }
}
