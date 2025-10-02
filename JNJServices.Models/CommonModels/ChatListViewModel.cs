namespace JNJServices.Models.CommonModels
{
    public class ChatListViewModel
    {
        public string? SearchQuery { get; set; }
        public string? RoomId { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int? ContractorId { get; set; }
    }
}
