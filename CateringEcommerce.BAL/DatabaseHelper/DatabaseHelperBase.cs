using CateringEcommerce.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
namespace CateringEcommerce.BAL.DatabaseHelper
{
    public abstract class DatabaseHelperBase : IDatabaseHelper
    {
        protected readonly string _connectionString;

        protected DatabaseHelperBase(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        public string GetConnectionString() => _connectionString;

        public abstract int ExecuteNonQuery(string query, SqlParameter[] parameters = null);
        public abstract object ExecuteScalar(string query, SqlParameter[] parameters = null);
        public abstract SqlDataReader ExecuteReader(string query, SqlParameter[] parameters = null);
        public abstract DataTable Execute(string query, SqlParameter[] parameters = null);
        public abstract Task<DataSet> ExecuteDataSet(string query, SqlParameter[] parameters = null);

        public abstract Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null);
        public abstract Task<object> ExecuteScalarAsync(string query, SqlParameter[] parameters = null);
        public abstract Task<SqlDataReader> ExecuteReaderAsync(string query, SqlParameter[] parameters = null);
        public abstract Task<DataTable> ExecuteAsync(string query, SqlParameter[] parameters = null);

        // New methods for stored procedures and query execution
        public abstract Task<T> ExecuteStoredProcedureAsync<T>(string storedProcedureName, SqlParameter[] parameters = null);
        public abstract Task<List<T>> ExecuteQueryAsync<T>(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text);
    }
}