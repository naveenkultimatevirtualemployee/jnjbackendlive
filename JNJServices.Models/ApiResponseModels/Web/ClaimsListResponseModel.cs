using System.Text.Json.Serialization;

namespace JNJServices.Models.ApiResponseModels.Web
{
    public class ClaimsListResponseModel
    {
        public int claimid { get; set; }
        public string claimnumber { get; set; } = string.Empty;
        public string ClaimantName { get; set; } = string.Empty;
        public int claimantid { get; set; }
        public int customerid { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerFL { get; set; }
        [JsonIgnore]
        public int TotalCount { get; set; }
    }
}
