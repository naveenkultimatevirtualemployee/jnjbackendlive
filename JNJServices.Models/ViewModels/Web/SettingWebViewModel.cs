using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class SettingWebViewModel
    {
        [Required]
        public string? Key { get; set; }

        public string? Value { get; set; }
    }
}
