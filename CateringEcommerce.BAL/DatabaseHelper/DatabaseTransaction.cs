using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces;

namespace CateringEcommerce.BAL.DatabaseHelper
{
    /// <summary>
    /// Represents a database transaction
    /// Automatically rolls back on dispose if not explicitly committed
    /// </summary>
    public class DatabaseTransaction : IDatabaseTransaction
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;
        private bool _committed;
        private bool _rolledBack;
        private bool _disposed;

        internal DatabaseTransaction(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _committed = false;
            _rolledBack = false;
            _disposed = false;
        }

        /// <summary>
        /// Commits the transaction
        /// </summary>
        public void Commit()
        {
            EnsureNotDisposed();
            EnsureNotCommitted();
            EnsureNotRolledBack();

            _transaction.Commit();
            _committed = true;
        }

        /// <summary>
        /// Commits the transaction asynchronously
        /// </summary>
        public async Task CommitAsync()
        {
            EnsureNotDisposed();
            EnsureNotCommitted();
            EnsureNotRolledBack();

            await _transaction.CommitAsync();
            _committed = true;
        }

        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        public void Rollback()
        {
            EnsureNotDisposed();
            EnsureNotCommitted();

            if (!_rolledBack)
            {
                _transaction.Rollback();
                _rolledBack = true;
            }
        }

        /// <summary>
        /// Rolls back the transaction asynchronously
        /// </summary>
        public async Task RollbackAsync()
        {
            EnsureNotDisposed();
            EnsureNotCommitted();

            if (!_rolledBack)
            {
                await _transaction.RollbackAsync();
                _rolledBack = true;
            }
        }

        /// <summary>
        /// Executes a non-query within this transaction
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            EnsureNotDisposed();
            EnsureNotCommitted();
            EnsureNotRolledBack();

            using var cmd = CreateCommand(query, parameters, commandType);
            return await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Executes a scalar query within this transaction
        /// </summary>
        public async Task<TResult> ExecuteScalarAsync<TResult>(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            EnsureNotDisposed();
            EnsureNotCommitted();
            EnsureNotRolledBack();

            using var cmd = CreateCommand(query, parameters, commandType);
            var result = await cmd.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
            {
                return default(TResult);
            }

            return (TResult)Convert.ChangeType(result, typeof(TResult));
        }

        /// <summary>
        /// Executes a query within this transaction
        /// </summary>
        public async Task<DataTable> ExecuteAsync(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            EnsureNotDisposed();
            EnsureNotCommitted();
            EnsureNotRolledBack();

            using var cmd = CreateCommand(query, parameters, commandType);
            using var adapter = new SqlDataAdapter(cmd);

            var dataTable = new DataTable();
            await Task.Run(() => adapter.Fill(dataTable));
            return dataTable;
        }

        /// <summary>
        /// Creates a command within this transaction
        /// </summary>
        private SqlCommand CreateCommand(string query, SqlParameter[] parameters, CommandType commandType)
        {
            var cmd = new SqlCommand(query, _connection, _transaction)
            {
                CommandType = commandType,
                CommandTimeout = 60 // 60 seconds default
            };

            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }

            return cmd;
        }

        /// <summary>
        /// Disposes the transaction (synchronous)
        /// Rolls back if not committed
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            // Auto-rollback if not committed
            if (!_committed && !_rolledBack)
            {
                try
                {
                    _transaction.Rollback();
                }
                catch
                {
                    // Ignore rollback errors during dispose
                }
            }

            _transaction?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Disposes the transaction asynchronously
        /// Rolls back if not committed
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            // Auto-rollback if not committed
            if (!_committed && !_rolledBack)
            {
                try
                {
                    await _transaction.RollbackAsync();
                }
                catch
                {
                    // Ignore rollback errors during dispose
                }
            }

            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }

            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }

            _disposed = true;
        }

        // Validation helpers
        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DatabaseTransaction));
            }
        }

        private void EnsureNotCommitted()
        {
            if (_committed)
            {
                throw new InvalidOperationException("Transaction has already been committed.");
            }
        }

        private void EnsureNotRolledBack()
        {
            if (_rolledBack)
            {
                throw new InvalidOperationException("Transaction has already been rolled back.");
            }
        }
    }
}
