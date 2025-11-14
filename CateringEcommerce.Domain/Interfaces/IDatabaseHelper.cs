
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.Domain.Interfaces
{
    public interface IDatabaseHelper
    {
        void SetConnectionString(string connectionString);
        int ExecuteNonQuery(string query, SqlParameter[] parameters = null);
        object ExecuteScalar(string query, SqlParameter[] parameters = null);
        SqlDataReader ExecuteReader(string query, SqlParameter[] parameters = null);
        DataTable Execute(string query, SqlParameter[] parameters = null);
        Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null);
        Task<object> ExecuteScalarAsync(string query, SqlParameter[] parameters = null);
        Task<SqlDataReader> ExecuteReaderAsync(string query, SqlParameter[] parameters = null);
        Task<DataTable> ExecuteAsync(string query, SqlParameter[] parameters = null);
    }
}
