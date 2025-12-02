using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IDecorations
    {
        Task<int> GetDecorationsCount(long ownerPKID, string filterJson);
        Task<List<DecorationsModel>> GetDecorations(long ownerPKID, int page, int pageSize, string filterJson);
        Task<long> AddDecoration(long ownerPKID, DecorationsDto decoration);
        Task<int> DeleteDecoration(long ownerPKID, long decorationId);
        Task<int> UpdateDecoration(long ownerPKID, DecorationsDto decoration);

        Task<List<DecorationThemeModel>> GetDecorationThemes();
        Task<bool> IsDecorationNameExistsAsync(long ownerPKID, string decorationName, long? decorationId = null);
        bool IsValidDecorationID(long ownerPKID, long decorationId);
        Task UpdateDecorationStatus(long ownerPKID, long decorationId, bool status);

    }
}
