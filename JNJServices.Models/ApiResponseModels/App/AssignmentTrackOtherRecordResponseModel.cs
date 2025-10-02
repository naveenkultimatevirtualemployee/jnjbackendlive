namespace JNJServices.Models.ApiResponseModels.App
{
    public class AssignmentTrackOtherRecordResponseModel
    {
        public int WaitingID { get; set; }
        public int ContractorID { get; set; }
        public int ReservationAssignmentTrackingID { get; set; }
        public int ReservationsAssignmentsID { get; set; }
        public string? StartLatitudeLongitude { get; set; }
        public DateTime? StartDateandTime { get; set; }
        public string? EndLatitudeLongitude { get; set; }
        public DateTime? EndDateandTime { get; set; }
        public string? Comments { get; set; }
        public int ButtonID { get; set; }
        public string? TimeInterval { get; set; }
        public int IsManuallyEntered { get; set; }
    }
}
