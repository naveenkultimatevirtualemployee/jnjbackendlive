using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ApiResponseModels.Web
{
    public class ClaimsApprovedContractorWebResponseModel
    {
        [Key]
        public int ClaimID { get; set; }

        [Key]
        public int ContractorID { get; set; }

        public decimal? Miles { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public int? PrioritySort { get; set; }

        public DateTime? CreateDate { get; set; }

        [StringLength(30)]
        public string? CreateUserID { get; set; }

        public DateTime? LastChangeDate { get; set; }

        [StringLength(30)]
        public string? LastChangeUserID { get; set; }

        public int archiveflag { get; set; }
        public string? ContractorName { get; set; }
        public string? Service { get; set; }
    }
}
