using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.DatabaseHelper
{
    public class SqlDatabaseManager : DatabaseHelperBase
    {
        private SqlConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Connection string not set.");

            return new SqlConnection(_connectionString);
        }

        // Synchronous methods (keep for backward compatibility)
        public override int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public override object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public override SqlDataReader ExecuteReader(string query, SqlParameter[] parameters = null)
        {
            var conn = GetConnection();
            var cmd = new SqlCommand(query, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public override DataTable Execute(string query, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        // ASYNC METHODS

        public override async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public override async Task<object> ExecuteScalarAsync(string query, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                await conn.OpenAsync();
                return await cmd.ExecuteScalarAsync();
            }
        }

        public override async Task<SqlDataReader> ExecuteReaderAsync(string query, SqlParameter[] parameters = null)
        {
            var conn = GetConnection();
            var cmd = new SqlCommand(query, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            await conn.OpenAsync();
            return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        public override async Task<DataTable> ExecuteAsync(string query, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var dt = new DataTable();
                    dt.Load(reader);  // Synchronous but no true async alternative
                    return dt;
                }
            }
        }
    }

}
