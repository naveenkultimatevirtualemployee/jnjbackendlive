using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class BaseContractorSearchViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Mobile { get; set; }
        public string? StatusCode { get; set; }
        public string? ServiceCode { get; set; }
        public string? State { get; set; }
        public string? VehicleSize { get; set; }
        public string? LanguageCode { get; set; }
        public int? inactiveflag { get; set; }
        public int? ZipCode { get; set; }
        public int? Miles { get; set; }
        [Range(1, Int32.MaxValue)]
        public int? Page { get; set; } = 1;
        [Range(1, Int32.MaxValue)]
        public int? Limit { get; set; } = 20;
    }
}
