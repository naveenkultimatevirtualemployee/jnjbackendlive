namespace JNJServices.Models.DbResponseModels
{
    public class FeedbackStatus
    {
        public int ReservationAssignmentID { get; set; }
        public int IsFeedbackComplete { get; set; } // 1 for complete, 0 for not complete
    }

    public class JobStatus
    {
        public int ReservationAssignmentID { get; set; }
        public int IsJobStarted { get; set; } // 1 for started, 0 for not started
    }

    public class ContractorProfile
    {
        public int ContractorID { get; set; }
        public string ContractorProfileImage { get; set; } = string.Empty;
    }

}

