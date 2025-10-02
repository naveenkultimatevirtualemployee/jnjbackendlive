namespace JNJServices.Models.ApiResponseModels
{
    public class UploadedFileInfo
    {
        public string FileName { get; set; } = string.Empty;  // Original file name (Unchanged)
        public string ContentType { get; set; } = string.Empty; // File MIME type
        public string FilePath { get; set; } = string.Empty;  // Saved file path
    }
}
