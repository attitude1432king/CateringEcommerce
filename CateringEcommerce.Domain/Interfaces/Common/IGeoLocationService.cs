using CateringEcommerce.Domain.Models.Common;

namespace CateringEcommerce.Domain.Interfaces.Common
{
    public interface IGeoLocationService
    {
        Task<GeoCityResult?> ResolveCityAsync(string ipAddress);
    }
}
