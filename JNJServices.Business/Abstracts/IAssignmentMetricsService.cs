using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;

namespace JNJServices.Business.Abstracts
{
    public interface IAssignmentMetricsService
    {
        Task<List<AssignmentMetricsResponse>> FetchAssignmentMetrics(int ReservationsAssignmentsID);
        Task<List<AssignmentTrackOtherRecordResponseModel>> FetchWaitingRecords(int ReservationsAssignmentsID);
        Task InsertAssignmentMetricsFromJsonAsync(List<SaveAssignmentMetricsViewModel> metricsList);
        Task<int> InsertReservationAssignmentTrackRecordMetrics(SaveAssignmentTrackRecordViewModel model);
        Task<int> DeleteReservationAssignmentTrackRecordMetrics(int waitingId);
        Task<IEnumerable<vwReservationAssignmentsSearch>> MetricsSearch(AssignmentMetricsSearchWebViewModel model);
        Task<List<UploadedFileInfo>> FetchMetricsUploadedDocuments(int ReservationsAssignmentsID);
        Task<IEnumerable<MetricsListResponseModel>> MetricsListSearch(AssignmentMetricsSearchWebViewModel model);
    }
}
