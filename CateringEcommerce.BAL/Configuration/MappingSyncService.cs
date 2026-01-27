using CateringEcommerce.BAL.DatabaseHelper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace CateringEcommerce.BAL.Configuration
{
    public class MappingSyncService
    {
        private readonly SqlDatabaseManager _db;

        public MappingSyncService(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        /// <summary>
        /// Synchronizes any mapping table using soft-delete strategy.
        /// - Inserts new mappings
        /// - Reactivates existing inactive mappings
        /// - Soft deletes removed mappings
        /// - Leaves unchanged mappings untouched
        /// </summary>
        public async Task SyncAsync(string tableName, string parentColumnName, string childColumnName, long parentPKID, List<long> incomingChildPKIDs)
        {
            if (incomingChildPKIDs == null)
                incomingChildPKIDs = new List<long>();

            if(incomingChildPKIDs.Count == 0)
                return;

            // -----------------------------------
            // STEP 1: Fetch existing mappings
            // -----------------------------------
            string selectQuery = $@"
            SELECT {childColumnName}, c_isactive
            FROM {tableName}
            WHERE {parentColumnName} = @ParentID";

            var existingTable = await _db.ExecuteAsync(
                selectQuery,
                new[] { new SqlParameter("@ParentID", parentPKID) }
            );

            // childId -> isActive
            var existingMap = existingTable.Rows
                .Cast<DataRow>()
                .ToDictionary(
                    r => Convert.ToInt64(r[childColumnName]),
                    r => Convert.ToBoolean(r["c_isactive"])
                );

            // -----------------------------------
            // STEP 2: Compare with incoming data
            // -----------------------------------
            var incomingSet = incomingChildPKIDs.Distinct().ToHashSet();
            var existingSet = existingMap.Keys.ToHashSet();

            var toInsert = incomingSet.Except(existingSet).ToList();
            var toDeactivate = existingSet.Except(incomingSet).ToList();
            var toReactivate = existingSet
                .Where(id => incomingSet.Contains(id) && existingMap[id] == false)
                .ToList();

            // -----------------------------------
            // STEP 3: Soft delete removed mappings
            // -----------------------------------
            if (toDeactivate.Any())
            {
                string deactivateQuery = $@"
                UPDATE {tableName}
                SET c_isactive = 0
                WHERE {parentColumnName} = @ParentID
                  AND {childColumnName} IN ({string.Join(",", toDeactivate)})";

                await _db.ExecuteNonQueryAsync(
                    deactivateQuery,
                    new[] { new SqlParameter("@ParentID", parentPKID) }
                );
            }

            // -----------------------------------
            // STEP 4: Reactivate existing mappings
            // -----------------------------------
            if (toReactivate.Any())
            {
                string reactivateQuery = $@"
                UPDATE {tableName}
                SET c_isactive = 1
                WHERE {parentColumnName} = @ParentID
                  AND {childColumnName} IN ({string.Join(",", toReactivate)})";

                await _db.ExecuteNonQueryAsync(
                    reactivateQuery,
                    new[] { new SqlParameter("@ParentID", parentPKID) }
                );
            }

            // -----------------------------------
            // STEP 5: Insert new mappings
            // -----------------------------------
            foreach (var childId in toInsert)
            {
                string insertQuery = $@"
                INSERT INTO {tableName}
                ({parentColumnName}, {childColumnName}, c_isactive)
                VALUES (@ParentID, @ChildID, 1)";

                await _db.ExecuteNonQueryAsync(
                    insertQuery,
                    new[]
                    {
                    new SqlParameter("@ParentID", parentPKID),
                    new SqlParameter("@ChildID", childId)
                    }
                );
            }
        }

        public async Task HardDeleteByParentAsync(string tableName, string parentColumnName, long parentPKID)
        {
            string query = $@"
            DELETE FROM {tableName}
            WHERE {parentColumnName} = @ParentID";

            await _db.ExecuteNonQueryAsync(
                query,
                new[] { new SqlParameter("@ParentID", parentPKID) }
            );
        }

        // ✅ Soft delete children by parent
        public async Task DeactivateByParentIdAsync(string tableName, string parentColumnName, long parentPKID)
        {
            string query = $@"
            UPDATE {tableName}
            SET c_isactive = 0
            WHERE {parentColumnName} = @ParentID
              AND c_isactive = 1";

            await _db.ExecuteNonQueryAsync(
                query,
                new[] { new SqlParameter("@ParentID", parentPKID) }
            );
        }

        public async Task<List<long>> GetChildIdsByParentAsync(string tableName, string parentColumnName, string childColumnName, long parentPKID, bool onlyActive = true)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append($@"
                SELECT {childColumnName}
                FROM {tableName}
                WHERE {parentColumnName} = @ParentID");

                if (onlyActive)
                    query.Append(" AND c_isactive = 1");

                var parameters = new[]
                {
                    new SqlParameter("@ParentID", parentPKID)
                };

                var dataTable = await _db.ExecuteAsync(query.ToString(), parameters);

                if (dataTable.Rows.Count == 0)
                    return new List<long>();

                return dataTable.Rows
                    .Cast<System.Data.DataRow>()
                    .Select(r => Convert.ToInt64(r[childColumnName]))
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching child IDs from {tableName}: {ex.Message}", ex);
            }
        }

    }
}
