using AutoMapper;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities.Views;

namespace JNJServices.API.Mapper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<vwDBUsers, UserLoginWebResponseModel>();
            CreateMap<vwContractorSearch, ContractorSearchAppResponseModel>();
            CreateMap<vwClaimantSearch, ClaimantSearchAppResponseModel>();
            CreateMap<vwReservationAssignmentsSearch, ReservationSearchContractorListResponseModel>();
            CreateMap<vwClaimantSearch, ClaimantFullNameResponseModel>();
            CreateMap<vwCustomerSearch, CustomerCompanyNameWebResponseModel>();
        }
    }
}
