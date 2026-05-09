using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IErrorLoggingService
    {
        Task LogAsync(ErrorLogEntry entry);
    }
}
