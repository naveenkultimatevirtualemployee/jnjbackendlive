using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ForgotPasswordMailTokenWebViewModel
    {
        public string UserID { get; set; } = string.Empty;
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime DateTime { get; set; }
    }
}
