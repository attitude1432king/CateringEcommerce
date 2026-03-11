using CateringEcommerce.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace CateringEcommerce.BAL.DatabaseHelper
{
    public sealed class SqlDatabaseManager : DatabaseHelperBase
    {
        public SqlDatabaseManager(IConfiguration configuration)
        : base(configuration)
        {
        }

        private const int DefaultTimeout = 60;

        private SqlConnection CreateConnection()
            => new SqlConnection(_connectionString);

        // Synchronous methods (keep for backward compatibility)
        #region SYNC METHODS
        public override int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using var conn = CreateConnection();
            using var cmd = CreateCommand(conn, query, parameters);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public override object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using var conn = CreateConnection();
            using var cmd = CreateCommand(conn, query, parameters);
            conn.Open();
            return cmd.ExecuteScalar();
        }

        public override SqlDataReader ExecuteReader(string query, SqlParameter[] parameters = null)
        {
            using var conn = CreateConnection();
            using var cmd = CreateCommand(conn, query, parameters);
            conn.Open();
            return cmd.ExecuteReader();
        }

        public override DataTable Execute(string query, SqlParameter[] parameters = null)
        {
            using var conn = CreateConnection();
            using var cmd = CreateCommand(conn, query, parameters);
            using var adapter = new SqlDataAdapter(cmd);

            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }
        #endregion

        // ASYNC METHODS
        #region ASYNC METHODS
        public override async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public override async Task<object> ExecuteScalarAsync(string query, SqlParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters);
            await conn.OpenAsync();
            return await cmd.ExecuteScalarAsync();
        }

        public override async Task<SqlDataReader> ExecuteReaderAsync(string query, SqlParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters);
            await conn.OpenAsync();
            return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        public override async Task<DataTable> ExecuteAsync(string query, SqlParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters);
            await conn.OpenAsync();

            await using var reader = await cmd.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public override async Task<DataSet> ExecuteDataSet(string query, SqlParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters);

            await conn.OpenAsync();

            return await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(cmd);
                var ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            });
        }
        #endregion

        // Execute Stored Procedure
        #region STORED PROCEDURE

        public override async Task<T> ExecuteStoredProcedureAsync<T>(string procedureName, SqlParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, procedureName, parameters, CommandType.StoredProcedure);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            var dt = new DataTable();
            dt.Load(reader);

            return dt.Rows.Count > 0 ? Map<T>(dt.Rows[0]) : default;
        }
        #endregion

        #region GENERIC QUERY

        public override async Task<List<T>> ExecuteQueryAsync<T>(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters, commandType);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            var dt = new DataTable();
            dt.Load(reader);

            return dt.AsEnumerable().Select(Map<T>).ToList();
        }

        #endregion

        #region ADDITIONAL HELPER METHODS

        public override async Task<T> ExecuteQueryFirstAsync<T>(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text) where T : class
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters, commandType);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            var dt = new DataTable();
            dt.Load(reader);

            return dt.Rows.Count > 0 ? Map<T>(dt.Rows[0]) : default;
        }

        public override async Task<(List<T1>, List<T2>)> ExecuteStoredProcedureMultipleAsync<T1, T2>(string procedureName, SqlParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, procedureName, parameters, CommandType.StoredProcedure);

            await conn.OpenAsync();

            var ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            });

            var list1 = new List<T1>();
            var list2 = new List<T2>();

            if (ds.Tables.Count > 0)
                list1 = ds.Tables[0].AsEnumerable().Select(Map<T1>).ToList();

            if (ds.Tables.Count > 1)
                list2 = ds.Tables[1].AsEnumerable().Select(Map<T2>).ToList();

            return (list1, list2);
        }

        public override async Task<DataSet> ExecuteStoredProcedureMultipleAsync(string procedureName, SqlParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, procedureName, parameters, CommandType.StoredProcedure);

            await conn.OpenAsync();

            var ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            });

            return ds;
        }

        public override async Task<TResult> ExecuteScalarAsync<TResult>(string query, SqlParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters);
            await conn.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
                return default;

            var targetType = Nullable.GetUnderlyingType(typeof(TResult)) ?? typeof(TResult);
            return (TResult)Convert.ChangeType(result, targetType);
        }

        public override async Task<TResult> ExecuteScalarAsync<TResult>(string query, SqlParameter[] parameters, CommandType commandType)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters, commandType);
            await conn.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
                return default;

            var targetType = Nullable.GetUnderlyingType(typeof(TResult)) ?? typeof(TResult);
            return (TResult)Convert.ChangeType(result, targetType);
        }

        public override async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters, CommandType commandType)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters, commandType);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        #endregion

        #region HELPERS

        private SqlCommand CreateCommand(
            SqlConnection conn,
            string query,
            SqlParameter[] parameters,
            CommandType commandType = CommandType.Text)
        {
            var cmd = new SqlCommand(query, conn)
            {
                CommandType = commandType,
                CommandTimeout = DefaultTimeout
            };

            if (parameters?.Length > 0)
                cmd.Parameters.AddRange(parameters);

            return cmd;
        }

        private static T Map<T>(DataRow row)
        {
            var obj = Activator.CreateInstance<T>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!row.Table.Columns.Contains(prop.Name) || row[prop.Name] == DBNull.Value)
                    continue;

                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                var value = Convert.ChangeType(row[prop.Name], targetType);
                prop.SetValue(obj, value);
            }

            return obj;
        }

        #endregion

        #region TRANSACTION SUPPORT

        /// <summary>
        /// Begins a database transaction synchronously
        /// </summary>
        /// <returns>A transaction object that must be committed or rolled back</returns>
        public override IDatabaseTransaction BeginTransaction()
        {
            var connection = CreateConnection();
            connection.Open();
            var transaction = connection.BeginTransaction();
            return new DatabaseTransaction(connection, transaction);
        }

        /// <summary>
        /// Begins a database transaction asynchronously
        /// </summary>
        /// <returns>A transaction object that must be committed or rolled back</returns>
        public override async Task<IDatabaseTransaction> BeginTransactionAsync()
        {
            var connection = CreateConnection();
            await connection.OpenAsync();
            var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            return new DatabaseTransaction(connection, transaction);
        }

        #endregion
    }
}
