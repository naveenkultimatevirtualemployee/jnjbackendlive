using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities.Views
{
    public class vwClaimsFacilities
    {
        public int claimid { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FacilityID { get; set; }

        public DateTime? origauthdate { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(20)]
        public string clfstcode { get; set; } = string.Empty;

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int inactiveflag { get; set; }

        public DateTime? origauthexpdate { get; set; }

        [StringLength(30)]
        public string createuserid { get; set; } = string.Empty;

        public DateTime? CreateDate { get; set; }

        [StringLength(174)]
        public string AuthContactFL { get; set; } = string.Empty;

        [StringLength(174)]
        public string OAuthContactFL { get; set; } = string.Empty;

        [StringLength(20)]
        public string FacilityShortName { get; set; } = string.Empty;

        [Key]
        [Column(Order = 4)]
        [StringLength(20)]
        public string factycode { get; set; } = string.Empty;

        [StringLength(50)]
        public string facilityname { get; set; } = string.Empty;

        [StringLength(50)]
        public string facilityname2 { get; set; } = string.Empty;

        [StringLength(50)]
        public string contactname { get; set; } = string.Empty;

        [StringLength(50)]
        public string complex { get; set; } = string.Empty;

        [StringLength(50)]
        public string address1 { get; set; } = string.Empty;

        [StringLength(50)]
        public string address2 { get; set; } = string.Empty;

        [StringLength(50)]
        public string city { get; set; } = string.Empty;

        [StringLength(3)]
        public string statecode { get; set; } = string.Empty;

        [StringLength(10)]
        public string zipcode { get; set; } = string.Empty;

        [StringLength(20)]
        public string phone { get; set; } = string.Empty;

        [StringLength(827)]
        public string FacilityAddress { get; set; } = string.Empty;
        [StringLength(100)]
        public string LanguageName { get; set; } = string.Empty;
        [StringLength(100)]
        public string FacilityType { get; set; } = string.Empty;
        public string CLFSTStatus { get; set; } = string.Empty;
        public string FLANGUCode1 { get; set; } = string.Empty;
        public string FLANGUCode2 { get; set; } = string.Empty;
        public DateTime? TranspExpDate { get; set; }
        public DateTime? InterpExpDate { get; set; }
    }
}
