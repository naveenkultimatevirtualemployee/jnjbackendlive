namespace JNJServices.Models.Entities
{
    public class ContractorRates
    {
        public int ContractorRatesID { get; set; }
        public string RATECTCode { get; set; } = string.Empty;
        public DateTime? EffectiveDateFrom { get; set; }
        public DateTime? EffectiveDateTo { get; set; }
        public string STATECode { get; set; } = string.Empty;
        public int? inactiveflag { get; set; }
        public string RateDescription { get; set; } = string.Empty;
        public int? ContractorID { get; set; }
    }
}
