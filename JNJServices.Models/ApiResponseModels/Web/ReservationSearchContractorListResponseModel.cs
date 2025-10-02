namespace JNJServices.Models.ApiResponseModels.Web
{
    public class ReservationSearchContractorListResponseModel
    {
        public int ContractorID { get; set; }
        public int Linenum { get; set; }
        public string? Contractor { get; set; }
        public string? ContractorCellPhone { get; set; }
        public string? ContractorEmail { get; set; }
        public string? Quantity { get; set; }
        public string? ResAsgnCode { get; set; }
        public string? ResTripType { get; set; }
        public string? PickupTime { get; set; }
        public string? PickupTimeHO { get; set; }
        public string? PickupTimeZone { get; set; }
        public string? Language { get; set; }
    }
}
