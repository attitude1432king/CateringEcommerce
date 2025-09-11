using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace CateringEcommerce.BAL.Configuration
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;


        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(string base64Data, Int64 ownerPkid, string documentType, bool isSecure, string fileName = null)
        {
            if (string.IsNullOrEmpty(base64Data))
            {
                return null;
            }

            var parts = base64Data.Split(',');
            if (parts.Length != 2) throw new ArgumentException("Invalid Base64 string format.");

            var mimeType = parts[0].Split(';')[0].Split(':')[1];
            var extension = GetFileExtension(mimeType);
            var fileBytes = Convert.FromBase64String(parts[1]);

            var basePath = isSecure
                ? Path.Combine(_env.WebRootPath, "secure_uploads")
                : Path.Combine(_env.WebRootPath, "uploads", "Media");

            var directoryPath = Path.Combine(basePath, $"owner{ownerPkid}", documentType);
            if(!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string CleanFileName(string name)
            {
                var invalidChars = Path.GetInvalidFileNameChars();
                return string.Concat(name.Where(c => !invalidChars.Contains(c))).Trim();
            }

            string finalFileName;

            if (isSecure)
            {
                // Use original filename directly (after cleaning)
                var cleanedName = CleanFileName(Path.GetFileNameWithoutExtension(fileName));
                finalFileName = $"{cleanedName}{extension}";
            }
            else
            {
                // Use GUID-based unique name
                finalFileName = string.IsNullOrEmpty(fileName)
                    ? $"{Guid.NewGuid()}{extension}"
                    : $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}{extension}";
                finalFileName = $"{Guid.NewGuid()}{extension}";
            }


            var filePath = Path.Combine(directoryPath, finalFileName);

            await File.WriteAllBytesAsync(filePath, fileBytes);

            var relativePath = Path.Combine(isSecure ? "/secure_uploads" : "/uploads/Media", $"owner{ownerPkid}", documentType, finalFileName)
                                   .Replace(Path.DirectorySeparatorChar, '/');

            return relativePath;
        }

        private string GetFileExtension(string mimeType)
        {
            return mimeType.ToLower() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "video/mp4" => ".mp4",
                "application/pdf" => ".pdf",
                _ => string.Empty,
            };
        }
    }
}
