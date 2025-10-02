using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ClaimantIDWebViewModel
    {
        [Required]
        public int? ClaimantID { get; set; }
    }
}
