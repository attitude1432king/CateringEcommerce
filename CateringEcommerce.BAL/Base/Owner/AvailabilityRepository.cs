using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;

namespace CateringEcommerce.BAL.Base.Owner
{
    public class AvailabilityRepository : IAvailabilityRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public AvailabilityRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// Asynchronously retrieves the global availability status and any special date overrides for a specified owner
        /// and month.
        /// </summary>
        /// <param name="ownerId">The unique identifier of the owner for whom to retrieve availability information.</param>
        /// <param name="year">The year component of the month for which to retrieve availability.</param>
        /// <param name="month">The month (1 through 12) for which to retrieve availability.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a GlobalAvailabilityModel with
        /// the global status and any special date overrides for the specified month.</returns>
        public async Task<GlobalAvailabilityModel> GetAvailabilityForPageAsync(long ownerId, int year, int month)
        {
            var globalStatus = await GetGlobalStatusAsync(ownerId);
            var dateOverrides = await GetCurrentMonthDatesAsync(ownerId, year, month);

            return new GlobalAvailabilityModel
            {
                GlobalStatus = globalStatus, // default behavior
                SpecialDates = dateOverrides
            };
        }

        /// <summary>
        /// Asynchronously retrieves the global catering status for the specified owner.
        /// </summary>
        /// <param name="ownerId">The unique identifier of the owner whose global catering status is to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the global status value. Returns
        /// 1 if no status is found for the specified owner.</returns>
        /// <exception cref="Exception">Thrown when an error occurs while retrieving the global status from the database.</exception>
        public async Task<int> GetGlobalStatusAsync(long ownerId)
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

                var result = await _dbHelper.ExecuteScalarAsync(query, sqlParameter);
                int status = result == DBNull.Value ? 1 : Convert.ToInt16(result);
                return status;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Asynchronously retrieves the availability status and notes for each date in the specified month for the
        /// given owner.
        /// </summary>
        /// <param name="ownerId">The unique identifier of the owner whose date availability is being queried.</param>
        /// <param name="year">The year component of the month for which to retrieve date availability.</param>
        /// <param name="month">The month component (1–12) for which to retrieve date availability.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary mapping each date
        /// in the specified month (formatted as "yyyy-MM-dd") to its corresponding availability payload. The dictionary
        /// will be empty if no availability data exists for the given criteria.</returns>
        /// <exception cref="Exception">Thrown if an error occurs while retrieving the date availability data.</exception>
        public async Task<Dictionary<string, DateAvailabilityPayload>> GetCurrentMonthDatesAsync(long ownerId, int year, int month)
        {
            try
            {
                var result = new Dictionary<string, DateAvailabilityPayload>();
                string query = $@" 
                SELECT c_date, c_status, c_note
                FROM {Table.SysCateringAvailabilityDate}
                WHERE c_ownerid = @OwnerId
                  AND c_date >= DATEFROMPARTS(@Year, @Month, 1)
                  AND c_date <  DATEADD(MONTH, 1, DATEFROMPARTS(@Year, @Month, 1))
                ORDER BY c_date";

                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@OwnerId", ownerId),
                    new SqlParameter("@Year", year),
                    new SqlParameter("@Month", month)
                };

                var dataTable = await _dbHelper.ExecuteAsync(query, sqlParameter);

                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    var date = ((DateTime)row["c_date"]).ToString("yyyy-MM-dd");
                    var statusValue = row["c_status"] == DBNull.Value ? 0 : Convert.ToInt32(row["c_status"]);

                    result[date] = new DateAvailabilityPayload
                    {
                        Status = Enum.IsDefined(typeof(AvailabilityStatus), statusValue)
                            ? (AvailabilityStatus)statusValue
                            : throw new Exception($"Invalid status value: {row["c_status"]}"),
                        Note = row["c_note"] == DBNull.Value ? null : row["c_note"].ToString()
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Creates or updates the global catering availability status for the specified owner asynchronously.
        /// </summary>
        /// <param name="ownerId">The unique identifier of the owner whose global catering availability status will be upserted.</param>
        /// <param name="status">The global catering availability status to set. Specify <see langword="true"/> to mark as available;
        /// otherwise, <see langword="false"/>.</param>
        /// <returns>A task that represents the asynchronous upsert operation.</returns>
        /// <exception cref="Exception">Thrown when an error occurs while accessing the database or executing the upsert operation.</exception>
        public async Task UpsertGlobalAsync(long ownerId, int status)
        {
            try
            {
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

                await _dbHelper.ExecuteNonQueryAsync(sql, sqlParameter);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Inserts a new availability date or updates an existing one for the specified owner, setting the status and
        /// optional note asynchronously.
        /// </summary>
        /// <param name="ownerId">The unique identifier of the owner for whom the availability date is being upserted.</param>
        /// <param name="date">The date to be inserted or updated for the owner's availability. Only the date component is used; time is
        /// ignored.</param>
        /// <param name="status">The status value to assign to the availability date. Must be a valid value defined in the AvailabilityStatus
        /// enumeration.</param>
        /// <param name="note">An optional note associated with the availability date. Can be null to indicate no note.</param>
        /// <returns>A task that represents the asynchronous upsert operation.</returns>
        /// <exception cref="Exception">Thrown if the specified status value is not defined in the AvailabilityStatus enumeration, or if a database
        /// error occurs during the operation.</exception>
        public async Task UpsertDateAsync(long ownerId, DateTime date, int status, string? note)
        {
            //check status value is not exclude to AvailablityStatus
            if (!Enum.IsDefined(typeof(AvailabilityStatus), status))
                throw new Exception($"Invalid status value: {status}. Status must be one of the defined AvailabilityStatus enum values.");
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

                await _dbHelper.ExecuteNonQueryAsync(sql, sqlParameter);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
