using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities.Views
{
    [Table("vwContractorSearch")]
    public class vwContractorSearch
    {
        public int ContractorID { get; set; }
        public string ShortName { get; set; } = string.Empty;
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public DateTime? Birthdate { get; set; }
        public string CellPhone { get; set; } = string.Empty;
        public DateTime? CertifiedDate { get; set; }
        public int certifiedflag { get; set; }
        public string City { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string CONCMCode { get; set; } = string.Empty;
        public string CONCTCode { get; set; } = string.Empty;
        public string CONDLCode { get; set; } = string.Empty;
        public string CONHACode_Fri { get; set; } = string.Empty;
        public string CONHACode_Mon { get; set; } = string.Empty;
        public string CONHACode_Sat { get; set; } = string.Empty;
        public string CONHACode_Sun { get; set; } = string.Empty;
        public string CONHACode_Thu { get; set; } = string.Empty;
        public string CONHACode_Tues { get; set; } = string.Empty;
        public string CONHACode_Wed { get; set; } = string.Empty;
        public string CONPCCode { get; set; } = string.Empty;
        public string CONSTCode { get; set; } = string.Empty;
        public string ContractorIntfID { get; set; } = string.Empty;
        public string CONTRCode { get; set; } = string.Empty;
        public string CONTYCode { get; set; } = string.Empty;
        public DateTime? CreateDate { get; set; }
        public string CreateUserID { get; set; } = string.Empty;
        public string dlSTATECode { get; set; } = string.Empty;
        public DateTime? DrivLicExp { get; set; }
        public string DrivLicNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public int Flag1099 { get; set; }
        public string Gender { get; set; } = string.Empty;
        public DateTime? HireDate { get; set; }
        public string HomePhone { get; set; } = string.Empty;
        public int inactiveflag { get; set; }
        public decimal? InterpretationRate { get; set; }
        public DateTime? LastChangeDate { get; set; }
        public string LastChangeUserID { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public decimal? PayRateperMile { get; set; }
        public DateTime? ReHireDate { get; set; }
        public string STATECode { get; set; } = string.Empty;
        public string SUFFXCode { get; set; } = string.Empty;
        public string TaxID { get; set; } = string.Empty;
        public DateTime? TerminationDate { get; set; }
        public string TITLECode { get; set; } = string.Empty;
        public string WorkPhone { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public int? MinInterpretationHours { get; set; }
        public string RATETYPCTCode { get; set; } = string.Empty;
        public DateTime? COIExpirationDate { get; set; }
        public string COIPolicyNumber { get; set; } = string.Empty;
        public string COIInsuranceCo { get; set; } = string.Empty;
        public DateTime? mvrExpirationDate { get; set; }
        public string WorkPhoneExt { get; set; } = string.Empty;
        public string Address3 { get; set; } = string.Empty;
        public DateTime? coCertifiedDate { get; set; }
        public int? coCertifiedFlag { get; set; }
        public string MailingName { get; set; } = string.Empty;
        public string mlAddress1 { get; set; } = string.Empty;
        public string mlAddress2 { get; set; } = string.Empty;
        public string mlAddress3 { get; set; } = string.Empty;
        public string mlCity { get; set; } = string.Empty;
        public string mlSTATECode { get; set; } = string.Empty;
        public string mlZipCode { get; set; } = string.Empty;
        public string FaxPrefix { get; set; } = string.Empty;
        public DateTime? w9ExpirationDate { get; set; }
        public string mlTimeZone { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public string COIDescription { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int? QBImportflag { get; set; }
        public int? insurancedeductionflag { get; set; }
        public string accountnumber { get; set; } = string.Empty;
        public string RATECTCode { get; set; } = string.Empty;
        public string TERMSCode { get; set; } = string.Empty;
        public string Email2 { get; set; } = string.Empty;
        public string Email3 { get; set; } = string.Empty;
        public string loginname { get; set; } = string.Empty;
        public string AssgnLogEmail { get; set; } = string.Empty;
        public string DBAName { get; set; } = string.Empty;
        public int archiveflag { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public string FormatHomePhone { get; set; } = string.Empty;
        public string FormatCellpone { get; set; } = string.Empty;
        public string ContractorProfileImage { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }
}
