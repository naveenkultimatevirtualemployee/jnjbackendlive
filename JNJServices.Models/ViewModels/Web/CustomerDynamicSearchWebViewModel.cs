using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class CustomerDynamicSearchWebViewModel
    {
        [Required]
        public string? CustomerName { get; set; }

    }
}
