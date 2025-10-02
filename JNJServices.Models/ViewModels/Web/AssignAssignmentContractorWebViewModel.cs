using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class AssignAssignmentContractorWebViewModel
    {
        [Required]
        public string? AssignType { get; set; }
        [Required]
        public int? reservationid { get; set; }
        [Required]
        public int? reservationassignmentid { get; set; }
        [Required]
        public int? contractorid { get; set; }
        public string? contractorname { get; set; }
        public string? company { get; set; }
        public string? CellPhone { get; set; }
        public string? contycode { get; set; }
        public string? conctcode { get; set; }
        public string? conpccode { get; set; }
        public string? gender { get; set; }
        public string? city { get; set; }
        public string? statecode { get; set; }
        public string? zipcode { get; set; }
        public Decimal? miles { get; set; }
        public string? cstatus { get; set; }
        public string? constcode { get; set; }
        public Decimal? RatePerMiles { get; set; }
        public Decimal? RatePerHour { get; set; }
        public string? Cost { get; set; }
        public int IsPreferredContractor { get; set; }
    }
}
