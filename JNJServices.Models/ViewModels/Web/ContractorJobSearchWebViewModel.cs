using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ContractorJobSearchWebViewModel
    {
        [Required]
        public int? ReservationID { get; set; }
        [Required]
        public int? ReservationAssignmentsID { get; set; }
    }
}
