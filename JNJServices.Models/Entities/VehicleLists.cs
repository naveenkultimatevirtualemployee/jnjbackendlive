using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities
{
    [Table("codesVEHSZ")]
    public class VehicleLists
    {
        public string code { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }
}
