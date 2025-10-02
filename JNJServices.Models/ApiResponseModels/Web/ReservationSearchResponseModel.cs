using JNJServices.Models.Entities.Views;

namespace JNJServices.Models.ApiResponseModels.Web
{
    public class ReservationSearchResponseModel : vwReservationSearch
    {
        public ReservationSearchResponseModel()
        {
            contractors = new List<ReservationSearchContractorListResponseModel>();
        }
        public List<ReservationSearchContractorListResponseModel> contractors { get; set; }
    }
}
