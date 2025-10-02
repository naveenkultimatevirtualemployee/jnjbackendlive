namespace JNJServices.Models.ApiResponseModels.App
{
    public class UserFeedbackAppResponseModel
    {
        public int? ReservationAssignmentID { get; set; }
        public int? FromID { get; set; }
        public int? ToID { get; set; }
        public string? Notes { get; set; }
        public Decimal? Answer1 { get; set; }
        public Decimal? Answer2 { get; set; }
        public Decimal? Answer3 { get; set; }
        public Decimal? Answer4 { get; set; }
        public Decimal? Answer5 { get; set; }
        public Decimal? Answer6 { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedUserID { get; set; }
    }
}
