using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities.Views
{
    [Table("vwClaimsSearch")]
    public class vwClaimsSearch
    {
        public int claimid { get; set; }

        public string claimnumber { get; set; } = string.Empty;

        public string ClaimantName { get; set; } = string.Empty;

        public int claimantid { get; set; }

        public int customerid { get; set; }

        public string claimintfid { get; set; } = string.Empty;

        public DateTime? injurydate { get; set; }

        public string injurytype { get; set; } = string.Empty;

        public DateTime? referraldate { get; set; }

        public DateTime? referraltime { get; set; }

        public string trntycode { get; set; } = string.Empty;

        public string ratecode { get; set; } = string.Empty;

        public int inactiveflag { get; set; }

        public string vehszcode { get; set; } = string.Empty;

        public string clmrscode { get; set; } = string.Empty;

        public string clmlscode { get; set; } = string.Empty;

        public string ratetypcode { get; set; } = string.Empty;

        public string clmdgcode1 { get; set; } = string.Empty;

        public string clmdgcode2 { get; set; } = string.Empty;

        public string clmdgcode3 { get; set; } = string.Empty;

        public string clmdgcode4 { get; set; } = string.Empty;

        public int? referralcontactid { get; set; }

        public string payerid { get; set; } = string.Empty;

        public string payername { get; set; } = string.Empty;

        public string PayerAddress1 { get; set; } = string.Empty;

        public string PayerAddress2 { get; set; } = string.Empty;

        public string PayerCity { get; set; } = string.Empty;

        public string PayerStateCode { get; set; } = string.Empty;

        public string PayerZipCode { get; set; } = string.Empty;

        public string AmerisysDBID { get; set; } = string.Empty;

        public string AmerisysClaimID { get; set; } = string.Empty;

        public int iccflag { get; set; }

        public string notes { get; set; } = string.Empty;

        public int qaflag { get; set; }

        public string Transportation { get; set; } = string.Empty;

        public string Interpretation { get; set; } = string.Empty;

        public string ClaimStatus { get; set; } = string.Empty;

        public string EmployerRelated { get; set; } = string.Empty;

        public string AutoAccident { get; set; } = string.Empty;

        public string OtherAccident { get; set; } = string.Empty;

        public DateTime? createdate { get; set; }

        public string createuserid { get; set; } = string.Empty;

        public DateTime? lastchangedate { get; set; }

        public string lastchangeuserid { get; set; } = string.Empty;

        public int SettledFlag { get; set; }

        public string ClaimantLastNameFirstName { get; set; } = string.Empty;

        public string ClaimantFullName { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string customerintfid { get; set; } = string.Empty;

        public int? billadjustercontactid { get; set; }

        public string InvoiceContactFL { get; set; } = string.Empty;

        public string InvoiceContactLF { get; set; } = string.Empty;

        public string ICcnttycode { get; set; } = string.Empty;

        public string InvoiceContact { get; set; } = string.Empty;

        public string ReferralContactFL { get; set; } = string.Empty;

        public string ReferralContactLN { get; set; } = string.Empty;

        public string ReferralContactFN { get; set; } = string.Empty;

        public string ClaimEmployer { get; set; } = string.Empty;

        public string ClaimEmployerAddress1 { get; set; } = string.Empty;

        public string ClaimEmployerAddress2 { get; set; } = string.Empty;

        public string ClaimEmployerAddress3 { get; set; } = string.Empty;

        public string ClaimEmployerCity { get; set; } = string.Empty;

        public string ClaimEmployerState { get; set; } = string.Empty;

        public string ClaimEmployerZip { get; set; } = string.Empty;

        public int DriverNeededFlag { get; set; }

        public string LanguCode { get; set; } = string.Empty;
        public DateTime? Birthdate { get; set; }
        public string height { get; set; } = string.Empty;
        public int? weight { get; set; }
        public string? CustomerFL { get; set; }
        public int TotalCount { get; set; }
    }
}
