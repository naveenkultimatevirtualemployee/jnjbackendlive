using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ReservationAssignmentWebViewModel
    {
        public int? ReservationID { get; set; }
        public int? ReservationsAssignmentsID { get; set; }
        public int? ContractorID { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public string? DateFrom { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public string? DateTo { get; set; }
        public int? ClaimID { get; set; }
        public int? ClaimantID { get; set; }
        public int? CustomerID { get; set; }
        public string? ASSGNCode { get; set; }
        public string? RSVACCode { get; set; }
        public string? AssignmentJobStatus { get; set; }
        public List<string>? ContractorContactInfo { get; set; }
        public int? inactiveflag { get; set; }
        public int? ConAssignStatus { get; set; }
        [Range(1, Int32.MaxValue)]
        public int? Page { get; set; } = 1;
        [Range(1, Int32.MaxValue)]
        public int? Limit { get; set; } = 20;
    }
}
