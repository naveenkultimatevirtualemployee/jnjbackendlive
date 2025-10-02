using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities
{
    [Table("ContractorsServiceLoc")]
    public class ContractorServiceLocation
    {
        public int ContractorID { get; set; }
        public string ZipCode { get; set; } = string.Empty;
        public DateTime? LastChangeDate { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string CountyName { get; set; } = string.Empty;
        public int? InactiveFlag { get; set; }
    }
}
