using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ContractorVehicleSearchWebViewModel
    {
        [Required]
        public int? ContractorID { get; set; }
        public int? IsPrimary { get; set; }
    }
}
