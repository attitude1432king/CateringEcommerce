using CateringEcommerce.Domain.Interfaces;
using System.Data;
using System.Data.Common;

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

        public abstract int ExecuteNonQuery(string query, DbParameter[] parameters = null);
        public abstract object ExecuteScalar(string query, DbParameter[] parameters = null);
        public abstract DbDataReader ExecuteReader(string query, DbParameter[] parameters = null);
        public abstract DataTable Execute(string query, DbParameter[] parameters = null);
        public abstract Task<DataSet> ExecuteDataSet(string query, DbParameter[] parameters = null);

        public abstract Task<int> ExecuteNonQueryAsync(string query, DbParameter[] parameters = null);
        public abstract Task<object> ExecuteScalarAsync(string query, DbParameter[] parameters = null);
        public abstract Task<DbDataReader> ExecuteReaderAsync(string query, DbParameter[] parameters = null);
        public abstract Task<DataTable> ExecuteAsync(string query, DbParameter[] parameters = null);

        public abstract Task<T> ExecuteStoredProcedureAsync<T>(string storedProcedureName, DbParameter[] parameters = null);
        public abstract Task<List<T>> ExecuteQueryAsync<T>(string query, DbParameter[] parameters = null, CommandType commandType = CommandType.Text);
        public abstract Task<T> ExecuteQueryFirstAsync<T>(string query, DbParameter[] parameters = null, CommandType commandType = CommandType.Text) where T : class;
        public abstract Task<(List<T1>, List<T2>)> ExecuteStoredProcedureMultipleAsync<T1, T2>(string storedProcedureName, DbParameter[] parameters = null);
        public abstract Task<DataSet> ExecuteStoredProcedureMultipleAsync(string storedProcedureName, DbParameter[] parameters = null);
        public abstract Task<TResult> ExecuteScalarAsync<TResult>(string query, DbParameter[] parameters = null);
        public abstract Task<TResult> ExecuteScalarAsync<TResult>(string query, DbParameter[] parameters, CommandType commandType);
        public abstract Task<int> ExecuteNonQueryAsync(string query, DbParameter[] parameters, CommandType commandType);

        public abstract Task<IDatabaseTransaction> BeginTransactionAsync();
        public abstract IDatabaseTransaction BeginTransaction();
    }
}
