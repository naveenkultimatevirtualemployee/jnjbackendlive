using Dapper;
using JNJServices.Business.Abstracts;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.DbConstants;
using System.Data;
using System.Text.Json;

namespace JNJServices.Business.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IDapperContext _context;
        public SettingsService(IDapperContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Settings>> SettingsAsync()
        {
            string query = @"
            SELECT 
                [SettingID],
                [SettingLabel],
                [SettingKey],
                [SettingValue],
                [InputType],
                [InputOptions],
                [DisplayOrder],
                [InactiveFlag],
                [CreatedAt],
                [UpdatedAt]
            FROM settings where InactiveFlag = 0 order by DisplayOrder";

            var result = await _context.ExecuteQueryAsync<Settings>(query, CommandType.Text);

            return result;
        }

        public async Task<(int responseCode, string message)> UpdateSettingsAsync(List<SettingWebViewModel> settings)
        {
            string procedureName = ProcEntities.spUpdateWebSettings;

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.Settings, JsonSerializer.Serialize(settings), DbType.String);
            parameters.Add(DbParams.ResponseCode, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

            await _context.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);

            int responseCode = parameters.Get<int>(DbParams.ResponseCode);
            string message = parameters.Get<string>(DbParams.Msg);

            return (responseCode, message);
        }

        public async Task<SettingValueResponseModel> GetSettingByKeyAsync(SettingKeyViewModel model)
        {
            string query = @"
			SELECT  [SettingValue] 
			FROM settings
			WHERE SettingKey = @SettingKey AND InactiveFlag = 0 
			ORDER BY DisplayOrder";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.SettingKey, model.SettingKey);

            return await _context.ExecuteQueryFirstOrDefaultAsync<SettingValueResponseModel?>(query, parameters, CommandType.Text) ?? new SettingValueResponseModel();

        }


    }
}
