using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.App
{
    public class ReservationAssignmentTrackingViewModel
    {
        [Required]
        public int? ButtonID { get; set; }
        public int? AssignmentTrackingID { get; set; }
        [Required]
        public int? ReservationAssignmentID { get; set; }
        public int? ClaimantID { get; set; }
        public int? ContractorID { get; set; }
        [Required]
        public string? ButtonStatus { get; set; }
        [Required]
        public string? DriverLatitudeLongitude { get; set; }
        [Required]
        public DateTime? TravelledDateandTime { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? CreateUserID { get; set; }
        public decimal? DeadMiles { get; set; }
        public decimal? TravellingMiles { get; set; }
        // New property for image URL
        public string? ImageUrl { get; set; }
        public string? DeadMileImageUrl { get; set; }
    }
}
