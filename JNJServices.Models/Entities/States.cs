using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.Entities
{
    public class States
    {
        [Key]
        public string code { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }
}
