using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ReservationSearchWebViewModel
    {
        public int? reservationid { get; set; }
        public int? relatedReservationId { get; set; }
        public string? claimnumber { get; set; }
        public int? ClaimID { get; set; }
        public int? CustomerID { get; set; }
        public int? ClaimantID { get; set; }
        public int? Contractorid { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public string? DateFrom { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public string? DateTo { get; set; }
        public string? Service { get; set; }
        public int? inactiveflag { get; set; }
        public string? TransportType { get; set; }
        public string? TripType { get; set; }
        public string? ClaimantConfirmation { get; set; }
        public string? ActionCode { get; set; }
        public int? ConAssignStatus { get; set; }
        [Range(1, Int32.MaxValue)]
        public int? Page { get; set; } = 1;
        [Range(1, Int32.MaxValue)]
        public int? Limit { get; set; } = 20;
    }
}
