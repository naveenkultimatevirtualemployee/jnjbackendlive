using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ContractorRatesDetailSearchViewModel
    {
        [Required]
        public int? ContractorRatesID { get; set; }
        public string? ACCTGCode { get; set; }
        public string? TransType { get; set; }
        public string? Language { get; set; }
        public string? LOB { get; set; }
    }
}
