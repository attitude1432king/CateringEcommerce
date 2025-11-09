using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IDecorations
    {
        Task<List<DecorationsModel>> GetDecorations(long ownerPKID);
        Task<long> AddDecoration(long ownerPKID, DecorationsDto decoration);
        Task<int> DeleteDecoration(long ownerPKID, long decorationId);
        Task<int> UpdateDecoration(long ownerPKID, DecorationsDto decoration);

        Task<List<DecorationThemeModel>> GetDecorationThemes();
        Task<bool> IsDecorationNameExistsAsync(long ownerPKID, string decorationName, long? decorationId = null);
        bool IsValidDecorationID(long ownerPKID, long decorationId);
    }
}
