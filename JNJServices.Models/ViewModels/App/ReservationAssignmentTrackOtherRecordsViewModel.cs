using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.App
{
    public class ReservationAssignmentTrackOtherRecordsViewModel
    {
        [Required]
        public int? ButtonID { get; set; }
        public int? ReservationAssignmentTrackingID { get; set; }
        public int? WaitingID { get; set; }
        [Required]
        public int? ReservationsAssignmentsID { get; set; }
        public int? ContractorID { get; set; }
        [Required]
        public string? DriverLatitudeLongitude { get; set; }
        [Required]
        public DateTime? WaitingDateandTime { get; set; }
        public string? Comments { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedUserID { get; set; }
    }
}
