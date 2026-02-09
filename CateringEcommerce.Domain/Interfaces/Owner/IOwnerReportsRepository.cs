using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IOwnerReportsRepository
    {
        /// <summary>
        /// Generate sales report
        /// </summary>
        Task<SalesReportDto> GenerateSalesReport(long ownerId, ReportFilterDto filter);

        /// <summary>
        /// Generate revenue report
        /// </summary>
        Task<RevenueReportDto> GenerateRevenueReport(long ownerId, ReportFilterDto filter);

        /// <summary>
        /// Generate customer report
        /// </summary>
        Task<CustomerReportDto> GenerateCustomerReport(long ownerId, ReportFilterDto filter);

        /// <summary>
        /// Generate menu performance report
        /// </summary>
        Task<MenuPerformanceReportDto> GenerateMenuPerformanceReport(long ownerId, ReportFilterDto filter);

        /// <summary>
        /// Generate financial report
        /// </summary>
        Task<FinancialReportDto> GenerateFinancialReport(long ownerId, ReportFilterDto filter);

        /// <summary>
        /// Export report to CSV format
        /// </summary>
        Task<byte[]> ExportReportToCSV(long ownerId, string reportType, ReportFilterDto filter);

        /// <summary>
        /// Export report to PDF format
        /// </summary>
        Task<byte[]> ExportReportToPDF(long ownerId, string reportType, ReportFilterDto filter);
    }
}
