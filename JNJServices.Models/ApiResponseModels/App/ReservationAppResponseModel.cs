namespace JNJServices.Models.ApiResponseModels.App
{
    public class ReservationAppResponseModel
    {
        public int reservationid { get; set; }
        public DateTime? ReservationDate { get; set; }
        public DateTime? ReservationTime { get; set; }
        public string? RSVSVCode { get; set; }
        public string? ServiceName { get; set; }
        public string? RSVTTCode { get; set; }
        public string? TripType { get; set; }
        public string? RSVACCode { get; set; }
        public string? ActionCode { get; set; }
        public string? PUAddress1 { get; set; }
        public string? PUAddress2 { get; set; }
        public string? DOFacilityName { get; set; }
        public string? DOFacilityName2 { get; set; }
        public string? DOPhone { get; set; }
        public string? DOAddress1 { get; set; }
        public string? DOAddress2 { get; set; }
        public int? inactiveflag { get; set; }
        public int? relatedreservationid { get; set; }
        public string? RSVCXdescription { get; set; }
        public DateTime? CanceledDate { get; set; }
        public int TotalCount { get; set; }
    }
}
