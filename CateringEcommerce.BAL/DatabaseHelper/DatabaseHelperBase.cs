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

        // Additional helper methods
        public abstract Task<T> ExecuteQueryFirstAsync<T>(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text) where T : class;
        public abstract Task<(List<T1>, List<T2>)> ExecuteStoredProcedureMultipleAsync<T1, T2>(string storedProcedureName, SqlParameter[] parameters = null);
        public abstract Task<DataSet> ExecuteStoredProcedureMultipleAsync(string storedProcedureName, SqlParameter[] parameters = null);
        public abstract Task<TResult> ExecuteScalarAsync<TResult>(string query, SqlParameter[] parameters = null);
        public abstract Task<TResult> ExecuteScalarAsync<TResult>(string query, SqlParameter[] parameters, CommandType commandType);
        public abstract Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters, CommandType commandType);

        // Transaction support
        public abstract Task<IDatabaseTransaction> BeginTransactionAsync();
        public abstract IDatabaseTransaction BeginTransaction();
    }
}