using CateringEcommerce.Domain.Enums;

namespace CateringEcommerce.Domain.Interfaces.Common
{
    public interface IFileStorageService
    {
        // Owner file uploads
        Task<string> SaveRoleBaseFormFileAsync(IFormFile file, long pkid, string role, string documentType, bool isSecure, string fileName = null, long? entityPkid = null);

        // File deletion
        void DeleteFilePath(string relativePath);
    }
}
