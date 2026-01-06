using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IAvailabilityRepository
    {
        Task<GlobalAvailabilityModel> GetAvailabilityForPageAsync(long ownerId);
        Task<string?> GetGlobalStatusAsync(long ownerId);
        Task<Dictionary<string, DateAvailabilityPayload>> GetCurrentMonthDatesAsync(long ownerId);
        Task UpsertGlobalAsync(long ownerId, string status);
        Task UpsertDateAsync(long ownerId, DateTime date, string status, string? note);
    }
}
