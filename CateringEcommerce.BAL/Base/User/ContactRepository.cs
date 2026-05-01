using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using Npgsql;

namespace CateringEcommerce.BAL.Base.User
{
    public class ContactRepository : IContactRepository
    {
        private readonly IDatabaseHelper _db;

        public ContactRepository(IDatabaseHelper db)
        {
            _db = db;
        }

        public bool SaveMessage(ContactMessageRequest request, string? ipAddress)
        {
            string query = $@"
                INSERT INTO t_sys_contact_messages
                    (c_name, c_email, c_message, c_status, c_ip_address, c_createddate)
                VALUES
                    (@Name, @Email, @Message, 'New', @IpAddress, NOW())";

            NpgsqlParameter[] parameters =
            {
                new NpgsqlParameter("@Name",      request.Name),
                new NpgsqlParameter("@Email",     request.Email),
                new NpgsqlParameter("@Message",   request.Message),
                new NpgsqlParameter("@IpAddress", (object?)ipAddress ?? DBNull.Value),
            };

            int rows = _db.ExecuteNonQuery(query, parameters);
            return rows > 0;
        }
    }
}

