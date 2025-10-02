using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities.Views
{
    [Table("vwContractorVehicleSearch")]
    public class vwContractorVehicleSearch
    {
        public int ContractorID { get; set; }

        [StringLength(50)]
        public string VIN { get; set; } = string.Empty;

        [StringLength(50)]
        public string CarMake { get; set; } = string.Empty;

        [StringLength(50)]
        public string CarModel { get; set; } = string.Empty;

        public int? CarYear { get; set; }

        [StringLength(50)]
        public string InsuranceCo { get; set; } = string.Empty;

        [StringLength(50)]
        public string Insured { get; set; } = string.Empty;

        public DateTime? PolicyExpiration { get; set; }

        [StringLength(50)]
        public string PolicyNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string RegisteredOwner { get; set; } = string.Empty;

        [StringLength(3)]
        public string RegSTATECode { get; set; } = string.Empty;

        [StringLength(20)]
        public string VEHSZCode { get; set; } = string.Empty;

        public DateTime? LastChangeDate { get; set; }

        [StringLength(30)]
        public string LastChangeUserID { get; set; } = string.Empty;

        public DateTime? CreateDate { get; set; }

        [StringLength(30)]
        public string CreateUserID { get; set; } = string.Empty;

        public int inactiveflag { get; set; }

        [StringLength(255)]
        public string limit { get; set; } = string.Empty;

        public int archiveflag { get; set; }
        [StringLength(100)]
        public string VEHSZName { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public int? IsPrimary { get; set; }
    }
}
