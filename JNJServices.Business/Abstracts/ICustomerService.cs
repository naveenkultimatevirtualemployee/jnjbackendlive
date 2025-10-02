using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;

namespace JNJServices.Business.Abstracts
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerCategory>> GetCategory();
        Task<IEnumerable<vwCustomerSearch>> CustomerSearch(CustomerSearchWebViewModel model);
        Task<IEnumerable<CustomerDynamicSearchWebResponseModel>> AllCustomers(CustomerDynamicSearchWebViewModel model);
        Task<IEnumerable<vwCustomerSearch>> CustomerByClaimantSearch(ClaimantCustomerSearchWebViewModel model);
    }
}
