using JNJServices.Models.ApiResponseModels.App;

namespace JNJServices.Models.ApiResponseModels.Web
{
    public class WebMetricsResponseModel
    {
        public WebMetricsResponseModel()
        {
            metricsResponse = new List<AssignmentMetricsResponse>();
            MetricsUploadedDocuments = new List<UploadedFileInfo>();
        }

        public List<AssignmentMetricsResponse> metricsResponse { get; set; }
        public List<UploadedFileInfo> MetricsUploadedDocuments { get; set; }
    }
}
