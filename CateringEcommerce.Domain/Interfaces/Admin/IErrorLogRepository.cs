using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IErrorLogRepository
    {
        Task<long> CreateAsync(ErrorLogEntry entry);
        Task<ErrorLogListResponse> GetLogsAsync(ErrorLogListRequest request);
        Task<ErrorLogDetail?> GetByIdAsync(long id);
        Task<int> DeleteBeforeAsync(DateTime beforeDate);
    }
}
