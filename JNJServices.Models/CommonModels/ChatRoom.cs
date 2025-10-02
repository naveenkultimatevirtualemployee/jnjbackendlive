namespace JNJServices.Models.CommonModels
{
    public class ChatRoom
    {
        public int chatRoomId { get; set; }
        public int roomId { get; set; }
        public string lastMessage { get; set; } = string.Empty;
        public DateTime lastMessageAt { get; set; }
        public DateTime createdAt { get; set; }
        public bool isInactive { get; set; }
        public DateTime? inactiveAt { get; set; }
        public string assgncode { get; set; } = string.Empty;
        public string assignmentJobStatus { get; set; } = string.Empty;
        public string rsvattCode { get; set; } = string.Empty;
        public string assgnNum { get; set; } = string.Empty;
        public string resAsgnCode { get; set; } = string.Empty;
        public string resTripType { get; set; } = string.Empty;
        public int? reservationid { get; set; }
        public int? reservationsAssignmentsID { get; set; }
        public string contractorNameFL { get; set; } = string.Empty;
        public string contractorInitials { get; set; } = string.Empty;
        public int contractorID { get; set; }
        public string actionCode { get; set; } = string.Empty;
        public string rsvacCode { get; set; } = string.Empty;
        public string profileImage { get; set; } = string.Empty; // Added Profile Image URL
        public int contractorMetricsFlag { get; set; }
        public bool WebReadStatus { get; set; }

    }

}
