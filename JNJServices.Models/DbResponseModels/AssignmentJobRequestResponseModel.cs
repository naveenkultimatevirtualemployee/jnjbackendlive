namespace JNJServices.Models.DbResponseModels
{
    public class AssignmentJobRequestResponseModel : BaseAssignmentResponseModel
    {
        public int ReservationsAssignmentsID { get; set; }
        public string NotificationType { get; set; } = string.Empty;
    }
}
