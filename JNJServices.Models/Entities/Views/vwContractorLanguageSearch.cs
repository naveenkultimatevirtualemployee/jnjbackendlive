using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities.Views
{
    [Table("vwContractorLangSearch")]
    public class vwContractorLanguageSearch
    {
        public int ContractorID { get; set; }
        public string LANGUCode { get; set; } = string.Empty;

        public int Interpretflag { get; set; }

        public int Translateflag { get; set; }

        public DateTime? LastChangeDate { get; set; }

        [StringLength(30)]
        public string LastChangeUserID { get; set; } = string.Empty;

        public DateTime? CreateDate { get; set; }

        [StringLength(30)]
        public string CreateUserID { get; set; } = string.Empty;

        public int? transcribeflag { get; set; }

        public int CertifiedFlag { get; set; }

        [StringLength(20)]
        public string CONLCTCode { get; set; } = string.Empty;

        public int archiveflag { get; set; }
        public string CONLCTName { get; set; } = string.Empty;
        public string LANGName { get; set; } = string.Empty;
    }
}
