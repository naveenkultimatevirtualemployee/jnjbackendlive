namespace JNJServices.Models.ApiResponseModels.Web
{
    public class MetricsListResponseModel
    {
        public int? customerid { get; set; }
        public int? contractorid { get; set; }
        //public int reservationID { get; set; }
        public int reservationid { get; set; }
        public int ReservationsAssignmentsID { get; set; }
        public string? AssgnNum { get; set; }
        public string? claimnumber { get; set; }
        public string? customerName { get; set; }
        public string DefaultContractorNameCO { get; set; } = string.Empty;
        public string? ReservationDate { get; set; }
        public string? assgncode { get; set; }
        public int? MetricsEnteredFlag { get; set; }
        public int TotalCount { get; set; }
    }
}
