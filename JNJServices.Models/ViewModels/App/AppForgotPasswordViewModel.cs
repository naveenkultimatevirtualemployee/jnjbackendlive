using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.App
{
    public class AppForgotPasswordViewModel
    {
        [Required]
        public int? Type { get; set; }
        [Required]
        public int? UserID { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
