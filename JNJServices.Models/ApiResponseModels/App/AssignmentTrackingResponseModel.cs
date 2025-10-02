namespace JNJServices.Models.ApiResponseModels.App
{
    public class AssignmentTrackingResponseModel
    {
        public AssignmentTrackingResponseModel()
        {
            WaitingRecordsList = new List<AssignmentTrackOtherRecordResponseModel>();
        }
        public int? AssignmentTrackingID { get; set; }
        public int? ReservationsAssignmentsID { get; set; }
        public int? ClaimantID { get; set; }
        public int? ContractorID { get; set; }
        public string? StartButtonStatus { get; set; }
        public string? StartDriverLatitudeLongitude { get; set; }
        public DateTime? StartDateandTime { get; set; }
        public string? ReachedButtonStatus { get; set; }
        public string? ReachedDriverLatitudeLongitude { get; set; }
        public DateTime? ReachedDateandTime { get; set; }
        public string? ClaimantPickedupButtonStatus { get; set; }
        public string? ClaimantPickedupLatitudeLongitude { get; set; }
        public DateTime? ClaimantPickedupDateandTime { get; set; }
        public string? TripEndButtonStatus { get; set; }
        public string? TripEndPickedupLatitudeLongitude { get; set; }
        public DateTime? TripEndPickedupDateandTime { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? CreateUserID { get; set; }
        public int? CurrentButtonID { get; set; }
        public List<AssignmentTrackOtherRecordResponseModel> WaitingRecordsList { get; set; }
    }
}
