using CateringEcommerce.Domain.Models;

namespace CateringEcommerce.Domain.Interfaces
{
    public interface IPublicStatsRepository
    {
        Task<PartnerStats> GetPartnerStatsAsync();
    }
}
