using Dapper;
using JNJServices.Business.Abstracts;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Utility.DbConstants;
using System.Data;

namespace JNJServices.Business.Services
{
    public class MiscellaneousService : IMiscellaneousService
    {
        private readonly IDapperContext _context;
        public MiscellaneousService(IDapperContext context)
        {
            _context = context;
        }

        public async Task<DashboardWebResponseModel> GetDashboardData()
        {
            DashboardWebResponseModel dashboardData = new DashboardWebResponseModel();

            var procedureName = ProcEntities.spDashboard;
            using (var connection = _context.CreateConnection())
            {
                using (var result = await connection.QueryMultipleAsync(procedureName, commandType: CommandType.StoredProcedure))
                {
                    var totalCounts = await result.ReadAsync<Counts>();
                    var reservationbyMonths = await result.ReadAsync<GraphByMonth>();
                    var reservationStatus = await result.ReadAsync<GraphByStatus>();
                    //var claimantByMonths = await result.ReadAsync<GraphByMonth>();
                    //var contractorStatus = await result.ReadAsync<GraphByStatus>();
                    //var contractorByMonths = await result.ReadAsync<GraphByMonth>();
                    //var reservationbyService = await result.ReadAsync<GraphByStatus>();
                    //var customerbyCategory = await result.ReadAsync<GraphByStatus>();
                    //var contractorbyService = await result.ReadAsync<GraphByStatus>();

                    dashboardData.counts = totalCounts.ToList();
                    dashboardData.reservationbyMonths.name = "Reservations By Month";
                    dashboardData.reservationbyMonths.data = reservationbyMonths.ToList();
                    dashboardData.reservationStatus.name = "Reservations By Action Code";
                    dashboardData.reservationStatus.data = reservationStatus.ToList();
                    //dashboardData.ClaimantByMonths.name = "Claimant By Month";
                    //dashboardData.ClaimantByMonths.data = claimantByMonths.ToList();
                    //dashboardData.ContractorStatus.name = "Contractor by Availability";
                    //dashboardData.ContractorStatus.data = contractorStatus.ToList();
                    //dashboardData.ContractorByMonths.name = "Contractor By Month";
                    //dashboardData.ContractorByMonths.data = contractorByMonths.ToList();
                    //dashboardData.ReservationbyService.name = "Reservations By Service";
                    //dashboardData.ReservationbyService.data = reservationbyService.ToList();
                    //dashboardData.CustomerbyCategory.name = "Customer By Category";
                    //dashboardData.CustomerbyCategory.data = customerbyCategory.ToList();
                    //dashboardData.ContractorbyService.name = "Contractor By Service";
                    //dashboardData.ContractorbyService.data = contractorbyService.ToList();
                }
                return dashboardData;
            }
        }

        public async Task<IEnumerable<VehicleLists>> VehicleList()
        {
            string query = "Select * From codesVEHSZ where inactiveflag = 0 order by description";

            return await _context.ExecuteQueryAsync<VehicleLists>(query, CommandType.Text);
        }

        public async Task<IEnumerable<Languages>> LanguageList()
        {
            string query = "Select * From codesLANGU where inactiveflag = 0 order by description";

            return await _context.ExecuteQueryAsync<Languages>(query, CommandType.Text);
        }

        public async Task<IEnumerable<States>> GetStates()
        {
            string query = "Select * From codesSTATE where inactiveflag = 0 order by description";

            return await _context.ExecuteQueryAsync<States>(query, CommandType.Text);

        }
    }
}
