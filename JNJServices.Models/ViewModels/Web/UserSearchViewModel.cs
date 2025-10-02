using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class UserSearchViewModel
    {
        public string? UserID { get; set; }
        public string? UserName { get; set; }
        public int? Role { get; set; }
        public string? email { get; set; }
        public int? inactiveflag { get; set; }
        [Range(1, Int32.MaxValue)]
        public int? Page { get; set; } = 1;
        [Range(1, Int32.MaxValue)]
        public int? Limit { get; set; } = 20;
    }
}
