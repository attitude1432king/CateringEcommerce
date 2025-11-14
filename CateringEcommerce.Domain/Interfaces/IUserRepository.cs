using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.Domain.Interfaces
{
    public interface IUserRepository
    {
        public UserModel GetUserDetails(Int64 userPKID);
    }
}
