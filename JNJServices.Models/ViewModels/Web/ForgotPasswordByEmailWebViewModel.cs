using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ForgotPasswordByEmailWebViewModel
    {
        [Required]
        public string? Email { get; set; }
    }
}
