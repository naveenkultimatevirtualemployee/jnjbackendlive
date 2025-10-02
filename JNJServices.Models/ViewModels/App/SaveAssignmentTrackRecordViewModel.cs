namespace JNJServices.Models.ViewModels.App
{
    public class SaveAssignmentTrackRecordViewModel
    {
        public int ContractorID { get; set; }
        public int ReservationsAssignmentsID { get; set; }
        public int ReservationAssignmentTrackingID { get; set; }
        public string? TimeInterval { get; set; } // Assuming TimeInterval is in the format HH:mm:ss
        public string? Comments { get; set; }
    }
}
