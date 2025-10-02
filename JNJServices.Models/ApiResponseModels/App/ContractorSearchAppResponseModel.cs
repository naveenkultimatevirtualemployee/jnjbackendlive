namespace JNJServices.Models.ApiResponseModels.App
{
    public class ContractorSearchAppResponseModel
    {
        public int ContractorID { get; set; }
        public string? fullName { get; set; }
        public string? firstName { get; set; }
        public string MiddleName { get; set; } = string.Empty;
        public string? lastName { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? CellPhone { get; set; }
        public string? Company { get; set; }
        public string? Gender { get; set; }
        public string? City { get; set; }
        public string? email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? mlAddress1 { get; set; }
        public string? homePhone { get; set; }
        public string ContractorProfileImage { get; set; } = string.Empty;

    }
}
