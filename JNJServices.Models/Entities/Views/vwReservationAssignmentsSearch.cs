using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities.Views
{
	[Table("vwReservationAssignSearch")]
	public class vwReservationAssignmentsSearch
	{
		public int reservationid { get; set; }
		public int? contractorid { get; set; }
		public int ReservationsAssignmentsID { get; set; }
		public int? linenum { get; set; }
		public string? BILCDECode { get; set; }
		public string? claimnumber { get; set; }
		public int WillCallFlag { get; set; }
		public string? rsvattcode { get; set; }
		public decimal? quantity { get; set; }
		public string? assgncode { get; set; }
		public string? langucode { get; set; }
		public int? inactiveflag { get; set; }
		public int? MetricsEnteredFlag { get; set; }
		public int? totaltime { get; set; }
		public string? ReferenceNumber { get; set; }
		public string? notes { get; set; }
		public int? driveremailflag { get; set; }
		public int? drivercalledflag { get; set; }
		public int? driverconfirmedflag { get; set; }
		public int driverreconfirmedflag { get; set; }
		public int? drivernaflag { get; set; }
		public int leftmessageflag { get; set; }
		public DateTime? PickupTimeSort { get; set; }
		public string? PickupTime { get; set; }
		public string? PickupTimewZone { get; set; }
		public string? AssgnNum { get; set; }
		public string? StartTime { get; set; }
		public string? EndTime { get; set; }
		public string? EarlyLatePickupTime { get; set; }
		public string? contractor { get; set; }
		public string? contycode { get; set; }
		public string? conctcode { get; set; }
		public string? contractorcontact { get; set; }
		public string? ContractorCompany { get; set; }
		public string? contractorhomephone { get; set; }
		public string? contractorworkphone { get; set; }
		public string? contractorcellphone { get; set; }
		public string? ContractorFax { get; set; }
		public string? ContractorEmail { get; set; }
		public string? concmcode { get; set; }
		public string? contractorintfid { get; set; }
		public string? contractorshortname { get; set; }
		public string? ResTripType { get; set; }
		public string? ResAssignCode { get; set; }
		public string? Language { get; set; }
		[DataType(DataType.Date)]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
		public string? ReservationDate { get; set; }
		public string? PickupTimeZone { get; set; }
		public int? ClaimID { get; set; }
		public int? claimantid { get; set; }
		public string? claimantName { get; set; }
		public int? customerid { get; set; }
		public string? customerName { get; set; }
		public int? EstimatedMinutes { get; set; }
		public string? RSVACCode { get; set; }
		public string? ActionCode { get; set; }
		public int? pickuplocid { get; set; }
		public string PUZipCode { get; set; } = string.Empty;
		public string CancelConfirm { get; set; } = string.Empty;
		public string CanceledBy { get; set; } = string.Empty;
		public DateTime? CanceledDate { get; set; }
		public int CanceledFlag { get; set; }
		public DateTime CanceledTime { get; set; }
		public string PUFacilityName { get; set; } = string.Empty;
		public string PUFacilityName2 { get; set; } = string.Empty;
		public string PUCity { get; set; } = string.Empty;
		public string DOFacilityName { get; set; } = string.Empty;
		public string DOFacilityName2 { get; set; } = string.Empty;
		public string DOAddress1 { get; set; } = string.Empty;
		public string DOAddress2 { get; set; } = string.Empty;
		public string DOZipCode { get; set; } = string.Empty;
		public string DOCity { get; set; } = string.Empty;
		public string PUAddress1 { get; set; } = string.Empty;
		public string PUAddress2 { get; set; } = string.Empty;
		public string HmPhone { get; set; } = string.Empty;
		public string height { get; set; } = string.Empty;
		public int? weight { get; set; }
		public DateTime? birthdate { get; set; }
		public int IsFeedbackComplete { get; set; }
		public string RSVCXCode { get; set; } = string.Empty;
		public string RSVCXdescription { get; set; } = string.Empty;
		public int IsAssignmentTracking { get; set; }
		public int? JobStatus { get; set; }
		public int? IsContractorFound { get; set; }
		public int? relatedreservationid { get; set; }
		public int TotalCount { get; set; }
		public string? AssignmentJobStatus { get; set; }
		public decimal? PULatitude { get; set; }
		public decimal? PULongitude { get; set; }
		public decimal? DOLatitude { get; set; }
		public decimal? DOLongitude { get; set; }
		public int? ContractorMetricsFlag { get; set; }
		public string? CustomerFL { get; set; }
		public DateTime CreateDate { get; set; }
		public string ResAsgnCode { get; set; } = string.Empty;
		public string PickupTimeHO { get; set; } = string.Empty;
		public string DefaultContractorNameCO { get; set; } = string.Empty;
		public string ForcedJobCompletionNotes { get; set; } = string.Empty;
        public string RSVPRCode { get; set; } = string.Empty;
        public string ProcedureDescription { get; set; } = string.Empty;
        public DateTime? ReservationCanceledDate { get; set; }
        public DateTime? ReservationCanceledTime { get; set; }
		public string? ReservationRSVCXdescription { get; set; }
    }
}
