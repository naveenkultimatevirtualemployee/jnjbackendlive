using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class ClaimantSearchViewModel
    {
        public int? ClaimantID { get; set; }
        public int? CustomerID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? LanguageCode { get; set; }
        public int? ZipCode { get; set; }
        public int? Miles { get; set; }
        public int? Inactive { get; set; }
        [Range(1, Int32.MaxValue)]
        public int? Page { get; set; } = 1;
        [Range(1, Int32.MaxValue)]
        public int? Limit { get; set; } = 20;
    }
}
