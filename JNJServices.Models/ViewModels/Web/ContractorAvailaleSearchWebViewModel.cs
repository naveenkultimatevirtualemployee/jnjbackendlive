using System.ComponentModel.DataAnnotations;

namespace JNJServices.Models.ViewModels.Web
{
	public class ContractorAvailaleSearchWebViewModel
	{
		[Required]
		public int? reservationid { get; set; }
		[Required]
		public int? reservationassignmentsid { get; set; }
		public string? zipCode { get; set; }
		public double? Miles { get; set; }
		public string? vehsize { get; set; }
		public string? language { get; set; }
		public string? vehtype { get; set; }
		public string? certified { get; set; }
		public int? ContractorID { get; set; }
		public string? status { get; set; }
		public bool? IsWebSearch { get; set; }
		public bool? unfilteredSearch { get; set; }
	}
}
