namespace JNJServices.Models.ApiResponseModels.Web
{
    public class ClaimantListResponseModel
    {
        public int ClaimantID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? HomePhone { get; set; } = string.Empty;
        public string CellPhone { get; set; } = string.Empty;
        public string? HomePhonewH { get; set; } = string.Empty;
        public string WkPhone { get; set; } = string.Empty;

        public DateTime? CreateDate { get; set; }
        public int inactiveflag { get; set; }
        public int TotalCount { get; set; }

    }
}
