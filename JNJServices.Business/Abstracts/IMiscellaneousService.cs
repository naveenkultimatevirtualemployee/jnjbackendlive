using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;

namespace JNJServices.Business.Abstracts
{
    public interface IMiscellaneousService
    {
        Task<DashboardWebResponseModel> GetDashboardData();
        Task<IEnumerable<VehicleLists>> VehicleList();
        Task<IEnumerable<Languages>> LanguageList();
        Task<IEnumerable<States>> GetStates();
    }
}
