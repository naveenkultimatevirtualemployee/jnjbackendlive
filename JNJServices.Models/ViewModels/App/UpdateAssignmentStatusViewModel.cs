using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.App
{
    public class UpdateAssignmentStatusViewModel
    {
        [Required]
        public int? Type { get; set; }
        public int? AssignmentID { get; set; }
        public int? ReservationID { get; set; }
        [Required]
        public string? ButtonStatus { get; set; }
        public string? CancelReason { get; set; }
        public int? CancelBy { get; set; }
        public DateTime? CancelDate { get; set; }
        public DateTime? CancelTime { get; set; }
        public string? Notes { get; set; }
    }
}
