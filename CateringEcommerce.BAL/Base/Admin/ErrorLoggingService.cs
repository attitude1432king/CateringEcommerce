using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Extensions.Logging;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class ErrorLoggingService : IErrorLoggingService
    {
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly ILogger<ErrorLoggingService> _logger;

        public ErrorLoggingService(
            IErrorLogRepository errorLogRepository,
            ILogger<ErrorLoggingService> logger)
        {
            _errorLogRepository = errorLogRepository ?? throw new ArgumentNullException(nameof(errorLogRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task LogAsync(ErrorLogEntry entry)
        {
            try
            {
                await _errorLogRepository.CreateAsync(entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist error log with ErrorId {ErrorId}", entry.ErrorId);
            }
        }
    }
}
