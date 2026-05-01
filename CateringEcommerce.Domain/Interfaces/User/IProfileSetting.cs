namespace CateringEcommerce.Domain.Interfaces.User
{
    public interface IProfileSetting
    {
        Task UpdateUserDetails(long? userPKID, Dictionary<string, string> dicData = null);
        
        Task<string> GetUserProfilePicture(long userPkid);
    }
}
