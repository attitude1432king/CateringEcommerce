using Npgsql;
using System.Data;
using System.Data.Common;
using System.Reflection;
using CateringEcommerce.Domain.Interfaces;

namespace CateringEcommerce.BAL.DatabaseHelper
{
    public sealed class NpgsqlDatabaseManager : DatabaseHelperBase
    {
        private const int DefaultTimeout = 60;

        public NpgsqlDatabaseManager(IConfiguration configuration)
            : base(configuration)
        {
        }

        private NpgsqlConnection CreateConnection() => new(_connectionString);

        public override int ExecuteNonQuery(string query, DbParameter[] parameters = null)
        {
            using var conn = CreateConnection();
            using var cmd = CreateCommand(conn, query, parameters);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public override object ExecuteScalar(string query, DbParameter[] parameters = null)
        {
            using var conn = CreateConnection();
            using var cmd = CreateCommand(conn, query, parameters);
            conn.Open();
            return cmd.ExecuteScalar();
        }

        public override DbDataReader ExecuteReader(string query, DbParameter[] parameters = null)
        {
            var conn = CreateConnection();
            var cmd = CreateCommand(conn, query, parameters);
            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public override DataTable Execute(string query, DbParameter[] parameters = null)
        {
            using var conn = CreateConnection();
            using var cmd = CreateCommand(conn, query, parameters);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public override async Task<int> ExecuteNonQueryAsync(string query, DbParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public override async Task<object> ExecuteScalarAsync(string query, DbParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters);
            await conn.OpenAsync();
            return await cmd.ExecuteScalarAsync();
        }

        public override async Task<DbDataReader> ExecuteReaderAsync(string query, DbParameter[] parameters = null)
        {
            var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters);
            await conn.OpenAsync();
            return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        public override async Task<DataTable> ExecuteAsync(string query, DbParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public override async Task<DataSet> ExecuteDataSet(string query, DbParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            return await LoadDataSetAsync(reader);
        }

        public override async Task<T> ExecuteStoredProcedureAsync<T>(string procedureName, DbParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, procedureName, parameters, CommandType.StoredProcedure);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            return dt.Rows.Count > 0 ? Map<T>(dt.Rows[0]) : default;
        }

        public override async Task<List<T>> ExecuteQueryAsync<T>(string query, DbParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters, commandType);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            return dt.AsEnumerable().Select(Map<T>).ToList();
        }

        public override async Task<T> ExecuteQueryFirstAsync<T>(string query, DbParameter[] parameters = null, CommandType commandType = CommandType.Text) where T : class
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters, commandType);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            return dt.Rows.Count > 0 ? Map<T>(dt.Rows[0]) : default;
        }

        public override async Task<(List<T1>, List<T2>)> ExecuteStoredProcedureMultipleAsync<T1, T2>(string procedureName, DbParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, procedureName, parameters, CommandType.StoredProcedure);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            var ds = await LoadDataSetAsync(reader);

            var list1 = ds.Tables.Count > 0 ? ds.Tables[0].AsEnumerable().Select(Map<T1>).ToList() : new List<T1>();
            var list2 = ds.Tables.Count > 1 ? ds.Tables[1].AsEnumerable().Select(Map<T2>).ToList() : new List<T2>();
            return (list1, list2);
        }

        public override async Task<DataSet> ExecuteStoredProcedureMultipleAsync(string procedureName, DbParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, procedureName, parameters, CommandType.StoredProcedure);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            return await LoadDataSetAsync(reader);
        }

        public override async Task<TResult> ExecuteScalarAsync<TResult>(string query, DbParameter[] parameters = null)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters);
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return ConvertScalar<TResult>(result);
        }

        public override async Task<TResult> ExecuteScalarAsync<TResult>(string query, DbParameter[] parameters, CommandType commandType)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters, commandType);
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return ConvertScalar<TResult>(result);
        }

        public override async Task<int> ExecuteNonQueryAsync(string query, DbParameter[] parameters, CommandType commandType)
        {
            await using var conn = CreateConnection();
            await using var cmd = CreateCommand(conn, query, parameters, commandType);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public override IDatabaseTransaction BeginTransaction()
        {
            var connection = CreateConnection();
            connection.Open();
            return new DatabaseTransaction(connection, connection.BeginTransaction());
        }

        public override async Task<IDatabaseTransaction> BeginTransactionAsync()
        {
            var connection = CreateConnection();
            await connection.OpenAsync();
            var transaction = await connection.BeginTransactionAsync();
            return new DatabaseTransaction(connection, transaction);
        }

        public static TResult ConvertScalar<TResult>(object result)
        {
            if (result == null || result == DBNull.Value)
            {
                return default;
            }

            var targetType = Nullable.GetUnderlyingType(typeof(TResult)) ?? typeof(TResult);
            if (targetType == typeof(Guid))
            {
                return (TResult)(object)Guid.Parse(result.ToString()!);
            }

            if (targetType.IsEnum)
            {
                return (TResult)Enum.ToObject(targetType, result);
            }

            return (TResult)Convert.ChangeType(result, targetType);
        }

        private NpgsqlCommand CreateCommand(NpgsqlConnection conn, string query, DbParameter[] parameters, CommandType commandType = CommandType.Text)
        {
            var cmd = new NpgsqlCommand(SqlQueryTranslator.Normalize(query, commandType, parameters), conn)
            {
                CommandType = SqlQueryTranslator.NormalizeCommandType(commandType),
                CommandTimeout = DefaultTimeout
            };

            SqlQueryTranslator.AddParameters(cmd, parameters);
            return cmd;
        }

        private static async Task<DataSet> LoadDataSetAsync(DbDataReader reader)
        {
            var ds = new DataSet();
            do
            {
                var table = new DataTable();
                table.Load(reader);
                ds.Tables.Add(table);
            }
            while (await reader.NextResultAsync());

            return ds;
        }

        private static T Map<T>(DataRow row)
        {
            var obj = Activator.CreateInstance<T>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!row.Table.Columns.Contains(prop.Name) || row[prop.Name] == DBNull.Value)
                {
                    continue;
                }

                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                object value;

                if (targetType == typeof(Guid))
                {
                    value = Guid.Parse(row[prop.Name].ToString()!);
                }
                else if (targetType.IsEnum)
                {
                    value = Enum.ToObject(targetType, row[prop.Name]);
                }
                else
                {
                    value = Convert.ChangeType(row[prop.Name], targetType);
                }

                prop.SetValue(obj, value);
            }

            return obj;
        }
    }
}
