using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class LogoutUserWebViewModel
    {
        [Required]
        public string? UserID { get; set; }
    }
}
