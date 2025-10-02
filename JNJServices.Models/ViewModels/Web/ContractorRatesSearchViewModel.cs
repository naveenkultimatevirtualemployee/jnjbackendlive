using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ContractorRatesSearchViewModel
    {
        [Required]
        public string? RateCode { get; set; }
        public string? StateCode { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public string? EffectiveDate { get; set; }
        public int? Inactiveflag { get; set; }
    }
}
