namespace CateringEcommerce.Domain.Interfaces
{
    /// <summary>
    /// Service for synchronizing mapping tables using soft-delete strategy
    /// </summary>
    public interface IMappingSyncService
    {
        /// <summary>
        /// Synchronizes any mapping table using soft-delete strategy.
        /// - Inserts new mappings
        /// - Reactivates existing inactive mappings
        /// - Soft deletes removed mappings
        /// - Leaves unchanged mappings untouched
        /// </summary>
        Task SyncAsync(string tableName, string parentColumnName, string childColumnName, long parentPKID, List<long> incomingChildPKIDs);

        /// <summary>
        /// Hard deletes all mappings for a given parent ID
        /// </summary>
        Task HardDeleteByParentAsync(string tableName, string parentColumnName, long parentPKID);

        /// <summary>
        /// Deactivates (soft deletes) all mappings for a given parent ID
        /// </summary>
        Task DeactivateByParentIdAsync(string tableName, string parentColumnName, long parentPKID);

        /// <summary>
        /// Gets all child IDs for a given parent ID
        /// </summary>
        Task<List<long>> GetChildIdsByParentAsync(string tableName, string parentColumnName, string childColumnName, long parentPKID, bool onlyActive = true);
    }
}
