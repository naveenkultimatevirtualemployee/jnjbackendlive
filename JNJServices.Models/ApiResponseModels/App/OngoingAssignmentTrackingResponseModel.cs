namespace JNJServices.Models.ApiResponseModels.App
{
    public class OngoingAssignmentTrackingResponseModel : BaseAssignmentTrackingResponseModel
    {
        public string? ContractorCellPhone { get; set; }
        public string RSVPRCode { get; set; } = string.Empty;
        public string ProcedureDescription { get; set; } = string.Empty;
        public int waittimeapprovedflag { get; set; }
    }
}
