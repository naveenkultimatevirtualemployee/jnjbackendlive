using System.Text.Json.Serialization;

namespace JNJServices.Models.ApiResponseModels.Web
{
    public class ReservationListResponseModel
    {
        public int reservationid { get; set; }
        public DateTime ReservationDate { get; set; }
        public int? inactiveflag { get; set; }
        public int? relatedReservationId { get; set; }
        public string? claimnumber { get; set; }
        public int customerid { get; set; }
        public int claimantid { get; set; }
        public string claimantname { get; set; } = string.Empty;
        public string? CustomerFL { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string TransType { get; set; } = string.Empty;
        [JsonIgnore]
        public int TotalCount { get; set; }
        public int RelatedReservationIdCount { get; set; }
    }
}
