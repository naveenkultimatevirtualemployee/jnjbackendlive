using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;

namespace JNJServices.Business.Abstracts
{
    public interface IClaimantService
    {
        Task<IEnumerable<vwClaimantSearch>> ClaimantSearch(ClaimantSearchViewModel model);
        Task<IEnumerable<vwClaimantSearch>> AssignCustomerInfoToClaimants(IEnumerable<vwClaimantSearch> claimants, int customerId);
        Task<IEnumerable<ClaimantDynamicSearchWebResponseModel>> AllClaimant(ClaimantDynamicSearchWebViewModel model);
        Task<IEnumerable<vwClaimantSearch>> ClaimantByCustomerSearch(ClaimantCustomerSearchWebViewModel model);
        Task<IEnumerable<ClaimantListResponseModel>> ClaimantListSearch(ClaimantSearchViewModel model);
    }
}
