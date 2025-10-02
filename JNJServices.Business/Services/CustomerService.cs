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
    public class CustomerService : ICustomerService
    {
        private readonly IDapperContext _context;
        public CustomerService(IDapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerCategory>> GetCategory()
        {
            string query = "Select * From codesCUSCT where inactiveflag = 0 order by description";

            return await _context.ExecuteQueryAsync<CustomerCategory>(query, CommandType.Text);
        }

        public async Task<IEnumerable<vwCustomerSearch>> CustomerSearch(CustomerSearchWebViewModel model)
        {
            string procedureName = ProcEntities.spCustomerSearch;

            var parameters = new DynamicParameters();

            if (model.CustomerID.ToValidateIntWithZero())
                parameters.Add(DbParams.CustomerID, model.CustomerID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.CustomerName))
                parameters.Add(DbParams.CustomerName, model.CustomerName, DbType.String);

            if (!string.IsNullOrEmpty(model.Email))
                parameters.Add(DbParams.Email, model.Email, DbType.String);

            if (!string.IsNullOrEmpty(model.States) && !model.States.Equals("all", StringComparison.OrdinalIgnoreCase))
                parameters.Add(DbParams.StateCode, model.States, DbType.String);

            if (!string.IsNullOrEmpty(model.Category) && !model.Category.Equals("all", StringComparison.OrdinalIgnoreCase))
                parameters.Add(DbParams.Category, model.Category, DbType.String);

            if (model.Inactive != null)
                parameters.Add(DbParams.InActive, model.Inactive, DbType.Int32);

            parameters.Add(DbParams.Page, model.Page.HasValue ? model.Page : DefaultAppSettings.PageSize, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit.HasValue ? model.Limit : DefaultAppSettings.PageLimit, DbType.Int32);

            return await _context.ExecuteQueryAsync<vwCustomerSearch>(procedureName, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<CustomerDynamicSearchWebResponseModel>> AllCustomers(CustomerDynamicSearchWebViewModel model)
        {
            var query = "SELECT CustomerID,CustomerName + ' - ' + CAST(CustomerID AS VARCHAR) AS FullName FROM vwCustomerSearch WHERE CustomerName LIKE @CustomerName";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.CustomerName, $"{model.CustomerName}%");

            return await _context.ExecuteQueryAsync<CustomerDynamicSearchWebResponseModel>(query, parameters, CommandType.Text);

        }

        public async Task<IEnumerable<vwCustomerSearch>> CustomerByClaimantSearch(ClaimantCustomerSearchWebViewModel model)
        {
            string query = @"
            SELECT Distinct cs.*, cl.FirstNameLastName AS ClaimantName
            FROM vwCustomerSearch cs
            INNER JOIN vwClaimantCustomerSearch cc ON cs.CustomerID = cc.CustomerID
            INNER JOIN vwClaimantSearch cl ON cc.ClaimantID = cl.ClaimantID
            WHERE cc.ClaimantID = @ClaimantID";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ClaimantID, model.ClaimantID, DbType.Int32);

            return await _context.ExecuteQueryAsync<vwCustomerSearch>(query, parameters, CommandType.Text);

        }

    }
}
