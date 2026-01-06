using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;

namespace CateringEcommerce.BAL.Base.Owner
{
    public class AvailabilityRepository : IAvailabilityRepository
    {
        private readonly SqlDatabaseManager _db;
        private static readonly HashSet<string> ValidStatuses = new() { "OPEN", "CLOSED", "FULLY_BOOKED" };

        public AvailabilityRepository(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }


        public async Task<GlobalAvailabilityModel> GetAvailabilityForPageAsync(long ownerId)
        {
            var globalStatus = await GetGlobalStatusAsync(ownerId);
            var dateOverrides = await GetCurrentMonthDatesAsync(ownerId);

            return new GlobalAvailabilityModel
            {
                GlobalStatus = globalStatus ?? "OPEN", // default behavior
                SpecialDates = dateOverrides
            };
        }

        public async Task<string?> GetGlobalStatusAsync(long ownerId)
        {
            try
            {
                string query = $@"
                SELECT c_global_status
                FROM {Table.SysCateringAvailabilityGlobal}
                WHERE c_ownerid = @OwnerId";

                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                };

                var result = await _db.ExecuteScalarAsync(query, sqlParameter);
                return result?.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<Dictionary<string, DateAvailabilityPayload>> GetCurrentMonthDatesAsync(long ownerId)
        {
            try
            {
                var result = new Dictionary<string, DateAvailabilityPayload>();

                string query = $@" 
                SELECT c_date, c_status, c_note
                FROM {Table.SysCateringAvailabilityDate}
                WHERE c_ownerid = @OwnerId
                  AND c_date >= DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
                  AND c_date <  DATEADD(MONTH, 1, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
                ORDER BY c_date";

                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                new SqlParameter("@OwnerId", ownerId),
                };

                using var reader = await _db.ExecuteReaderAsync(query, sqlParameter);

                while (await reader.ReadAsync())
                {
                    var date = ((DateTime)reader["c_date"]).ToString("yyyy-MM-dd");

                    result[date] = new DateAvailabilityPayload
                    {
                        Status = reader["c_status"].ToString()!,
                        Note = reader["c_note"] as string
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task UpsertGlobalAsync(long ownerId, string status)
        {
            try
            {
                if (!ValidStatuses.Contains(status))
                    throw new Exception("Invalid global status");

                var sql = $@"
                IF EXISTS (SELECT 1 FROM {Table.SysCateringAvailabilityGlobal} WHERE c_ownerid=@OwnerId)
                    UPDATE t_catering_availability_global
                    SET c_global_status=@Status,
                        c_modifieddate=GETDATE()
                    WHERE c_ownerid=@OwnerId
                ELSE
                    INSERT INTO {Table.SysCateringAvailabilityGlobal}
                    (c_ownerid, c_global_status, c_modifieddate)
                    VALUES (@OwnerId, @Status, GETDATE())";

                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@Status", status)
                };

                await _db.ExecuteNonQueryAsync(sql, sqlParameter);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // 🔹 DATE UPSERT
        public async Task UpsertDateAsync(long ownerId, DateTime date, string status, string? note)
        {

            if (!ValidStatuses.Contains(status))
                throw new Exception("Invalid global status");
            try
            {
                var sql = $@"
                IF EXISTS (
                    SELECT 1 FROM {Table.SysCateringAvailabilityDate}
                    WHERE c_ownerid=@OwnerId AND c_date=@Date
                )
                    UPDATE t_catering_availability_dates
                    SET c_status=@Status,
                        c_note=@Note,
                        c_modifieddate=GETDATE()
                    WHERE c_ownerid=@OwnerId AND c_date=@Date
                ELSE
                INSERT INTO {Table.SysCateringAvailabilityDate}
                (c_ownerid, c_date, c_status, c_note, c_createddate, c_modifieddate)
                VALUES (@OwnerId, @Date, @Status, @Note, GETDATE(), GETDATE())";

                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                new SqlParameter("@OwnerId", ownerId),
                new SqlParameter("@Status", status),
                new SqlParameter("@Date", date.Date.ToString("yyyy-MM-dd")),
                new SqlParameter("@Note", (object?)note ?? DBNull.Value)
                };

                await _db.ExecuteNonQueryAsync(sql, sqlParameter);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        #region Delete the Specific Date record
        public async Task DeleteDateAsync(long ownerId, DateTime date)
        {
            try
            {
                string query = $@"
                    DELETE FROM {Table.SysCateringAvailabilityDate}
                    WHERE c_ownerid=@OwnerId AND c_date=@Date";

                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@Date", date.Date)
                };

                await _db.ExecuteNonQueryAsync(query, sqlParameter);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

       

        //private static void Validate(GlobalAvailabilityModel request)
        //{
        //    if (!ValidStatuses.Contains(request.GlobalStatus))
        //        throw new Exception("Invalid global status");

        //    foreach (var item in request.SpecialDates)
        //    {
        //        if (!DateTime.TryParse(item.Key, out _))
        //            throw new Exception($"Invalid date format: {item.Key}");

        //        if (!ValidStatuses.Contains(item.Value.Status))
        //            throw new Exception($"Invalid status for date {item.Key}");
        //    }
        //}
    }

}
