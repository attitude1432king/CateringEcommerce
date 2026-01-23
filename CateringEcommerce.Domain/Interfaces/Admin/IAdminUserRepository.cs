using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IAdminUserRepository
    {
        AdminUserListResponse GetAllUsers(AdminUserListRequest request);
        AdminUserDetail? GetUserById(long userId);
        bool UpdateUserStatus(AdminUserStatusUpdate request);
    }
}
