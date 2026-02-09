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
    }
}
