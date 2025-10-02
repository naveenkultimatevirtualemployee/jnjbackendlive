using JNJServices.Models.DbResponseModels;
using JNJServices.Models.Entities.Views;

namespace JNJServices.Models.ApiResponseModels.App
{
    public class BaseAssignmentTrackingResponseModel : BaseAssignmentTrackingAcceptResponseModel
    {
        public BaseAssignmentTrackingResponseModel()
        {
            WaitingRecords = new List<AssignmentTrackOtherRecordResponseModel>();
            WaitingRecordsList = new List<AssignmentTrackOtherRecordResponseModel>();
            ContractorVehicles = new List<vwContractorVehicleSearch>();
            ContractorDrivers = new List<vwContractorDriversSearch>();
        }

        public int? AssignmentTrackingID { get; set; }
        public int? ReservationsAssignmentsID { get; set; }
        public int? ReservationID { get; set; }
        public string? notes { get; set; }
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
        public int? Quantity { get; set; }
        public int? EstimatedMinutes { get; set; }
        public string? DOFacilityName2 { get; set; }
        public string? DOFacilityName { get; set; }
        public string? ClaimantName { get; set; }
        public string? rsvattcode { get; set; }
        public string? HmPhone { get; set; }
        public string? assgncode { get; set; }
        public string Language { get; set; } = string.Empty;
        public string ClaimantLanguage1 { get; set; } = string.Empty;
        public string ClaimantLanguage2 { get; set; } = string.Empty;
        public string Contractor { get; set; } = string.Empty;
        public string? contractorhomephone { get; set; }
        public decimal? DeadMiles { get; set; }
        public decimal? TravellingMiles { get; set; }
        public string? ImageUrl { get; set; }
        public int ContractorMetricsFlag { get; set; }
        public string RSVPRCode { get; set; } = string.Empty;
        public string ProcedureDescription { get; set; } = string.Empty;
        public List<AssignmentTrackOtherRecordResponseModel> WaitingRecords { get; set; }
        public List<AssignmentTrackOtherRecordResponseModel> WaitingRecordsList { get; set; }
        public IEnumerable<vwContractorVehicleSearch> ContractorVehicles { get; set; }
        public IEnumerable<vwContractorDriversSearch> ContractorDrivers { get; set; }
    }
}
