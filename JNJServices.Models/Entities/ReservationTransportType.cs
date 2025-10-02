using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities
{
    [Table("codesTRNTY")]
    public class ReservationTransportType
    {
        public string code { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }
}
