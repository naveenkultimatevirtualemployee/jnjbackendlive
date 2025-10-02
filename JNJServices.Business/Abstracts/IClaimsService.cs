using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;

namespace JNJServices.Business.Abstracts
{
    public interface IClaimsService
    {
        Task<IEnumerable<vwClaimsSearch>> ClaimsSearch(ClaimsSearchWebViewModel model);
        Task<IEnumerable<vwClaimsFacilities>> ClaimsFacility(ClaimsIDWebViewModel model);
        Task<IEnumerable<ClaimsApprovedContractorWebResponseModel>> ClaimsApprovedContractors(ClaimsIDWebViewModel model);
        Task<IEnumerable<ClaimsListResponseModel>> ClaimsListSearch(ClaimsSearchWebViewModel model);
    }
}
