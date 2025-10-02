using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.CommonModels
{
    public class AppTokenDetails
    {
        public AppTokenDetails()
        {
            FcmToken = string.Empty;
        }

        [Required]
        public int? Type { get; set; }
        [Required]
        public int? UserID { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNo { get; set; }
        public string FcmToken { get; set; }
    }
}
