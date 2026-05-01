using System.Data;
using System.Data.Common;

namespace CateringEcommerce.Domain.Interfaces
{
    public interface IDatabaseHelper
    {
        string GetConnectionString();
        int ExecuteNonQuery(string query, DbParameter[] parameters = null);
        object ExecuteScalar(string query, DbParameter[] parameters = null);
        DbDataReader ExecuteReader(string query, DbParameter[] parameters = null);
        DataTable Execute(string query, DbParameter[] parameters = null);
        Task<DataSet> ExecuteDataSet(string query, DbParameter[] parameters = null);
        Task<int> ExecuteNonQueryAsync(string query, DbParameter[] parameters = null);
        Task<object> ExecuteScalarAsync(string query, DbParameter[] parameters = null);
        Task<DbDataReader> ExecuteReaderAsync(string query, DbParameter[] parameters = null);
        Task<DataTable> ExecuteAsync(string query, DbParameter[] parameters = null);

        Task<T> ExecuteStoredProcedureAsync<T>(string storedProcedureName, DbParameter[] parameters = null);
        Task<List<T>> ExecuteQueryAsync<T>(string query, DbParameter[] parameters = null, CommandType commandType = CommandType.Text);

        Task<T> ExecuteQueryFirstAsync<T>(string query, DbParameter[] parameters = null, CommandType commandType = CommandType.Text) where T : class;
        Task<(List<T1>, List<T2>)> ExecuteStoredProcedureMultipleAsync<T1, T2>(string storedProcedureName, DbParameter[] parameters = null);
        Task<DataSet> ExecuteStoredProcedureMultipleAsync(string storedProcedureName, DbParameter[] parameters = null);
        Task<TResult> ExecuteScalarAsync<TResult>(string query, DbParameter[] parameters = null);
        Task<TResult> ExecuteScalarAsync<TResult>(string query, DbParameter[] parameters, CommandType commandType);
        Task<int> ExecuteNonQueryAsync(string query, DbParameter[] parameters, CommandType commandType);

        Task<IDatabaseTransaction> BeginTransactionAsync();
        IDatabaseTransaction BeginTransaction();
    }

    public interface IDatabaseTransaction : IDisposable, IAsyncDisposable
    {
        void Commit();
        Task CommitAsync();
        void Rollback();
        Task RollbackAsync();
        Task<int> ExecuteNonQueryAsync(string query, DbParameter[] parameters = null, CommandType commandType = CommandType.Text);
        Task<TResult> ExecuteScalarAsync<TResult>(string query, DbParameter[] parameters = null, CommandType commandType = CommandType.Text);
        Task<DataTable> ExecuteAsync(string query, DbParameter[] parameters = null, CommandType commandType = CommandType.Text);
    }
}
