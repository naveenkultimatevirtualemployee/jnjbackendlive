using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities.Views
{
    [Table("vwClaimantSearch")]
    public class vwContractorDriversSearch
    {
        public int ContractorID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(100)]
        public string DriverName { get; set; } = string.Empty;

        public DateTime? Birthdate { get; set; }

        [StringLength(20)]
        public string CONDLCode { get; set; } = string.Empty;

        [StringLength(3)]
        public string DLSTATECode { get; set; } = string.Empty;

        public DateTime? DrivLicExp { get; set; }

        [StringLength(50)]
        public string DrivLicNumber { get; set; } = string.Empty;

        [StringLength(2)]
        public string Gender { get; set; } = string.Empty;

        [StringLength(20)]
        public string SSN { get; set; } = string.Empty;

        public DateTime? LastChangeDate { get; set; }

        [StringLength(30)]
        public string LastChangeUserID { get; set; } = string.Empty;

        public DateTime? CreateDate { get; set; }

        [StringLength(30)]
        public string CreateUserID { get; set; } = string.Empty;

        public int archiveflag { get; set; }
        public string DLType { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public int? IsPrimary { get; set; }
    }
}
