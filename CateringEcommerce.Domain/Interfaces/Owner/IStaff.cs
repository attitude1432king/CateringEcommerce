using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IStaff
    {
        Task<int> AddStaffAsync(long ownerPKID, StaffDto staff);
        Task<int> UpdateStaffAsync(long ownerPKID, StaffDto staff);
        Task<int> DeleteStaffAsync(long ownerPKID, long staffPKID);
        Task<List<StaffModel>> GetStaffListAsync(long ownerPKID, int page, int pageSize, string filterJson);
        Task<int> GetStaffCountAsync(long ownerPKID, string filterJson);
        Task<bool> IsStaffNumberExistsAsync(long ownerPKID, string number, long? excludeStaffPKID = null);
        Task<int> UpdateStaffDocumentPath(long ownerPKID, long? staffPKID, Dictionary<string, string> dicPath);
        Task<bool> TryClearStaffFilePathAsync(long ownerPKID, long? staffId, string filePath);
        Task<List<string>> GetAllStaffFilePathsAsync(long ownerPKID, long? staffId);
        Task<bool> IsValidStaffId(long ownerPKID, long staffPKID);
        Task UpdateStaffStatus(long ownerPKID, long staffPKID, bool status);
    }
}
