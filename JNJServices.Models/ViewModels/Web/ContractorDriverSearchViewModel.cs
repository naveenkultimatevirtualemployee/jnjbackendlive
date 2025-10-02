using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ContractorDriverSearchViewModel
    {
        [Required]
        public int? ContractorID { get; set; }
        public int? IsPrimary { get; set; }
    }
}
