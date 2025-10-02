namespace JNJServices.Models.Entities
{
    public class Claimants
    {
        public int ClaimantID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string LastNameFirstName { get; set; } = string.Empty;
        public string FirstNameLastName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FirstNameLastInitial { get; set; } = string.Empty;
        public string FirstInitialLastName { get; set; } = string.Empty;
        public string height { get; set; } = string.Empty;
        public int? weight { get; set; }
        public string HomePhone { get; set; } = string.Empty;
        public string CellPhone { get; set; } = string.Empty;
        public string hmcomplex { get; set; } = string.Empty;
        public string hmaddress1 { get; set; } = string.Empty;
        public string HmAddress2 { get; set; } = string.Empty;
        public string hmcity { get; set; } = string.Empty;
        public string hmSTATECode { get; set; } = string.Empty;
        public string HmZipCode { get; set; } = string.Empty;
        public string hmPickupNotes { get; set; } = string.Empty;
        public string AltComplex { get; set; } = string.Empty;
        public string AltAddress1 { get; set; } = string.Empty;
        public string AltAddress2 { get; set; } = string.Empty;
        public string AltCity { get; set; } = string.Empty;
        public string altSTATECode { get; set; } = string.Empty;
        public string AltZipCode { get; set; } = string.Empty;
        public string altPickupNotes { get; set; } = string.Empty;
        public string WkComplex { get; set; } = string.Empty;
        public string WkAddress1 { get; set; } = string.Empty;
        public string WkAddress2 { get; set; } = string.Empty;
        public string wkaddress3 { get; set; } = string.Empty;
        public string WkCity { get; set; } = string.Empty;
        public string wkSTATECode { get; set; } = string.Empty;
        public string WkZipCode { get; set; } = string.Empty;
        public string wkPickupNotes { get; set; } = string.Empty;
        public DateTime? birthdate { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string shortname { get; set; } = string.Empty;
        public int? hmStairs { get; set; }
        public string LANGUCode1 { get; set; } = string.Empty;
        public string ssn { get; set; } = string.Empty;
        public string wkshortname { get; set; } = string.Empty;
        public string cnttycode { get; set; } = string.Empty;
        public string cusifcode { get; set; } = string.Empty;
        public string HeightWeight { get; set; } = string.Empty;
        public string HomePhonewH { get; set; } = string.Empty;
        public string ClaimantHomeAddresswNotes { get; set; } = string.Empty;
        public string ClaimantAltAddresswNotes { get; set; } = string.Empty;
        public string ClaimantWorkAddresswNotes { get; set; } = string.Empty;
        public string claimantnotes { get; set; } = string.Empty;
        public DateTime? CreateDate { get; set; }
        public string CreateUserID { get; set; } = string.Empty;
        public DateTime? LastChangeDate { get; set; }
        public string LastChangeUserID { get; set; } = string.Empty;
        public string ClaimantStatus { get; set; } = string.Empty;
        public string ClaimantCompany { get; set; } = string.Empty;
        public string ClaimantCompanycontact { get; set; } = string.Empty;
        public string WkPhone { get; set; } = string.Empty;
        public string wkPhoneExt { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LANGUCode2 { get; set; } = string.Empty;
        public string CLMMSCode { get; set; } = string.Empty;
        public string Spouse { get; set; } = string.Empty;
        public string altPerson { get; set; } = string.Empty;
        public string altRelationship { get; set; } = string.Empty;
        public string AltPhone { get; set; } = string.Empty;
        public int? wkStairs { get; set; }
        public int? altStairs { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string HmPhone { get; set; } = string.Empty;
        public int inactiveflag { get; set; }
        public string MiddleName { get; set; } = string.Empty;
        public string SUFFXCode { get; set; } = string.Empty;
        public string TITLECode { get; set; } = string.Empty;
        public int wkContactID { get; set; }
        public string altTimeZone { get; set; } = string.Empty;
        public string hmTimeZone { get; set; } = string.Empty;
        public string Email2 { get; set; } = string.Empty;
        public string Email3 { get; set; } = string.Empty;
        public string AmerisysClaimantID { get; set; } = string.Empty;
        public DateTime AmerisysClaimantEntryDate { get; set; }
        public DateTime AmerisysClaimantUpdateDate { get; set; }
        public int WeightChair { get; set; }
        public int qaflag { get; set; }
        public string PhoneTypCode { get; set; } = string.Empty;
        public int archiveflag { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string companyName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }
}
