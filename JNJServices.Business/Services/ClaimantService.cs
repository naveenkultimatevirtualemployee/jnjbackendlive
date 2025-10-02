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
    public class ClaimantService : IClaimantService
    {
        private readonly IDapperContext _context;
        public ClaimantService(IDapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<vwClaimantSearch>> ClaimantSearch(ClaimantSearchViewModel model)
        {
            string procedureName = ProcEntities.spClaimantSearch;
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(model.FirstName))
                parameters.Add(DbParams.FirstName, model.FirstName, DbType.String);

            if (!string.IsNullOrEmpty(model.LastName))
                parameters.Add(DbParams.LastName, model.LastName, DbType.String);

            if (!string.IsNullOrEmpty(model.Email))
                parameters.Add(DbParams.Email, model.Email, DbType.String);

            if (!string.IsNullOrEmpty(model.Mobile))
                parameters.Add(DbParams.Mobile, model.Mobile, DbType.String);

            if (!string.IsNullOrEmpty(model.LanguageCode))
                parameters.Add(DbParams.Language, model.LanguageCode, DbType.String);

            if (model.ClaimantID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimantID, model.ClaimantID, DbType.Int32);

            if (model.CustomerID.ToValidateIntWithZero())
                parameters.Add(DbParams.CustomerID, model.CustomerID, DbType.Int32);

            if (model.Inactive.ToValidateInActiveFlag())
                parameters.Add(DbParams.InActive, model.Inactive, DbType.String);

            if (model.ZipCode.ToValidateIntWithZero())
                parameters.Add(DbParams.ZipCode, model.ZipCode, DbType.Int32);

            if (model.Miles.ToValidateIntWithZero())
                parameters.Add(DbParams.Miles, model.Miles, DbType.Int32);

            parameters.Add(DbParams.Page, model.Page.HasValue ? model.Page.Value : DefaultAppSettings.PageSize, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit.HasValue ? model.Limit.Value : DefaultAppSettings.PageLimit, DbType.Int32);

            var response = await _context.ExecuteQueryAsync<vwClaimantSearch>(procedureName, parameters, CommandType.StoredProcedure);
            return response;
        }

        public async Task<IEnumerable<vwClaimantSearch>> AssignCustomerInfoToClaimants(IEnumerable<vwClaimantSearch> claimants, int customerId)
        {
            string query = "SELECT CustomerName, CompanyName FROM vwCustomerSearch WHERE CustomerID = @CustomerID";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.CustomerID, customerId);

            var result = await _context.ExecuteQueryFirstOrDefaultAsync<CustomerClaimantsInfoWebResponseModel>(query, parameters, CommandType.Text);

            foreach (var claimantItem in claimants)
            {
                claimantItem.CustomerName = result?.CustomerName ?? string.Empty;
                claimantItem.companyName = result?.CompanyName ?? string.Empty;
            }
            return claimants;
        }

        public async Task<IEnumerable<vwClaimantSearch>> ClaimantByCustomerSearch(ClaimantCustomerSearchWebViewModel model)
        {
            var parameters = new DynamicParameters();

            string query = "SELECT DISTINCT c.*, cs.CustomerName FROM vwClaimantSearch c INNER JOIN vwClaimantCustomerSearch cc ON c.ClaimantID = cc.ClaimantID INNER JOIN vwCustomerSearch cs ON cc.CustomerID = cs.CustomerID WHERE cc.CustomerID = @CustomerID;";
            parameters.Add(DbParams.CustomerID, model.CustomerID, DbType.Int32);

            return await _context.ExecuteQueryAsync<vwClaimantSearch>(query, parameters, CommandType.Text);
        }

        public async Task<IEnumerable<ClaimantDynamicSearchWebResponseModel>> AllClaimant(ClaimantDynamicSearchWebViewModel model)
        {
            var query = "SELECT ClaimantID, FullName + ' - ' + CAST(ClaimantID AS VARCHAR) AS FullName FROM vwClaimantSearch  WHERE FullName LIKE @FullName";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.FullName, $"{model.ClaimantName}%");

            return await _context.ExecuteQueryAsync<ClaimantDynamicSearchWebResponseModel>(query, parameters, CommandType.Text);
        }

        public async Task<IEnumerable<ClaimantListResponseModel>> ClaimantListSearch(ClaimantSearchViewModel model)
        {
            string procedureName = ProcEntities.spClaimantListSearch;
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(model.FirstName))
                parameters.Add(DbParams.FirstName, model.FirstName, DbType.String);

            if (!string.IsNullOrEmpty(model.LastName))
                parameters.Add(DbParams.LastName, model.LastName, DbType.String);

            if (!string.IsNullOrEmpty(model.Email))
                parameters.Add(DbParams.Email, model.Email, DbType.String);

            if (!string.IsNullOrEmpty(model.Mobile))
                parameters.Add(DbParams.Mobile, model.Mobile, DbType.String);

            if (!string.IsNullOrEmpty(model.LanguageCode))
                parameters.Add(DbParams.Language, model.LanguageCode, DbType.String);

            if (model.ClaimantID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimantID, model.ClaimantID, DbType.Int32);

            if (model.CustomerID.ToValidateIntWithZero())
                parameters.Add(DbParams.CustomerID, model.CustomerID, DbType.Int32);

            if (model.Inactive.ToValidateInActiveFlag())
                parameters.Add(DbParams.InActive, model.Inactive, DbType.String);

            if (model.ZipCode.ToValidateIntWithZero())
                parameters.Add(DbParams.ZipCode, model.ZipCode, DbType.Int32);

            if (model.Miles.ToValidateIntWithZero())
                parameters.Add(DbParams.Miles, model.Miles, DbType.Int32);

            parameters.Add(DbParams.Page, model.Page.HasValue ? model.Page.Value : DefaultAppSettings.PageSize, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit.HasValue ? model.Limit.Value : DefaultAppSettings.PageLimit, DbType.Int32);

            var response = await _context.ExecuteQueryAsync<ClaimantListResponseModel>(procedureName, parameters, CommandType.StoredProcedure);
            return response;
        }
    }
}