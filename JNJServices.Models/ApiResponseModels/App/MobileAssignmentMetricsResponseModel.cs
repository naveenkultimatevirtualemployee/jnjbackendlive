namespace JNJServices.Models.ApiResponseModels.App
{
	public class MobileAssignmentMetricsResponseModel
	{
		public MobileAssignmentMetricsResponseModel()
		{
			metricsResponse = new List<AssignmentMetricsResponse>();
			WaitingRecordsList = new List<AssignmentTrackOtherRecordResponseModel>();
			MetricsUploadedDocuments = new List<UploadedFileInfo>();
		}

		public List<AssignmentMetricsResponse> metricsResponse { get; set; }
		public List<AssignmentTrackOtherRecordResponseModel> WaitingRecordsList { get; set; }
		public List<UploadedFileInfo> MetricsUploadedDocuments { get; set; }
	}

	public class AssignmentMetricsResponse
	{
		public AssignmentMetricsResponse()
		{
			code = description = Quantity = metricunitofmeasure = expectedQty = Notes = AssgnNum = billunitofmeasure = rate = string.Empty;
		}

		public string AssgnNum { get; set; }
		public string code { get; set; }
		public string description { get; set; }
		public string Quantity { get; set; }
		public string metricunitofmeasure { get; set; }
		public string expectedQty { get; set; }
		public string Notes { get; set; }
		public string billunitofmeasure { get; set; }
		public string rate { get; set; }
	}
}
