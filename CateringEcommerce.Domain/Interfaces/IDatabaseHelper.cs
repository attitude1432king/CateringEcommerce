
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.Domain.Interfaces
{
    public interface IDatabaseHelper
    {
        string GetConnectionString();
        int ExecuteNonQuery(string query, SqlParameter[] parameters = null);
        object ExecuteScalar(string query, SqlParameter[] parameters = null);
        SqlDataReader ExecuteReader(string query, SqlParameter[] parameters = null);
        DataTable Execute(string query, SqlParameter[] parameters = null);
        Task<DataSet> ExecuteDataSet(string query, SqlParameter[] parameters = null);
        Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null);
        Task<object> ExecuteScalarAsync(string query, SqlParameter[] parameters = null);
        Task<SqlDataReader> ExecuteReaderAsync(string query, SqlParameter[] parameters = null);
        Task<DataTable> ExecuteAsync(string query, SqlParameter[] parameters = null);

        // New methods for stored procedures and query execution
        Task<T> ExecuteStoredProcedureAsync<T>(string storedProcedureName, SqlParameter[] parameters = null);
        Task<List<T>> ExecuteQueryAsync<T>(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text);
    }
}
