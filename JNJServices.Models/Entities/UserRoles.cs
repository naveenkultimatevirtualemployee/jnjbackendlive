using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities
{
    [Table("DBUserGroups")]
    public class UserRoles
    {
        public int GroupNum { get; set; }
        public string GroupID { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
    }
}
