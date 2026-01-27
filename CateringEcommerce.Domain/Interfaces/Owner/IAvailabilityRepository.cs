using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IAvailabilityRepository
    {
        Task<GlobalAvailabilityModel> GetAvailabilityForPageAsync(long ownerId, int year, int month);
        Task<int> GetGlobalStatusAsync(long ownerId);
        Task<Dictionary<string, DateAvailabilityPayload>> GetCurrentMonthDatesAsync(long ownerId, int year, int month);
        Task UpsertGlobalAsync(long ownerId, int status);
        Task UpsertDateAsync(long ownerId, DateTime date, int status, string? note);
    }
}
