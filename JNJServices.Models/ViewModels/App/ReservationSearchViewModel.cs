using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.App
{
    public class ReservationSearchViewModel
    {
        public int? Type { get; set; }
        public int? reservationid { get; set; }
        public int? ClaimantID { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public string? DateFrom { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public string? DateTo { get; set; }
        public DateTime? ReservationTimeFrom { get; set; }
        public DateTime? ReservationTimeTo { get; set; }
        public string? ActionCode { get; set; }
        public string? ReservationTextSearch { get; set; }
        public int? IsTodayJobNotStarted { get; set; }
        public int? IsJobComplete { get; set; }
        public string? ClaimantConfirmation { get; set; }
        public int? inactiveflag { get; set; }
        [Range(1, Int32.MaxValue)]
        public int? Page { get; set; } = 1;
        [Range(1, Int32.MaxValue)]
        public int? Limit { get; set; } = 20;
    }
}
