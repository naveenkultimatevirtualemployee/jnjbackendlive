namespace JNJServices.Models.ViewModels.App
{
    public class SaveLiveTrackingCoordinatesViewModel
    {
        public int ReservationsAssignmentsID { get; set; }
        public int AssignmentTrackingID { get; set; }
        public string? LatitudeLongitude { get; set; }
        public DateTime TrackingDateTime { get; set; }
        public int isDeadMile { get; set; }
    }
}
