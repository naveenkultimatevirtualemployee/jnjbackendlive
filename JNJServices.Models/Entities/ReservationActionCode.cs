using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities
{
    [Table("codesRSVAC")]
    public class ReservationActionCode
    {
        public string code { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }
}
