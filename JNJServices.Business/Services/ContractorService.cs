using Dapper;
using JNJServices.Business.Abstracts;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.ApiConstants;
using JNJServices.Utility.DbConstants;
using JNJServices.Utility.Extensions;
using System.Data;

namespace JNJServices.Business.Services
{
    public class ContractorService : IContractorService
    {
        private readonly IDapperContext _context;
        public ContractorService(IDapperContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<vwContractorSearch>> ContractorSearch(ContractorSearchViewModel model)
        {
            var parameters = new DynamicParameters();

            if (model.ContractorID.ToValidateIntWithZero())
                parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.FirstName))
                parameters.Add(DbParams.FirstName, model.FirstName, DbType.String);

            if (!string.IsNullOrEmpty(model.LastName))
                parameters.Add(DbParams.LastName, model.LastName, DbType.String);

            if (!string.IsNullOrEmpty(model.Mobile))
                parameters.Add(DbParams.Mobile, model.Mobile, DbType.String);

            if (!string.IsNullOrEmpty(model.StatusCode))
                parameters.Add(DbParams.StatusCode, model.StatusCode, DbType.String);

            if (!string.IsNullOrEmpty(model.ServiceCode))
                parameters.Add(DbParams.ServiceCode, model.ServiceCode, DbType.String);

            if (!string.IsNullOrEmpty(model.State))
                parameters.Add(DbParams.State, model.State, DbType.String);

            if (!string.IsNullOrEmpty(model.VehicleSize))
                parameters.Add(DbParams.VehicleSize, model.VehicleSize, DbType.String);

            if (!string.IsNullOrEmpty(model.LanguageCode))
                parameters.Add(DbParams.Language, model.LanguageCode, DbType.String);

            if (model.inactiveflag.ToValidateInActiveFlag())
                parameters.Add(DbParams.InactiveFlag, model.inactiveflag, DbType.Int32);

            if (model.ZipCode.ToValidateIntWithZero())
                parameters.Add(DbParams.ZipCode, model.ZipCode, DbType.Int32);

            if (model.Miles.ToValidateIntWithZero())
                parameters.Add(DbParams.Miles, model.Miles, DbType.Int32);

            parameters.Add(DbParams.Page, model.Page ?? DefaultAppSettings.PageSize, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit ?? DefaultAppSettings.PageLimit, DbType.Int32);

            return await _context.ExecuteQueryAsync<vwContractorSearch>(ProcEntities.spContractorsSearch, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ContractorServiceType>> GetContractorService()
        {
            string query = "Select * From codesCONTY where inactiveflag = 0 order by description";

            return await _context.ExecuteQueryAsync<ContractorServiceType>(query, CommandType.Text);
        }

        public async Task<IEnumerable<ContractorStatus>> GetContractorStatus()
        {
            string query = "Select * From codesCONST where inactiveflag = 0 order by description";

            return await _context.ExecuteQueryAsync<ContractorStatus>(query, CommandType.Text);
        }

        public async Task<IEnumerable<ContractorServiceLocation>> ContractorServiceLoc(ContractorIDWebViewModel model)
        {
            string query = "SELECT * FROM ContractorsServiceLoc WHERE (@ContractorID IS NULL OR ContractorID = @ContractorID)";
            var parameters = new DynamicParameters();

            if (model.ContractorID != null && model.ContractorID > 0)
                parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);

            return await _context.ExecuteQueryAsync<ContractorServiceLocation>(query, parameters, CommandType.Text);
        }

        public async Task<IEnumerable<ContractorAvailableSearchWebResponseModel>> ContractorAvlSearch(ContractorAvailaleSearchWebViewModel model)
        {
            string procedureName = ProcEntities.spRsvAssignContractorSearch;

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationID, model.reservationid, DbType.String);
            parameters.Add(DbParams.ReservationAssignmentsID, model.reservationassignmentsid, DbType.String);
            parameters.Add(DbParams.ZipCode, model.zipCode, DbType.String);
            parameters.Add(DbParams.Miles, model.Miles, DbType.Double);
            parameters.Add(DbParams.VehSize, model.vehsize, DbType.String);
            parameters.Add(DbParams.Language, model.language, DbType.String);
            parameters.Add(DbParams.Type, model.vehtype, DbType.String);
            parameters.Add(DbParams.Certified, model.certified, DbType.String);
            parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);
            parameters.Add(DbParams.Status, model.status, DbType.String);

            return await _context.ExecuteQueryAsync<ContractorAvailableSearchWebResponseModel>(procedureName, parameters, CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ContractorAvailableSearchWebResponseModel>> ApprovedContractorAvailableSearch(ContractorAvailaleSearchWebViewModel model)
        {
            string procedureName = ProcEntities.spAssignmentApprovedContractorSearch;

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationID, model.reservationid, DbType.String);
            parameters.Add(DbParams.ReservationAssignmentsID, model.reservationassignmentsid, DbType.String);
            parameters.Add(DbParams.ZipCode, model.zipCode, DbType.String);
            parameters.Add(DbParams.Miles, model.Miles, DbType.Int32);
            parameters.Add(DbParams.VehSize, model.vehsize, DbType.String);
            parameters.Add(DbParams.Language, model.language, DbType.String);
            parameters.Add(DbParams.Type, model.vehtype, DbType.String);
            parameters.Add(DbParams.Certified, model.certified, DbType.String);
            parameters.Add(DbParams.Status, model.status, DbType.String);
            parameters.Add(DbParams.IsWebSearch, model.IsWebSearch, DbType.Boolean);

            return await _context.ExecuteQueryAsync<ContractorAvailableSearchWebResponseModel>(procedureName, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ContractorRates>> ContractorRates(ContractorRatesSearchViewModel model)
        {
            string query = "Select * From ContractorRates where 1=1 ";

            var parameters = new DynamicParameters();
            if (!string.IsNullOrEmpty(model.RateCode))
            {
                query += " and RATECTCode = @Ratecode ";
                parameters.Add(DbParams.RateCode, model.RateCode, DbType.String);
            }
            if (!string.IsNullOrEmpty(model.StateCode))
            {
                query += " and STATECode = @STATEcode ";
                parameters.Add(DbParams.StateCode, model.StateCode, DbType.String);
            }
            if (!string.IsNullOrEmpty(model.EffectiveDate))
            {
                query += " and EffectiveDateFrom = @EffectiveDate ";
                parameters.Add(DbParams.EffectiveDate, model.EffectiveDate, DbType.String);
            }
            if (model.Inactiveflag.ToValidateInActiveFlag())
            {
                query += " and inactiveflag = @Inactiveflag ";
                parameters.Add(DbParams.InactiveFlag, model.Inactiveflag, DbType.String);
            }

            return await _context.ExecuteQueryAsync<ContractorRates>(query, parameters, commandType: CommandType.Text);
        }

        public async Task<IEnumerable<ContractorRatesDetails>> ContractorRateDetails(ContractorRatesDetailSearchViewModel model)
        {
            string query = "Select crd.*,ca.billunitofmeasure as UOM ,ca.LOBCode as LOB ,lang.description as Language,ct.description as TRNTYName   FROM [ContractorRatesDet] as crd " +
            "inner join codesACCTG as ca on crd.ACCTGCode = ca.code  " +
            "left Join codesLANGU as lang  on crd.LANGUCode =lang.code " +
            "left join codesTRNTY as ct on crd.TRNTYCode = ct.code " +
            "where 1=1 ";

            var parameters = new DynamicParameters();
            if (model.ContractorRatesID.ToValidateIntWithZero())
            {
                query += " and crd.ContractorRatesID = @ContractorRatesID ";
                parameters.Add(DbParams.ContractorRatesID, model.ContractorRatesID, DbType.Int32);
            }
            if (!string.IsNullOrEmpty(model.ACCTGCode))
            {
                query += " and crd.ACCTGCode = @ACCTGCode ";
                parameters.Add(DbParams.ACCTGCode, model.ACCTGCode, DbType.String);
            }
            if (!string.IsNullOrEmpty(model.TransType))
            {
                query += " and crd.TRNTYCode = @TRNTYCode ";
                parameters.Add(DbParams.TRNTYCode, model.TransType, DbType.String);
            }
            if (!string.IsNullOrEmpty(model.Language))
            {
                query += " and crd.LANGUCode = @LANGUCode ";
                parameters.Add(DbParams.LANGUCode, model.Language, DbType.String);
            }
            if (!string.IsNullOrEmpty(model.LOB))
            {
                query += " and ca.LOBCode = @LOBCode ";
                parameters.Add(DbParams.LOBCode, model.LOB, DbType.String);
            }

            var result = await _context.ExecuteQueryAsync<ContractorRatesDetails>(query, parameters, commandType: CommandType.Text);
            return result;
        }

        public async Task<IEnumerable<ContractorsAvailableHours>> ContractorAvailablehours(ContractorIDWebViewModel model)
        {
            string query = "SELECT  ContractorID ,AvailableDayNum ,Convert(Time,StartTime) as StartTime ,Convert(Time,EndTime) as  EndTime,CreateDate ,CreateUserID ,LastChangeDate ,LastChangeUserID ,archiveflag  FROM ContractorsAvailableHours WHERE (@ContractorID IS NULL OR ContractorID = @ContractorID)";
            var parameter = new DynamicParameters();

            if (model.ContractorID.ToValidateIntWithZero())
                parameter.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);

            return await _context.ExecuteQueryAsync<ContractorsAvailableHours>(query, parameter, CommandType.Text);
        }

        public async Task<IEnumerable<vwContractorDriversSearch>> ContractorDriver(ContractorDriverSearchViewModel model)
        {
            string query = "SELECT  *  FROM vwContractorDriversSearch where ContractorID=@ContractorID ";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);

            if (model.IsPrimary != null)
            {
                query += " and IsPrimary = @IsPrimary";
                parameters.Add(DbParams.IsPrimary, model.IsPrimary, DbType.Int32);
            }

            return await _context.ExecuteQueryAsync<vwContractorDriversSearch>(query, parameters, CommandType.Text);
        }

        public async Task<IEnumerable<vwContractorVehicleSearch>> ContractorVehicle(ContractorVehicleSearchWebViewModel model)
        {
            string query = "SELECT * FROM vwContractorVehicleSearch where ContractorID=@ContractorID ";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);

            if (model.IsPrimary != null)
            {
                query += " and IsPrimary = @IsPrimary";
                parameters.Add(DbParams.IsPrimary, model.IsPrimary, DbType.Int32);
            }

            return await _context.ExecuteQueryAsync<vwContractorVehicleSearch>(query, parameters, CommandType.Text);
        }

        public async Task<IEnumerable<vwContractorLanguageSearch>> ContractorLang(ContractorIDWebViewModel model)
        {
            string query = "SELECT * FROM vwContractorLangSearch WHERE (@ContractorID IS NULL OR ContractorID = @ContractorID)";
            var parameters = new DynamicParameters();

            if (model.ContractorID.ToValidateIntWithZero())
            {
                parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);
            }

            return await _context.ExecuteQueryAsync<vwContractorLanguageSearch>(query, parameters, CommandType.Text);
        }

        public async Task<IEnumerable<ContractorShowSelectiveWebResponseModel>> ContractorSelectiveDetails(ContractorIDWebViewModel model)
        {
            string query = "SELECT Company FROM vwContractorSearch WHERE (@ContractorID IS NULL OR ContractorID = @ContractorID)";
            var parameters = new DynamicParameters();

            if (model.ContractorID.ToValidateIntWithZero())
            {
                parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);
            }

            return await _context.ExecuteQueryAsync<ContractorShowSelectiveWebResponseModel>(query, parameters, CommandType.Text);
        }

        public async Task<IEnumerable<ContractorJobSearch>> ContractorAssignmentJobStatus(AssignmentIDWebViewModel model)
        {
            string query = "Select cj.*,ra.EstimatedMinutes AS 'EstimateMinutes' , ra.Quantity AS 'EstimateMiles'  FROM ContractorJobSearch cj JOIN ReservationsAssignments ra ON cj.ReservationsAssignmentsID = ra.ReservationsAssignmentsID where cj.ReservationsAssignmentsID = @ReservationsAssignmentsID  order by JobStatus desc";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationsAssignmentsID, model.ReservationsAssignmentsID, DbType.Int32);

            return await _context.ExecuteQueryAsync<ContractorJobSearch>(query, parameters, CommandType.Text);

        }

        public async Task<IEnumerable<ContractorDynamicSearchWebResponseModel>> AllContractor(ContractorDynamicWebViewModel model)
        {
            var query = "SELECT ContractorID, FullName + ' - ' + CAST(ContractorID AS VARCHAR) AS FullName FROM vwContractorSearch WHERE FullName LIKE @FullName";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.FullName, $"{model.ContractorName}%");

            return await _context.ExecuteQueryAsync<ContractorDynamicSearchWebResponseModel>(query, parameters, CommandType.Text);
        }

        public async Task<(int, string)> ContractorWebJobSearch(ContractorJobSearchWebViewModel model)
        {
            var procedureName = ProcEntities.spWebContractorJobSearch;

            var parameter = new DynamicParameters();
            parameter.Add(DbParams.ReservationID, model.ReservationID, DbType.Int32);
            parameter.Add(DbParams.AssignmentID, model.ReservationAssignmentsID, DbType.Int32);

            parameter.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);

            await _context.ExecuteScalerAsync(procedureName, parameter, CommandType.StoredProcedure);

            var responseCode = parameter.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameter.Get<string?>(DbParams.Msg) ?? "Contractor Entered Already";
            return (responseCode, message.ToString());
        }

        public async Task<(int ResponseCode, string Msg)> UpsertContractorMedia(ContractorMediaViewModel model)
        {
            var procedureName = ProcEntities.spUpdateContractorMedia;

            var parameter = new DynamicParameters();
            parameter.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);
            parameter.Add(DbParams.ProfileImageBase64, model.ProfileImageBase64, DbType.String);
            parameter.Add(DbParams.ProfileImageUrl, model.ProfileImageUrl, DbType.String);
            parameter.Add(DbParams.ResponseCode, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add(DbParams.Msg, dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

            await _context.ExecuteScalerAsync(procedureName, parameter, CommandType.StoredProcedure);

            int responseCode = parameter.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameter.Get<string>(DbParams.Msg) ?? "Unknown Error";


            return (responseCode, message);
        }

        public async Task<ContractorMediaResponseModel> GetContractorMediaAsync(int contractorId)
        {

            string query = ProcEntities.spGetContractorMedia;

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ContractorID, contractorId, DbType.Int32);

            return await _context.ExecuteQueryFirstOrDefaultAsync<ContractorMediaResponseModel>(
                query,
                parameters,
                 CommandType.StoredProcedure
            ) ?? new ContractorMediaResponseModel();
        }

        public async Task<(int, string, string)> preferredContractorSearch(ContractorJobSearchWebViewModel model)
        {
            var procedureName = ProcEntities.spPreferredContractor;

            var parameter = new DynamicParameters();
            parameter.Add(DbParams.ReservationID, model.ReservationID, DbType.Int32);
            parameter.Add(DbParams.AssignmentID, model.ReservationAssignmentsID, DbType.Int32);

            parameter.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);
            parameter.Add(DbParams.AssgnNum, "", dbType: DbType.String, direction: ParameterDirection.Output);

            await _context.ExecuteScalerAsync(procedureName, parameter, CommandType.StoredProcedure);

            var responseCode = parameter.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameter.Get<string?>(DbParams.Msg) ?? "Contractor Entered Already";
            string AssgnNum = parameter.Get<string?>(DbParams.AssgnNum) ?? "";
            return (responseCode, message.ToString(), AssgnNum);
        }

        public async Task<IEnumerable<ContractorAvailableSearchWebResponseModel>> UnfilteredContractorSearch(ContractorAvailaleSearchWebViewModel model)
        {
            string procedureName = ProcEntities.spAssignmentContractorRadiusSearch;

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationID, model.reservationid, DbType.String);
            parameters.Add(DbParams.ReservationAssignmentsID, model.reservationassignmentsid, DbType.String);
            parameters.Add(DbParams.ZipCode, model.zipCode, DbType.String);
            parameters.Add(DbParams.Miles, model.Miles, DbType.Double);
            parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);

            return await _context.ExecuteQueryAsync<ContractorAvailableSearchWebResponseModel>(procedureName, parameters, CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ContractorListResponseModel>> ContractorListSearch(ContractorSearchViewModel model)
        {
            var parameters = new DynamicParameters();

            if (model.ContractorID.ToValidateIntWithZero())
                parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.FirstName))
                parameters.Add(DbParams.FirstName, model.FirstName, DbType.String);

            if (!string.IsNullOrEmpty(model.LastName))
                parameters.Add(DbParams.LastName, model.LastName, DbType.String);

            if (!string.IsNullOrEmpty(model.Mobile))
                parameters.Add(DbParams.Mobile, model.Mobile, DbType.String);

            if (!string.IsNullOrEmpty(model.StatusCode))
                parameters.Add(DbParams.StatusCode, model.StatusCode, DbType.String);

            if (!string.IsNullOrEmpty(model.ServiceCode))
                parameters.Add(DbParams.ServiceCode, model.ServiceCode, DbType.String);

            if (!string.IsNullOrEmpty(model.State))
                parameters.Add(DbParams.State, model.State, DbType.String);

            if (!string.IsNullOrEmpty(model.VehicleSize))
                parameters.Add(DbParams.VehicleSize, model.VehicleSize, DbType.String);

            if (!string.IsNullOrEmpty(model.LanguageCode))
                parameters.Add(DbParams.Language, model.LanguageCode, DbType.String);

            if (model.inactiveflag.ToValidateInActiveFlag())
                parameters.Add(DbParams.InactiveFlag, model.inactiveflag, DbType.Int32);

            if (model.ZipCode.ToValidateIntWithZero())
                parameters.Add(DbParams.ZipCode, model.ZipCode, DbType.Int32);

            if (model.Miles.ToValidateIntWithZero())
                parameters.Add(DbParams.Miles, model.Miles, DbType.Int32);

            parameters.Add(DbParams.Page, model.Page ?? DefaultAppSettings.PageSize, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit ?? DefaultAppSettings.PageLimit, DbType.Int32);

            return await _context.ExecuteQueryAsync<ContractorListResponseModel>(ProcEntities.spContractorsListSearch, parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
