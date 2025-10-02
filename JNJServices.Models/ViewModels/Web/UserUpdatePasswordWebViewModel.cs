using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class UserUpdatePasswordWebViewModel
    {
        [Required]
        public string UserID { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
