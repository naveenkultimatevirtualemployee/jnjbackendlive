namespace JNJServices.Models.ApiResponseModels.App
{
    public class TrackAssignmentByIDResponseModel : BaseAssignmentTrackingResponseModel
    {
        public string? ForcedJobCompletionNotes { get; set; }
        public string TransType { get; set; } = string.Empty;
        public string? DeadMileImageUrl { get; set; }
        public string? AssignmentJobStatus { get; set; }
    }
}
