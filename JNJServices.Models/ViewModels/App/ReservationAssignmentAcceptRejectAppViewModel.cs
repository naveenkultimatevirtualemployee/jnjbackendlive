using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.App
{
    public class ReservationAssignmentAcceptRejectAppViewModel
    {
        [Required]
        public int? Type { get; set; }
        public int? ReservationID { get; set; }
        public int? AssignmentID { get; set; }
        public int? ContractorID { get; set; }
        [Required]
        public string? ButtonStatus { get; set; }
        public string? Notes { get; set; }
    }
}
