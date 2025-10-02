namespace JNJServices.Models.DbResponseModels
{
    public class ContractorWebJobSearchRespnseModel : BaseAssignmentResponseModel
    {
        public int ReservationsAssignmentsID { get; set; }
        public string NotificationType { get; set; } = string.Empty;
    }
}
