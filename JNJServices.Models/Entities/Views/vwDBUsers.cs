using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JNJServices.Models.Entities.Views
{
    [Table("vwDBUsers")]
    public class vwDBUsers
    {
        [JsonIgnore]
        public int UserNum { get; set; }
        public string? UserID { get; set; }
        public int inactiveflag { get; set; }
        public string? UserName { get; set; }
        [JsonIgnore]
        public string? email { get; set; }
        public DateTime LastLoginDate { get; set; }
        [JsonIgnore]
        public int groupnum { get; set; }
        [JsonIgnore]
        public string GroupID { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        [JsonIgnore]
        public string AuthToken { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }
}
