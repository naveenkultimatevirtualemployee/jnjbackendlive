namespace JNJServices.Models.ApiResponseModels.Web
{
    public class ContractorListResponseModel
    {
        public int ContractorID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CellPhone { get; set; } = string.Empty;
        public string HomePhone { get; set; } = string.Empty;
        public string WorkPhone { get; set; } = string.Empty;
        public string WorkPhoneExt { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public int inactiveflag { get; set; }
        public string RATECTCode { get; set; } = string.Empty;
        public string ContractorProfileImage { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }
}
