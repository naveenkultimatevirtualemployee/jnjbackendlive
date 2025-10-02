using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ContractorClaimantResetPasswordWebViewModel
    {
        [Required]
        public int? Type { get; set; }
        [Required]
        public int? UserID { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
