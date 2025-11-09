using CateringEcommerce.Domain.Enums;

namespace CateringEcommerce.Domain.Interfaces.Common
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(string base64Data, long ownerPkid, string documentType, bool isSecure, string fileName = "");
        public void DeleteFilePath(string relativePath);
    }
}
