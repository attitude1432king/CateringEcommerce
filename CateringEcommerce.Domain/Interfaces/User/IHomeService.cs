using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.Domain.Interfaces.User
{
    public interface IHomeService
    {
        /// <summary>
        /// Gets verified catering businesses by city ID.
        /// If cityId is less than or equal to 0, returns all verified catering businesses.
        /// </summary>
        /// <param name="cityId">The city ID to filter by. Pass 0 or negative value to get all.</param>
        /// <returns>List of verified catering businesses</returns>
        Task<List<CateringBusinessListDto>> GetVerifiedCateringListAsync(string cityName);
    }
}
