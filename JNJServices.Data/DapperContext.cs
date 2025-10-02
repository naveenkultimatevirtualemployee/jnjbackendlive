using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using static Dapper.SqlMapper;

namespace JNJServices.Data
{
    public class DapperContext : IDapperContext
    {
        private readonly IConfiguration _configuration;
        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IDbConnection CreateConnection()
            => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        public async Task<int> ExecuteAsync(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.Text)
        {
            using (var connection = CreateConnection())
            {
                return await connection.ExecuteAsync(procedureName, parameters, commandType: commandType);
            }
        }

        public async Task<string?> ExecuteScalerAsync(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.Text)
        {
            using (var connection = CreateConnection())
            {
                var res = await connection.ExecuteScalarAsync(procedureName, parameters, commandType: commandType);
                if (res != null)
                {
                    return res.ToString();
                }

                return string.Empty;
            }
        }

        public async Task<T?> ExecuteQueryFirstOrDefaultAsync<T>(string procedureName, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = CreateConnection())
            {
                var response = await connection.QueryFirstOrDefaultAsync<T>(procedureName, commandType: commandType);
                return response;
            }
        }

        public async Task<T?> ExecuteQueryFirstOrDefaultAsync<T>(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = CreateConnection())
            {
                var response = await connection.QueryFirstOrDefaultAsync<T>(procedureName, parameters, commandType: commandType);
                return response;
            }
        }

        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string procedureName, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = CreateConnection())
            {
                var response = await connection.QueryAsync<T>(procedureName, commandType: commandType);
                return response.ToList();
            }
        }

        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = CreateConnection())
            {
                var response = await connection.QueryAsync<T>(procedureName, parameters, commandType: commandType);
                return response.ToList();
            }
        }

        public async Task<GridReader> ExecuteQueryMultipleAsync(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = CreateConnection())
            {
                var response = await connection.QueryMultipleAsync(procedureName, parameters, commandType: commandType);
                return response;
            }
        }
    }
}
