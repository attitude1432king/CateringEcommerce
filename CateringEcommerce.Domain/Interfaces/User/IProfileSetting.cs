namespace CateringEcommerce.Domain.Interfaces.User
{
    public interface IProfileSetting
    {
        void UpdateUserDetails(long? userPKID, Dictionary<string, string> dicData = null);
    }
}
