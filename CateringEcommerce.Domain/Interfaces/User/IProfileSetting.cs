namespace CateringEcommerce.Domain.Interfaces.User
{
    public interface IProfileSetting
    {
        Task UpdateUserDetails(long? userPKID, Dictionary<string, string> dicData = null);
        
        string GetUserProfilePicture(long userPkid);
    }
}
