using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;

namespace JNJServices.Business.Abstracts
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationAppResponseModel>> MobileReservationSearch(ReservationSearchViewModel model);
        Task<IEnumerable<ReservationActionCode>> ReservationActionCode();
        Task<IEnumerable<ReservationsServices>> ReservationServices();
        Task<IEnumerable<ReservationTripType>> ReservationTripType();
        Task<IEnumerable<ReservationTransportType>> ReservationTransportType();
        Task<IEnumerable<ReservationSearchResponseModel>> ReservationSearch(ReservationSearchWebViewModel model);
        Task<IEnumerable<ReservationListResponseModel>> ReservationListSearch(ReservationSearchWebViewModel model);
    }
}
