using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ContractorIDWebViewModel
    {
        [Required]
        public int? ContractorID { get; set; }
    }
}
