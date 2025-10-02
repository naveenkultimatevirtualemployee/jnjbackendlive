using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities.Views
{
    [Table("vwCustomerSearch")]
    public class vwCustomerSearch
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public int inactiveflag { get; set; }
        public string Lastname { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string TollfreePhone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string MailingName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string LeadSourceName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }
}
