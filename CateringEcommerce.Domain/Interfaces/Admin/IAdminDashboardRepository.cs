using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IAdminDashboardRepository
    {
        AdminDashboardMetrics GetDashboardMetrics();
    }
}
