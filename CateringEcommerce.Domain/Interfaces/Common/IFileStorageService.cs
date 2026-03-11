using CateringEcommerce.Domain.Enums;

namespace CateringEcommerce.Domain.Interfaces.Common
{
    public interface IFileStorageService
    {
        // Owner file uploads
        Task<string> SaveFileAsync(string base64Data, long ownerPkid, string documentType, bool isSecure, string fileName = null, long? entityPkid = null);
        Task<string> SaveFormFileAsync(IFormFile file, long ownerPkid, string documentType, bool isSecure, string fileName = null, long? entityPkid = null);
        
        // User file uploads
        Task<string> SaveUserFileAsync(string base64Data, long userPkid, string documentType, string fileName = null);
        Task<string> SaveRoleBaseFormFileAsync(IFormFile file, long userPkid, string role, bool isSecure, string documentType, string fileName = null);

        // File deletion
        void DeleteFilePath(string relativePath);
    }
}
