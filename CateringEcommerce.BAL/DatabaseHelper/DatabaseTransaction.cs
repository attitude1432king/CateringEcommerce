using Npgsql;
using System.Data;
using System.Data.Common;
using CateringEcommerce.Domain.Interfaces;

namespace CateringEcommerce.BAL.DatabaseHelper
{
    public class DatabaseTransaction : IDatabaseTransaction
    {
        private readonly NpgsqlConnection _connection;
        private readonly NpgsqlTransaction _transaction;
        private bool _committed;
        private bool _rolledBack;
        private bool _disposed;

        internal DatabaseTransaction(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public void Commit()
        {
            EnsureNotDisposed();
            EnsureNotCommitted();
            EnsureNotRolledBack();
            _transaction.Commit();
            _committed = true;
        }

        public async Task CommitAsync()
        {
            EnsureNotDisposed();
            EnsureNotCommitted();
            EnsureNotRolledBack();
            await _transaction.CommitAsync();
            _committed = true;
        }

        public void Rollback()
        {
            EnsureNotDisposed();
            EnsureNotCommitted();
            if (_rolledBack)
            {
                return;
            }

            _transaction.Rollback();
            _rolledBack = true;
        }

        public async Task RollbackAsync()
        {
            EnsureNotDisposed();
            EnsureNotCommitted();
            if (_rolledBack)
            {
                return;
            }

            await _transaction.RollbackAsync();
            _rolledBack = true;
        }

        public async Task<int> ExecuteNonQueryAsync(string query, DbParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            EnsureReady();
            await using var cmd = CreateCommand(query, parameters, commandType);
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<TResult> ExecuteScalarAsync<TResult>(string query, DbParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            EnsureReady();
            await using var cmd = CreateCommand(query, parameters, commandType);
            var result = await cmd.ExecuteScalarAsync();
            return NpgsqlDatabaseManager.ConvertScalar<TResult>(result);
        }

        public async Task<DataTable> ExecuteAsync(string query, DbParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            EnsureReady();
            await using var cmd = CreateCommand(query, parameters, commandType);
            await using var reader = await cmd.ExecuteReaderAsync();
            var dataTable = new DataTable();
            dataTable.Load(reader);
            return dataTable;
        }

        private NpgsqlCommand CreateCommand(string query, DbParameter[] parameters, CommandType commandType)
        {
            var cmd = new NpgsqlCommand(SqlQueryTranslator.Normalize(query, commandType, parameters), _connection, _transaction)
            {
                CommandType = SqlQueryTranslator.NormalizeCommandType(commandType),
                CommandTimeout = 60
            };

            SqlQueryTranslator.AddParameters(cmd, parameters);
            return cmd;
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (!_committed && !_rolledBack)
            {
                try
                {
                    _transaction.Rollback();
                }
                catch
                {
                }
            }

            _transaction.Dispose();
            _connection.Dispose();
            _disposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            if (!_committed && !_rolledBack)
            {
                try
                {
                    await _transaction.RollbackAsync();
                }
                catch
                {
                }
            }

            await _transaction.DisposeAsync();
            await _connection.DisposeAsync();
            _disposed = true;
        }

        private void EnsureReady()
        {
            EnsureNotDisposed();
            EnsureNotCommitted();
            EnsureNotRolledBack();
        }

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
