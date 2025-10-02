using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;

namespace JNJServices.Business.Abstracts
{
    public interface IContractorService
    {
        Task<IEnumerable<vwContractorSearch>> ContractorSearch(ContractorSearchViewModel model);
        Task<IEnumerable<ContractorServiceType>> GetContractorService();
        Task<IEnumerable<ContractorStatus>> GetContractorStatus();
        Task<IEnumerable<ContractorServiceLocation>> ContractorServiceLoc(ContractorIDWebViewModel model);
        Task<IEnumerable<ContractorAvailableSearchWebResponseModel>> ContractorAvlSearch(ContractorAvailaleSearchWebViewModel model);
        Task<IEnumerable<ContractorAvailableSearchWebResponseModel>> ApprovedContractorAvailableSearch(ContractorAvailaleSearchWebViewModel model);
        Task<IEnumerable<ContractorRates>> ContractorRates(ContractorRatesSearchViewModel model);
        Task<IEnumerable<ContractorRatesDetails>> ContractorRateDetails(ContractorRatesDetailSearchViewModel model);
        Task<IEnumerable<ContractorsAvailableHours>> ContractorAvailablehours(ContractorIDWebViewModel model);
        Task<IEnumerable<vwContractorDriversSearch>> ContractorDriver(ContractorDriverSearchViewModel model);
        Task<IEnumerable<vwContractorVehicleSearch>> ContractorVehicle(ContractorVehicleSearchWebViewModel model);
        Task<IEnumerable<vwContractorLanguageSearch>> ContractorLang(ContractorIDWebViewModel model);
        Task<IEnumerable<ContractorShowSelectiveWebResponseModel>> ContractorSelectiveDetails(ContractorIDWebViewModel model);
        Task<IEnumerable<ContractorJobSearch>> ContractorAssignmentJobStatus(AssignmentIDWebViewModel model);
        Task<IEnumerable<ContractorDynamicSearchWebResponseModel>> AllContractor(ContractorDynamicWebViewModel model);
        Task<(int, string)> ContractorWebJobSearch(ContractorJobSearchWebViewModel model);
        Task<(int ResponseCode, string Msg)> UpsertContractorMedia(ContractorMediaViewModel model);
        Task<ContractorMediaResponseModel> GetContractorMediaAsync(int contractorId);
        Task<(int, string, string)> preferredContractorSearch(ContractorJobSearchWebViewModel model);
        Task<IEnumerable<ContractorAvailableSearchWebResponseModel>> UnfilteredContractorSearch(ContractorAvailaleSearchWebViewModel model);
        Task<IEnumerable<ContractorListResponseModel>> ContractorListSearch(ContractorSearchViewModel model);
    }
}
