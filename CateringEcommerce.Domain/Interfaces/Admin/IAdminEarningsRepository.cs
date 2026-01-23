using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IAdminEarningsRepository
    {
        AdminEarningsSummary GetEarningsSummary();
        List<AdminEarningsByDateItem> GetEarningsByDate(AdminEarningsByDateRequest request);
        AdminEarningsByCateringResponse GetEarningsByCatering(AdminEarningsByCateringRequest request);
        List<AdminMonthlyReportItem> GetMonthlyReport(int year);
    }
}
