using CateringEcommerce.Domain.Enums;

namespace CateringEcommerce.Domain.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(string base64Data, Int64 ownerPkid, string documentType, bool isSecure, string fileName = null);
    }
}
