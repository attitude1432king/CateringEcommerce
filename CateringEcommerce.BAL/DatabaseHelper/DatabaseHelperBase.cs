using CateringEcommerce.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
namespace CateringEcommerce.BAL.DatabaseHelper
{
    public abstract class DatabaseHelperBase : IDatabaseHelper
    {
        protected string _connectionString;

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public abstract int ExecuteNonQuery(string query, SqlParameter[] parameters = null);
        public abstract object ExecuteScalar(string query, SqlParameter[] parameters = null);
        public abstract SqlDataReader ExecuteReader(string query, SqlParameter[] parameters = null);
        public abstract DataTable Execute(string query, SqlParameter[] parameters = null);
    }
}