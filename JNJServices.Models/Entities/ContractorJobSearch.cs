namespace JNJServices.Models.Entities
{
    public class ContractorJobSearch
    {
        public int? JobSearchID { get; set; }
        public int? ReservationsAssignmentsID { get; set; }
        public int? ContractorId { get; set; }
        public string? ContractorName { get; set; }
        public string? Company { get; set; }
        public string? CellPhone { get; set; }
        public string? Contycode { get; set; }
        public string? Conctcode { get; set; }
        public string? Conpccode { get; set; }
        public string? Gender { get; set; }
        public string? City { get; set; }
        public string? StateCode { get; set; }
        public string? ZipCode { get; set; }
        public float? Miles { get; set; }
        public string? Cstatus { get; set; }
        public string? ConstCode { get; set; }
        public string? Cost { get; set; }
        public Decimal? RatePerMiles { get; set; }
        public DateTime? NotificationDateTime { get; set; }
        public int? JobStatus { get; set; }
        public bool? isPreferredContractor { get; set; }
        public string? EstimateMiles { get; set; }
        public string? EstimateMinutes { get; set; }
        public string? RatePerHour { get; set; }
    }
}
