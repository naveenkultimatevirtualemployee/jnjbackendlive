using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities
{
    [Table("codesLANGU")]
    public class Languages
    {
        public string code { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }
}
