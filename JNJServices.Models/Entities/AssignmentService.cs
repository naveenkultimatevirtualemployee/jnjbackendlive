using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities
{
    [Table("codesASSGN")]
    public class AssignmentService
    {
        public string code { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }
}
