using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.CommonModels;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;

namespace JNJServices.Business.Abstracts
{
    public interface IReservationAssignmentsService
    {
        Task<IEnumerable<AssignmentService>> ReservationAssignmentType();
        Task<IEnumerable<ReservationAssignmentAppResponseModel>> MobileReservationAssignmentSearch(AppReservationAssignmentSearchViewModel model);
        Task<IEnumerable<ReservationAssignmentAppResponseModel>> ProcessAssignmentsAsync(List<ReservationAssignmentAppResponseModel> assignments);
        Task<(int, string, List<ReservationAssignmentAcceptRejectAppViewModel>)> MobileAcceptRejectAssignment(ReservationAssignmentAcceptRejectAppViewModel assignmentId);
        Task<(int, string)> MobileCancelAfterAccept(UpdateAssignmentStatusViewModel assignment);
        Task<IEnumerable<TrackAssignmentByIDResponseModel>> TrackAssignmentByID(int assignmentId);
        Task<IEnumerable<TrackAssignmentByIDResponseModel>> RetrieveAssignmentAndContractorDetails(List<TrackAssignmentByIDResponseModel> trackAssignments);
        Task<IEnumerable<OngoingAssignmentTrackingResponseModel>> OnGoingAssignment(AppTokenDetails claims);
        Task LiveTraking(SaveLiveTrackingCoordinatesViewModel liveTraking);
        Task<List<Coordinate>> GetCoordinatesFromDatabase(TrackingAssignmentIDViewModel model);
        Task<(IEnumerable<AssignmentTrackingResponseModel>, int, string)> AssignmentTracking(ReservationAssignmentTrackingViewModel model);
        Task<IEnumerable<AssignmentTrackingResponseModel>> FetchOtherRecordsForTracking(List<AssignmentTrackingResponseModel> trackingRecords);
        Task<(int, string, IEnumerable<AssignmentTrackOtherRecordResponseModel>)> AssignmentTrackingOtherRecords(ReservationAssignmentTrackOtherRecordsViewModel model);
        Task<IEnumerable<ReservationCancelDetails>> ReservationAssignmentCancelStatus(int type);
        Task<string> SearchAssignmentPath(int assignmentId);
        Task SaveAssignmentPath(int assignmentId, string path, bool updateExisting);
        Task<List<Coordinate>> GetLiveTrackingCoordinates(TrackingAssignmentIDViewModel model);
        Task<List<Coordinate>> GetGooglePathCoordinates(int reservationsAssignmentsID);
        Task<(int, string)> UserFeedback(UserFeedbackAppViewModel model);
        Task<(int, string, bool)> CheckReservationAssignmentAsync(int reservationAssignmentId, string notes);
        Task<IEnumerable<UserFeedbackAppResponseModel>> ViewUserFeedback(AssignmentIDAppViewModel model);
        Task<IEnumerable<vwReservationAssignmentsSearch>> ReservationAssignmentSearch(ReservationAssignmentWebViewModel model);
        Task<IEnumerable<ReservationAssigncontractorWebResponseModel>> AssignAssignmentContractor(AssignAssignmentContractorWebViewModel model);
        Task<IEnumerable<TrackAssignmentByIDResponseModel>> WebTrackAssignmentByID(int assignmentId);

        Task<(List<ReservationAssignmentAppResponseModel>, int, string)> GetAllRelatedAssignmentsAsync(int reservationId);

        Task<(int ResponseCode, string Message, ReservationCheckTimeSlotResponseModel)> CheckReservationTimeSlotAsync(ReservationCheckTimeSlotViewModel model);

        Task<bool> SaveAssignmentDocument(int reservationsAssignmentsID, UploadedFileInfo file);
        Task<AppMilesRecordResponseModel> AssignmentMilesRecord(AppReservationAssignmentSearchViewModel model);
        Task<IEnumerable<ReservationAssignmentListResponseModel>> ReservationAssignmentListSearch(ReservationAssignmentWebViewModel model);
    }
}
