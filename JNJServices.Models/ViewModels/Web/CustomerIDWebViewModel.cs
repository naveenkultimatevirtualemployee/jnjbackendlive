using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class CustomerIDWebViewModel
    {
        [Required]
        public int? CustomerID { get; set; }
    }
}
