using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IAdminSearchRepository
    {
        Task<GlobalSearchResponse> GlobalSearchAsync(GlobalSearchRequest request, long adminId);
    }
}
