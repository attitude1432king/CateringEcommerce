using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.Domain.Interfaces.User
{
    public interface IAuthentication
    {
        Task<int> CreateUserAccount(string name, string phoneNumber = null, Dictionary<string, string> dicData = null);
        Task<UserModel?> GetUserData(string? phoneNumber = null);
    }
}
