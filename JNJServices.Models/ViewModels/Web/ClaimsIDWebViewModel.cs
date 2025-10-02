using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ClaimsIDWebViewModel
    {
        [Required]
        public int? ClaimID { get; set; }
    }
}
