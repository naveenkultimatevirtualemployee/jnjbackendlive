namespace JNJServices.Models.DbResponseModels
{
    public class ContractorNotAssignedResponseModel : BaseContractorResponseModel
    {
        public string ReservationsAssignmentsID { get; set; } = string.Empty;
        public DateTime? PickupTime { get; set; }
        public int? ContractorID { get; set; }
    }
}
