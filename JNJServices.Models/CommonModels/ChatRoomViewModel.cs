using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.CommonModels
{
	public class ChatRoomViewModel
	{
		[Required]
		public int? RoomId { get; set; }
		public DateTime CreateAt { get; set; }
	}
}
