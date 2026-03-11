
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

        // Additional helper methods
        Task<T> ExecuteQueryFirstAsync<T>(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text) where T : class;
        Task<(List<T1>, List<T2>)> ExecuteStoredProcedureMultipleAsync<T1, T2>(string storedProcedureName, SqlParameter[] parameters = null);
        Task<DataSet> ExecuteStoredProcedureMultipleAsync(string storedProcedureName, SqlParameter[] parameters = null);
        Task<TResult> ExecuteScalarAsync<TResult>(string query, SqlParameter[] parameters = null);
        Task<TResult> ExecuteScalarAsync<TResult>(string query, SqlParameter[] parameters, CommandType commandType);
        Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters, CommandType commandType);

        // Transaction support
        Task<IDatabaseTransaction> BeginTransactionAsync();
        IDatabaseTransaction BeginTransaction();
    }

    /// <summary>
    /// Represents a database transaction
    /// Implements IDisposable for automatic rollback on dispose if not committed
    /// </summary>
    public interface IDatabaseTransaction : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Commits the transaction
        /// </summary>
        void Commit();

        /// <summary>
        /// Commits the transaction asynchronously
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        void Rollback();

        /// <summary>
        /// Rolls back the transaction asynchronously
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// Executes a non-query within this transaction
        /// </summary>
        Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text);

        /// <summary>
        /// Executes a scalar query within this transaction
        /// </summary>
        Task<TResult> ExecuteScalarAsync<TResult>(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text);

        /// <summary>
        /// Executes a query within this transaction
        /// </summary>
        Task<DataTable> ExecuteAsync(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text);
    }
}
