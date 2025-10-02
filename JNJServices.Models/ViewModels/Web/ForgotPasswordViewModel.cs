using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public string? EmailID { get; set; }

        [Required]
        [MinLength(5)]
        public string? oldPassword { get; set; }

        [Required]
        [MinLength(5)]
        public string? newPassword { get; set; }

    }
}
