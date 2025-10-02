using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities
{
    [Table("ContractorRatesDet")]
    public class ContractorRatesDetails
    {
        public int ContractorRatesDetID { get; set; }
        public int? ContractorRatesID { get; set; }
        public string ACCTGCode { get; set; } = string.Empty;
        public string TRNTYCode { get; set; } = string.Empty;
        public string LANGUCode { get; set; } = string.Empty;
        public decimal rate { get; set; }
        public int flatrateflag { get; set; }
        public decimal TMQty { get; set; }
        public decimal MPQty { get; set; }
        public decimal RoundInt { get; set; }
    }
}
