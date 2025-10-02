using Dapper;
using JNJServices.Business.Abstracts;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.ApiConstants;
using JNJServices.Utility.DbConstants;
using JNJServices.Utility.Extensions;
using System.Data;

namespace JNJServices.Business.Services
{
    public class ClaimsService : IClaimsService
    {
        private readonly IDapperContext _context;
        public ClaimsService(IDapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<vwClaimsSearch>> ClaimsSearch(ClaimsSearchWebViewModel model)
        {
            string procedureName = ProcEntities.spClaimsSearch;
            var parameters = new DynamicParameters();

            if (model.ClaimID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimID, model.ClaimID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.ClaimNumber))
                parameters.Add(DbParams.ClaimNumber, model.ClaimNumber, DbType.String);

            if (model.CustomerID.ToValidateIntWithZero())
                parameters.Add(DbParams.CustomerID, model.CustomerID, DbType.Int32);

            if (model.ClaimantID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimantID, model.ClaimantID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.Birthdate))
                parameters.Add(DbParams.Birthdate, Convert.ToDateTime(model.Birthdate).ToString("yyyy-MM-dd"), DbType.DateTime);

            parameters.Add(DbParams.Page, model.Page.HasValue ? model.Page : DefaultAppSettings.PageSize, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit.HasValue ? model.Limit : DefaultAppSettings.PageLimit, DbType.Int32);

            return await _context.ExecuteQueryAsync<vwClaimsSearch>(procedureName, parameters, CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<vwClaimsFacilities>> ClaimsFacility(ClaimsIDWebViewModel model)
        {
            string query = "Select * From vwClaimFacilitySearch Where (@claimId IS NULL OR ClaimID=@claimid)";

            var parameter = new DynamicParameters();
            if (model.ClaimID.ToValidateIntWithZero())
                parameter.Add(DbParams.ClaimID, model.ClaimID, DbType.Int32);

            return await _context.ExecuteQueryAsync<vwClaimsFacilities>(query, parameter, CommandType.Text);
        }

        public async Task<IEnumerable<ClaimsApprovedContractorWebResponseModel>> ClaimsApprovedContractors(ClaimsIDWebViewModel model)
        {
            string query = "Select cac.*,CONCAT(con.FirstName ,' ', con.MiddleName,' ' ,con.LastName) as ContractorName,con.Service as Service From ClaimsApprovedContractors cac join vwContractorSearch con on cac.ContractorID = con.ContractorID Where (@claimId IS NULL OR cac.ClaimID=@claimid)";

            var parameter = new DynamicParameters();
            if (model.ClaimID.ToValidateIntWithZero())
                parameter.Add(DbParams.ClaimID, model.ClaimID, DbType.Int32);

            return await _context.ExecuteQueryAsync<ClaimsApprovedContractorWebResponseModel>(query, parameter, CommandType.Text);
        }

        public async Task<IEnumerable<ClaimsListResponseModel>> ClaimsListSearch(ClaimsSearchWebViewModel model)
        {
            string procedureName = ProcEntities.spClaimsListSearch;
            var parameters = new DynamicParameters();

            if (model.ClaimID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimID, model.ClaimID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.ClaimNumber))
                parameters.Add(DbParams.ClaimNumber, model.ClaimNumber, DbType.String);

            if (model.CustomerID.ToValidateIntWithZero())
                parameters.Add(DbParams.CustomerID, model.CustomerID, DbType.Int32);

            if (model.ClaimantID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimantID, model.ClaimantID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.Birthdate))
                parameters.Add(DbParams.Birthdate, Convert.ToDateTime(model.Birthdate).ToString("yyyy-MM-dd"), DbType.DateTime);

            parameters.Add(DbParams.Page, model.Page.HasValue ? model.Page : DefaultAppSettings.PageSize, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit.HasValue ? model.Limit : DefaultAppSettings.PageLimit, DbType.Int32);

            return await _context.ExecuteQueryAsync<ClaimsListResponseModel>(procedureName, parameters, CommandType.StoredProcedure);
        }
    }
}
