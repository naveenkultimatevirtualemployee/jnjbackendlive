using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.App
{
    public class SaveAssignmentMetricsViewModel
    {
        [Required]
        public int ReservationsAssignmentsID { get; set; }
        [Required]
        public string? ACCTGCode { get; set; }
        [Required]
        public string? Qty { get; set; }
        public string? Notes { get; set; }
    }
}
