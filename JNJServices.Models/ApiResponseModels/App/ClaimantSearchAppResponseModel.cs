namespace JNJServices.Models.ApiResponseModels.App
{
    public class ClaimantSearchAppResponseModel
    {
        public int ClaimantID { get; set; }
        public string? FullName { get; set; }
        public string? firstName { get; set; }
        public string MiddleName { get; set; } = string.Empty;
        public string? lastName { get; set; }
        public string? hmaddress1 { get; set; }
        public string? HmAddress2 { get; set; }
        public string? hmcity { get; set; }
        public string? Gender { get; set; }
        public string? email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? HmPhone { get; set; }
        public string? homePhone { get; set; }
        public string? CellPhone { get; set; }
    }
}
