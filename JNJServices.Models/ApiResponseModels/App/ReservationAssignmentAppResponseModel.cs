using JNJServices.Models.Entities.Views;
using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ApiResponseModels.App
{
    public class ReservationAssignmentAppResponseModel
    {
        public ReservationAssignmentAppResponseModel()
        {
            ContractorVehicles = new List<vwContractorVehicleSearch>();
            ContractorDrivers = new List<vwContractorDriversSearch>();

        }
        public int reservationid { get; set; }
        public int? contractorid { get; set; }
        public int ReservationsAssignmentsID { get; set; }
        public string? AssgnNum { get; set; }

        public int WillCallFlag { get; set; }
        [StringLength(20)]
        public string? rsvattcode { get; set; }
        public decimal? quantity { get; set; }
        [StringLength(20)]
        public string? assgncode { get; set; }
        [StringLength(20)]
        public string? langucode { get; set; }
        public int? inactiveflag { get; set; }
        public int? MetricsEnteredFlag { get; set; }
        public int? totaltime { get; set; }
        [StringLength(500)]
        public string? notes { get; set; }
        public int? driverconfirmedflag { get; set; }
        public DateTime? PickupTimeSort { get; set; }
        [StringLength(8)]
        public string? PickupTime { get; set; }
        [StringLength(12)]
        public string? PickupTimewZone { get; set; }
        [StringLength(123)]
        public string? contractor { get; set; }
        [StringLength(25)]
        public string? contractorhomephone { get; set; }
        [StringLength(25)]
        public string? contractorcellphone { get; set; }
        public string? ResTripType { get; set; }
        public string? ResAsgnCode { get; set; }
        public string? Language { get; set; }
        public DateTime? ReservationDate { get; set; }
        public DateTime? ReservationTime { get; set; }
        public int? claimantid { get; set; }
        public string? claimantName { get; set; }
        public int? customerid { get; set; }
        public string? customerName { get; set; }
        public int? EstimatedMinutes { get; set; }
        public int? relatedreservationid { get; set; }
        public string? RSVACCode { get; set; }
        public string? ActionCode { get; set; }
        public DateTime? CanceledDate { get; set; }
        public DateTime CanceledTime { get; set; }
        public string? PUFacilityName { get; set; }
        public string? PUFacilityName2 { get; set; }
        public string? PUCity { get; set; }
        public string? DOFacilityName { get; set; }
        public string? DOFacilityName2 { get; set; }
        public string? DOAddress1 { get; set; }
        public string? DOAddress2 { get; set; }
        public string? DOCity { get; set; }
        public string? PUAddress1 { get; set; }
        public string? PUAddress2 { get; set; }
        public string? HmPhone { get; set; }
        public int IsFeedbackComplete { get; set; }
        public string? RSVCXCode { get; set; }
        public string? RSVCXdescription { get; set; }
        public int TotalCount { get; set; }
        public int IsJobStarted { get; set; }
        public string? LANGUCode1 { get; set; }
        public string? LANGUCode2 { get; set; }
        public string? ClaimantLanguage1 { get; set; }
        public string? ClaimantLanguage2 { get; set; }
        public string? AssignmentJobStatus { get; set; }
        public string? Cost { get; set; }
        public double? PULatitude { get; set; }
        public double? PULongitude { get; set; }
        public double? DOLatitude { get; set; }
        public double? DOLongitude { get; set; }
        public int ContractorMetricsFlag { get; set; }
        public int childAssignmentCount { get; set; }
        public string ContractorProfileImage { get; set; } = string.Empty;
        public string TransType { get; set; } = string.Empty;
        public string ContractorGender { get; set; } = string.Empty;
        public string RSVPRCode { get; set; } = string.Empty;
        public string ProcedureDescription { get; set; } = string.Empty;
        public DateTime? ReservationCanceledDate { get; set; }
        public DateTime? ReservationCanceledTime { get; set; }
        public string? ReservationRSVCXdescription { get; set; }
        public int isEnableForMetrics { get; set; }
        public int waittimeapprovedflag { get; set; }
        public IEnumerable<vwContractorVehicleSearch> ContractorVehicles { get; set; }
        public IEnumerable<vwContractorDriversSearch> ContractorDrivers { get; set; }
    }
}
