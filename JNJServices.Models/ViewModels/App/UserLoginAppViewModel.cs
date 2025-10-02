using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JNJServices.Models.ViewModels.App
{
    public class UserLoginAppViewModel
    {
        [Required]
        [Range(1, Int32.MaxValue)]
        public int Type { get; set; }
        [Required]
        [Range(1, Int32.MaxValue)]
        public int UserID { get; set; }
        public string? PhoneNo { get; set; }
        public string? LoginPassword { get; set; }
        public string? DeviceID { get; set; }
        public string? FcmToken { get; set; }
    }
}
