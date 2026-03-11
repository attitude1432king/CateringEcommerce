namespace CateringEcommerce.Domain.Interfaces
{
    public interface ISystemSettingsProvider
    {
        string GetString(string key, string defaultValue = "");
        int GetInt(string key, int defaultValue = 0);
        bool GetBool(string key, bool defaultValue = false);
        decimal GetDecimal(string key, decimal defaultValue = 0m);
        Task RefreshAsync();
        Dictionary<string, string> GetPublicSettings();
    }
}
