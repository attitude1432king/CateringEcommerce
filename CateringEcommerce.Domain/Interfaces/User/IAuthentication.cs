using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.Domain.Interfaces.User
{
    public interface IAuthentication
    {
        int CreateUserAccount(string name, string phoneNumber = null, Dictionary<string, string> dicData = null);
        UserModel? GetUserData(string? phoneNumber = null);
    }
}
