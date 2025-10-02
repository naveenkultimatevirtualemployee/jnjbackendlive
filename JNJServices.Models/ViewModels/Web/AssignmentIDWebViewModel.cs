using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
    public class AssignmentIDWebViewModel
    {
        [Required]
        public int? ReservationsAssignmentsID { get; set; }
    }
}
