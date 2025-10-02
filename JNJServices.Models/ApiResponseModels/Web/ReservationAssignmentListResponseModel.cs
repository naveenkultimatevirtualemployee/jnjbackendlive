using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JNJServices.Models.ApiResponseModels.Web
{
    public class ReservationAssignmentListResponseModel
    {
        public int reservationsAssignmentsID { get; set; }
        public int? linenum { get; set; }
        public int? reservationid { get; set; }
        public int? relatedreservationid { get; set; }
        public string? contractorCompany { get; set; }
        public string? conctcode { get; set; }
        public string? contractor { get; set; }
        public int? jobStatus { get; set; }
        public string? assgncode { get; set; }
        public string? resTripType { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string? ReservationDate { get; set; }
        public string? pickupTime { get; set; }
        public string? actionCode { get; set; }
        public string? assignmentJobStatus { get; set; }
        public int? inactiveflag { get; set; }
        public string? rsvacCode { get; set; }
        public int? contractorid { get; set; }
        public int? contractorMetricsFlag { get; set; }
        // Computed/Derived Fields
        public int isAssignmentTracking { get; set; }
        public int? isContractorFound { get; set; }
        [JsonIgnore]
        public int TotalCount { get; set; }
    }
}
