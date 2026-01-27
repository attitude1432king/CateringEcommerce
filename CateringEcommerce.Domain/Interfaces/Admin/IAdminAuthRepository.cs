using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IAdminAuthRepository
    {
        AdminModel? AuthenticateAdmin(string username, string passwordHash);
        AdminModel? GetAdminById(long adminId);
        void UpdateLastLogin(long adminId);
        void IncrementFailedLoginAttempts(string username);
        void ResetFailedLoginAttempts(long adminId);
        void LockAccount(string username, DateTime lockUntil);
        bool IsAccountLocked(string username);
        void LogAdminActivity(long adminId, string action, string? details = null);
    }
}
