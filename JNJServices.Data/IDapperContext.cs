using Dapper;
using System.Data;
using static Dapper.SqlMapper;

namespace JNJServices.Data
{
    public interface IDapperContext
    {
        IDbConnection CreateConnection();

        // Executes a non-query SQL command (e.g., INSERT, UPDATE, DELETE) asynchronously
        Task<int> ExecuteAsync(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.Text);

        // Executes a scalar SQL command and returns a single value asynchronously
        Task<string?> ExecuteScalerAsync(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.Text);

        // Executes a SQL query and returns the first result or default value asynchronously
        Task<T?> ExecuteQueryFirstOrDefaultAsync<T>(string procedureName, CommandType commandType = CommandType.StoredProcedure);

        // Executes a SQL query with parameters and returns the first result or default value asynchronously
        Task<T?> ExecuteQueryFirstOrDefaultAsync<T>(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure);

        // Executes a SQL query and returns all results asynchronously
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(string procedureName, CommandType commandType = CommandType.StoredProcedure);

        // Executes a SQL query with parameters and returns all results asynchronously
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure);

        // Executes a SQL query that returns multiple result sets asynchronously
        Task<GridReader> ExecuteQueryMultipleAsync(string procedureName, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure);
    }
}
