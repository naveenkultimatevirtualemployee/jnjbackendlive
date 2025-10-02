using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class CustomerSearchWebViewModel
    {
        public int? CustomerID { get; set; }
        public string? CustomerName { get; set; }
        public string? Email { get; set; }
        public string? States { get; set; }
        public string? Category { get; set; }
        public int? Inactive { get; set; }
        [Range(1, Int32.MaxValue)]
        public int? Page { get; set; } = 1;
        [Range(1, Int32.MaxValue)]
        public int? Limit { get; set; } = 20;
    }
}
