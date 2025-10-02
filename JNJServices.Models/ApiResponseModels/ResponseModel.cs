namespace JNJServices.Models.ApiResponseModels
{
    public class ResponseModel
    {
        public ResponseModel()
        {
            status = false;
            statusMessage = string.Empty;
            data = string.Empty;
        }

        public bool status { get; set; }
        public string? statusMessage { get; set; }
        public object data { get; set; }
    }

    public class PaginatedResponseModel
    {
        public PaginatedResponseModel()
        {
            status = false;
            statusMessage = string.Empty;
            data = string.Empty;
        }

        public bool status { get; set; }
        public string? statusMessage { get; set; }
        public object data { get; set; }
        public int totalData { get; set; }
    }
}
