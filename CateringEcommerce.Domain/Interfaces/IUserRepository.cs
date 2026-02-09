using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.Domain.Interfaces
{
    public interface IUserRepository
    {
        UserModel GetUserDetails(Int64 userPKID);
        bool IsExistEmail(string email, string role = "User");
        bool IsExistNumber(string phoneNumber, string role);
        bool IsExistRoleBaseNumber(string phoneNumber, string type, string role);
    }
}
