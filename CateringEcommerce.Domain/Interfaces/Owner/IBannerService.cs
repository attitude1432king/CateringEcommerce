using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IBannerService
    {
        Task<int> GetBannersCount(long ownerPKID, BannerFilter filter);
        Task<List<BannerDto>> GetBanners(long ownerPKID, int page, int pageSize, BannerFilter filter);
        Task<long> AddBanner(long ownerPKID, BannerDto banner);
        Task<bool> UpdateBanner(long ownerPKID, BannerDto banner);
        Task<bool> DeleteBanner(long ownerPKID, long bannerID);
        Task<bool> UpdateBannerStatus(long ownerPKID, long bannerId, bool isActive);
        Task<bool> IsBannerTitleExists(long ownerPKID, string title, long? excludeId = null);
        Task<List<BannerDto>> GetActiveBannersForHomepage();
        Task IncrementViewCount(long bannerId);
        Task IncrementClickCount(long bannerId);
    }
}
