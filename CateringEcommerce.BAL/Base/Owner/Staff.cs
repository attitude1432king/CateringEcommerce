using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Npgsql;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Base.Owner
{
    public class Staff : IStaff
    {
        private readonly IDatabaseHelper _dbHelper;
        public Staff(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// Add new staff member to the database
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="staff"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> AddStaffAsync(long ownerPKID, StaffDto staff)
        {
            try
            {
                // âœ… Define insert query
                string insertQuery = $@"INSERT INTO {Table.SysCateringStaff}
                        (c_ownerid, c_fullname, c_contact_number, c_gender, c_role, c_other_role, c_expertise_categoryid, c_experience_years, 
                        c_salary_type, c_salary_amount, c_availability)
                        VALUES
                        (@OwnerPKID, @Name, @Contact, @Gender, @Role, @OtherRole, @CategoryId, @Experience, @SalaryType, @SalaryAmount, @Availability)
                    RETURNING c_staffid;"; // âœ… Return the new staff ID

                // âœ… Prepare SQL parameters
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@Name", staff.Name?.ToString()),
                    new NpgsqlParameter("@Contact", staff.Contact?.ToString()),
                    new NpgsqlParameter("@Gender", staff.Gender?.ToString()),
                    new NpgsqlParameter("@Role", staff.Role?.ToString()),
                    new NpgsqlParameter("@OtherRole", staff.OtherRole?.ToString()),
                    new NpgsqlParameter("@CategoryId", staff.CategoryId > 0 ? staff.CategoryId : DBNull.Value),
                    new NpgsqlParameter("@Experience", staff.Experience),
                    new NpgsqlParameter("@SalaryType", (object?)staff.salaryType ?? DBNull.Value),
                    new NpgsqlParameter("@SalaryAmount", staff.SalaryAmount),
                    new NpgsqlParameter("@Availability", staff.Availability.ToString())
                };

                // âœ… Execute insert query
                var result = await _dbHelper.ExecuteScalarAsync(insertQuery, parameters.ToArray());

                // âœ… Convert result to int (new record ID)
                int newStaffId = result != null ? Convert.ToInt32(result) : 0;
                return newStaffId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while adding staff: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update the staff member details
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="staff"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> UpdateStaffAsync(long ownerPKID, StaffDto staff)
        {
            try
            {
                // âœ… Security check: make sure the record belongs to this owner
                string validationQuery = $@"SELECT COUNT(1) FROM {Table.SysCateringStaff} 
                                    WHERE c_ownerid = @OwnerPKID AND c_staffid = @StaffID";

                List<NpgsqlParameter> validateParams = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@StaffID", staff.ID ?? 0)
                };

                var existsResult = await _dbHelper.ExecuteScalarAsync(validationQuery, validateParams.ToArray());
                int existsCount = existsResult != null ? Convert.ToInt32(existsResult) : 0;

                if (existsCount == 0)
                    throw new Exception("Invalid staff ID or unauthorized access.");

                // âœ… Update query (updates only editable fields)
                string updateQuery = $@"
                UPDATE {Table.SysCateringStaff}
                SET 
                    c_fullname = @Name,
                    c_contact_number = @Contact,
                    c_gender = @Gender,
                    c_role = @Role,                    
                    c_other_role = @OtherRole,
                    c_expertise_categoryid = @CategoryId,
                    c_experience_years = @Experience,
                    c_salary_type = @SalaryType,
                    c_salary_amount = @SalaryAmount,
                    c_availability = @Availability,
                    c_modifieddate = NOW()
                WHERE c_ownerid = @OwnerPKID AND c_staffid = @StaffID";

                // âœ… Prepare parameters
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@StaffID", staff.ID ?? 0),
                    new NpgsqlParameter("@Name", staff.Name ?? string.Empty),
                    new NpgsqlParameter("@Contact", staff.Contact ?? string.Empty),
                    new NpgsqlParameter("@Gender", staff.Gender ?? string.Empty),
                    new NpgsqlParameter("@Role", staff.Role ?? string.Empty),
                    new NpgsqlParameter("@OtherRole", staff.OtherRole?.ToString()),
                    new NpgsqlParameter("@CategoryId", staff.CategoryId > 0 ? staff.CategoryId : DBNull.Value),
                    new NpgsqlParameter("@Experience", staff.Experience),
                    new NpgsqlParameter("@SalaryType", (object?)staff.salaryType ?? DBNull.Value),
                    new NpgsqlParameter("@SalaryAmount", staff.SalaryAmount),
                    new NpgsqlParameter("@Availability", staff.Availability),
                };

                // âœ… Execute update
                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(updateQuery, parameters.ToArray());
                return rowsAffected;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating staff: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Soft delete staff member from the database
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="staffPKID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> SoftDeleteStaffAsync(long ownerPKID, long staffPKID)
        {
            try
            {
                string query = $@"
                    UPDATE {Table.SysCateringStaff} SET c_is_deleted = TRUE, c_availability = 0, c_modifieddate = NOW()
                    WHERE c_ownerid = @OwnerPKID AND c_staffid = @StaffPKID";

                List<NpgsqlParameter> deleteParams = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@StaffPKID", staffPKID)
                };

                int rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, deleteParams.ToArray());

                return rowsAffected; // 1 = deleted successfully, 0 = not deleted
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting staff: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get the list of staff members with pagination and filtering
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<StaffModel>> GetStaffListAsync(long ownerPKID, int page, int pageSize, string filterJson)
        {
            try
            {

                var filter = string.IsNullOrWhiteSpace(filterJson)
                    ? new StaffFilter()
                    : JsonConvert.DeserializeObject<StaffFilter>(filterJson) ?? new StaffFilter();

                int offset = (page - 1) * pageSize;

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@Offset", offset),
                    new NpgsqlParameter("@PageSize", pageSize)
                };

                StringBuilder sql = new();

                sql.Append($@"SELECT s.c_staffid, s.c_fullname, s.c_contact_number, s.c_gender, s.c_role, s.c_other_role,
                            fc.c_categoryname AS Expertise, s.c_expertise_categoryid, s.c_experience_years, s.c_salary_type, 
                            s.c_salary_amount, s.c_availability, s.c_profile_path, s.c_identity_doc_path, s.c_resume_doc_path
                            FROM {Table.SysCateringStaff} s
                            LEFT JOIN {Table.SysFoodCategory} fc ON s.c_expertise_categoryid = fc.c_categoryid");

                // Attach dynamic filters
                sql.Append(BuildStaffFilterQuery(filter, parameters));

                sql.Append(@"
                    ORDER BY s.c_staffid DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                ");

                var staffData = await _dbHelper.ExecuteAsync(sql.ToString(), parameters.ToArray());
                if (staffData.Rows.Count == 0)
                    return new List<StaffModel>();

                var staffList = new List<StaffModel>();

                foreach (System.Data.DataRow row in staffData.Rows)
                {
                    var staffId = row["c_staffid"] != DBNull.Value ? Convert.ToInt64(row["c_staffid"]) : 0;

                    // âœ… Helper to safely extract file info
                    StaffMediaModel[] BuildMedia(string? filePath)
                    {
                        if (string.IsNullOrWhiteSpace(filePath))
                            return null;

                        return new StaffMediaModel[]
                        {
                            new StaffMediaModel
                            {
                                Path = filePath,
                                Type = Path.GetExtension(filePath)
                            }
                        };
                    }

                    staffList.Add(new StaffModel
                    {
                        ID = staffId,
                        Name = row["c_fullname"]?.ToString(),
                        Contact = row["c_contact_number"]?.ToString(),
                        Gender = row["c_gender"]?.ToString(),
                        Role = row["c_role"]?.ToString(),
                        OtherRole = row["c_other_role"]?.ToString(),
                        CategoryId = row["c_expertise_categoryid"] != DBNull.Value ? Convert.ToInt32(row["c_expertise_categoryid"]) : 0,
                        Experience = row["c_experience_years"] != DBNull.Value ? Convert.ToInt32(row["c_experience_years"]) : 0,
                        Expertise = row["Expertise"]?.ToString(),
                        salaryType = row["c_salary_type"]?.ToString(),
                        SalaryAmount = row["c_salary_amount"] != DBNull.Value ? Convert.ToDecimal(row["c_salary_amount"]) : 0,
                        Availability = row["c_availability"] != DBNull.Value && Convert.ToBoolean(row["c_availability"]),

                        // âœ… Use safe method for media
                        Photo = BuildMedia(row["c_profile_path"]?.ToString()),
                        IdProof = BuildMedia(row["c_identity_doc_path"]?.ToString()),
                        Resume = BuildMedia(row["c_resume_doc_path"]?.ToString())
                    });
                }

                return staffList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while fetching staff list: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get staff count based on filters
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="filterJson"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> GetStaffCountAsync(long ownerPKID, string filterJson)
        {
            try
            {
                var filter = string.IsNullOrWhiteSpace(filterJson)
                ? new StaffFilter()
                : JsonConvert.DeserializeObject<StaffFilter>(filterJson) ?? new StaffFilter(); // Ensure filter is never null

                StringBuilder selectQuery = new StringBuilder();

                selectQuery.Append($@"
                    SELECT COUNT(1)
                    FROM {Table.SysCateringStaff} AS s"); 

                // âœ… Parameters
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID)
                };

                selectQuery.Append(BuildStaffFilterQuery(filter, parameters));
                // âœ… Execute and read result
                var result = await _dbHelper.ExecuteScalarAsync(selectQuery.ToString(), parameters.ToArray());
                int count = result != null ? Convert.ToInt32(result) : 0;

                return count; // returns 0 if no staff exist
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while getting staff count: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Staff contact number existence check
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="number"></param>
        /// <param name="excludeStaffPKID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> IsStaffNumberExistsAsync(long ownerPKID, string number, long? excludeStaffPKID = null)
        {
            try
            {
                // âœ… Base query
                string selectQuery = $@"
                    SELECT COUNT(1)
                    FROM {Table.SysCateringStaff}
                    WHERE c_ownerid = @OwnerPKID AND c_is_deleted = FALSE
                      AND LTRIM(RTRIM(c_contact_number)) = LTRIM(RTRIM(@ContactNumber))";

                // âœ… Exclude current record if updating
                if (excludeStaffPKID.HasValue && excludeStaffPKID.Value > 0)
                {
                    selectQuery += " AND c_staffid <> @ExcludeStaffPKID";
                }

                // âœ… Prepare parameters
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@ContactNumber", number)
                };

                if (excludeStaffPKID.HasValue && excludeStaffPKID.Value > 0)
                    parameters.Add(new NpgsqlParameter("@ExcludeStaffPKID", excludeStaffPKID.Value));

                // âœ… Execute and get result
                var result = await _dbHelper.ExecuteScalarAsync(selectQuery, parameters.ToArray());
                int count = result != null ? Convert.ToInt32(result) : 0;

                return count > 0; // true = number already exists
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while checking staff number: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update staff document paths
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="staffPKID"></param>
        /// <param name="dicPath"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Task<int> UpdateStaffDocumentPath(long ownerPKID, long? staffPKID, Dictionary<string, string> dicPath)
        {
            try
            {
                // âœ… Build dynamic SET clause based on provided paths
                List<string> setClauses = new();
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@StaffPKID", staffPKID)
                };
                foreach (var kvp in dicPath)
                {
                    string columnName = kvp.Key switch
                    {
                        "ProfilePath" => "c_profile_path",
                        "IdentityDocumentPath" => "c_identity_doc_path",
                        "ResumeDocumentPath" => "c_resume_doc_path",
                        _ => throw new Exception($"Invalid document type: {kvp.Key}")
                    };
                    setClauses.Add($"{columnName} = @{columnName}");
                    parameters.Add(new NpgsqlParameter($"@{columnName}", kvp.Value));
                }
                if (setClauses.Count == 0)
                    throw new Exception("No valid document paths provided for update.");
                string setClause = string.Join(", ", setClauses);
                // âœ… Construct update query
                string updateQuery = $@"
                    UPDATE {Table.SysCateringStaff}
                    SET {setClause},
                        c_modifieddate = NOW()
                    WHERE c_ownerid = @OwnerPKID AND c_staffid = @StaffPKID";
                // âœ… Execute update
                return _dbHelper.ExecuteNonQueryAsync(updateQuery, parameters.ToArray());

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Clear staff file path if it matches the provided file path
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="staffId"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> TryClearStaffFilePathAsync(long ownerPKID, long? staffId, string filePath)
        {
            try
            {
                // âœ… Step 1: Select file paths
                string selectQuery = $@"
                    SELECT c_profile_path, c_identity_doc_path, c_resume_doc_path
                    FROM {Table.SysCateringStaff}
                    WHERE c_staffid = @StaffID AND c_ownerid = @OwnerPKID";

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@StaffID", staffId),
                };

                var staffData = await _dbHelper.ExecuteAsync(selectQuery, parameters.ToArray());
                if (staffData.Rows.Count == 0)
                    return false;

                // âœ… Step 2: Loop through rows and find matching path
                foreach (System.Data.DataRow row in staffData.Rows)
                {
                    string? profilePath = row["c_profile_path"]?.ToString();
                    string? identityPath = row["c_identity_doc_path"]?.ToString();
                    string? resumePath = row["c_resume_doc_path"]?.ToString();

                    string? columnToUpdate = null;

                    if (!string.IsNullOrEmpty(profilePath) && string.Equals(profilePath, filePath, StringComparison.OrdinalIgnoreCase))
                        columnToUpdate = "c_profile_path";
                    else if (!string.IsNullOrEmpty(identityPath) && string.Equals(identityPath, filePath, StringComparison.OrdinalIgnoreCase))
                        columnToUpdate = "c_identity_doc_path";
                    else if (!string.IsNullOrEmpty(resumePath) && string.Equals(resumePath, filePath, StringComparison.OrdinalIgnoreCase))
                        columnToUpdate = "c_resume_doc_path";

                    // âœ… Step 3: If found, set the column to NULL
                    if (columnToUpdate != null)
                    {
                        string updateQuery = $@"
                            UPDATE {Table.SysCateringStaff}
                            SET {columnToUpdate} = NULL
                            WHERE c_staffid = @StaffID AND c_ownerid = @OwnerPKID";

                        List<NpgsqlParameter> updateParams = new()
                        {
                            new NpgsqlParameter("@StaffID", staffId),
                            new NpgsqlParameter("@OwnerPKID", ownerPKID)
                        };

                        int result = await _dbHelper.ExecuteNonQueryAsync(updateQuery, updateParams.ToArray());
                        return result > 0; // true â†’ can safely delete the physical file
                    }
                }

                // âœ… Step 4: No matching file found
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while verifying and clearing staff file path: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get All staff uploaded file paths
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<string>> GetAllStaffFilePathsAsync(long ownerPKID, long? staffId)
        {
            try
            {
                string selectQuery = $@"
                    SELECT c_profile_path, c_identity_doc_path, c_resume_doc_path
                    FROM {Table.SysCateringStaff}
                    WHERE c_staffid = @StaffID AND c_ownerid = @OwnerPKID";

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@StaffID", staffId),
                };

                var staffData = await _dbHelper.ExecuteAsync(selectQuery, parameters.ToArray());

                // List to collect all non-null paths
                List<string> filePaths = new();

                if (staffData.Rows.Count == 0)
                    return filePaths;

                foreach (DataRow row in staffData.Rows)
                {
                    string? profile = row["c_profile_path"]?.ToString();
                    string? identity = row["c_identity_doc_path"]?.ToString();
                    string? resume = row["c_resume_doc_path"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(profile))
                        filePaths.Add(profile);

                    if (!string.IsNullOrWhiteSpace(identity))
                        filePaths.Add(identity);

                    if (!string.IsNullOrWhiteSpace(resume))
                        filePaths.Add(resume);
                }

                return filePaths;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while fetching staff file paths: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="staffPKID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> IsValidStaffId(long ownerPKID, long staffPKID)
        {
            try
            {
                // âœ… Step 1: Validate ownership (security check)
                string validationQuery = $@"
                SELECT COUNT(1)
                FROM {Table.SysCateringStaff}
                WHERE c_ownerid = @OwnerPKID AND c_is_deleted = FALSE AND c_staffid = @StaffPKID";

                List<NpgsqlParameter> validationParams = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@StaffPKID", staffPKID)
                };

                var existsResult = await _dbHelper.ExecuteScalarAsync(validationQuery, validationParams.ToArray());
                int existsCount = existsResult != null ? Convert.ToInt32(existsResult) : 0;

                if (existsCount == 0)
                {
                    // âŒ Record not found or doesnâ€™t belong to owner
                    throw new Exception("Invalid Staff ID or unauthorized access.");
                }
                return true;
            }
            catch (Exception ex)
            {
                // âŒ Record not found or doesnâ€™t belong to owner
                throw new Exception("Invalid Staff ID or unauthorized access.");
            }
        }

        /// <summary>
        /// Update the availability status of a staff member
        /// </summary>
        /// <param name="ownerPKID"></param>
        /// <param name="staffPKID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdateStaffStatus(long ownerPKID, long staffPKID, bool status)
        {
            try
            {
                string updateQuery = $@"UPDATE {Table.SysCateringStaff} SET c_availability = @Status, c_modifieddate = NOW()
                                   WHERE c_ownerid = @OwnerPKID
                                    AND c_staffid = @StaffId";

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@OwnerPKID", ownerPKID),
                    new NpgsqlParameter("@StaffId", staffPKID),
                    new NpgsqlParameter("@Status", status.ToString())
                };

                var result = await _dbHelper.ExecuteNonQueryAsync(updateQuery, parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Build dynamic WHERE clause based on provided filters
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string BuildStaffFilterQuery(StaffFilter filter, List<NpgsqlParameter> parameters)
        {
            StringBuilder where = new();
            where.Append(" WHERE s.c_ownerid = @OwnerPKID AND c_is_deleted = FALSE");

            // Name search
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                where.Append(" AND LOWER(s.c_fullname) LIKE LOWER('%' || @Name || '%') ");
                parameters.Add(new NpgsqlParameter("@Name", filter.Name));
            }

            // Theme filter
            if (!string.IsNullOrWhiteSpace(filter.Role))
            {
                where.Append($" AND (LOWER(s.c_role) LIKE LOWER('%' || @Role || '%') OR LOWER(s.c_other_role) LIKE LOWER('%' || @Role || '%'))");
                parameters.Add(new NpgsqlParameter("@Role", filter.Role));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                where.Append(" AND s.c_availability = @Status ");
                parameters.Add(new NpgsqlParameter("@Status", filter.Status));
            }


            return where.ToString();
        }

    }
}
