using CateringEcommerce.Domain.Enums;

namespace CateringEcommerce.Domain.Interfaces.Common
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(string base64Data, long ownerPkid, string documentType, bool isSecure, string fileName = null, long? entityPkid = null);
        public void DeleteFilePath(string relativePath);
        Task<string> SaveFormFileAsync(IFormFile file, long ownerPkid, string documentType, bool isSecure, string fileName = null, long? entityPkid = null);
    }
}
