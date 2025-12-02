using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.Common;
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

        public async Task<string> SaveFileAsync(string base64Data, long ownerPkid, string documentType, bool isSecure, string fileName = null, long? entityPkid = null)
        {
            if (string.IsNullOrEmpty(base64Data))
                return null;

            var parts = base64Data.Split(',');
            if (parts.Length != 2)
                throw new ArgumentException("Invalid Base64 string format.");

            var mimeType = parts[0].Split(';')[0].Split(':')[1];
            var extension = GetFileExtension(mimeType);
            var fileBytes = Convert.FromBase64String(parts[1]);

            // ✅ Base upload directory
            var basePath = isSecure
                ? Path.Combine(_env.WebRootPath, "secure_uploads")
                : Path.Combine(_env.WebRootPath, "uploads", "Media");

            // ✅ Start with owner folder
            var directoryPath = Path.Combine(basePath, $"owner{ownerPkid}");

            // ✅ Special case: hierarchical folder handling (Staff, etc.)
            if (!string.IsNullOrEmpty(documentType))
            {
                directoryPath = Path.Combine(directoryPath, documentType);

                // If it's a "staff" type or similar hierarchical structure, create subfolder like staff{PKID}
                if (documentType.Equals("Staff", StringComparison.OrdinalIgnoreCase) && entityPkid.HasValue && entityPkid.Value > 0)
                {
                    directoryPath = Path.Combine(directoryPath, $"staff{entityPkid.Value}");
                }
            }

            // ✅ Ensure directory exists
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            // ✅ Clean filename
            string CleanFileName(string name)
            {
                var invalidChars = Path.GetInvalidFileNameChars();
                return string.Concat(name.Where(c => !invalidChars.Contains(c))).Trim();
            }

            string finalFileName;

            if (isSecure)
            {
                var cleanedName = CleanFileName(Path.GetFileNameWithoutExtension(fileName));
                finalFileName = $"{cleanedName}{extension}";
            }
            else
            {
                finalFileName = string.IsNullOrEmpty(fileName)
                    ? $"{Guid.NewGuid()}{extension}"
                    : $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}{extension}";
            }

            // ✅ Full path and save
            var filePath = Path.Combine(directoryPath, finalFileName);
            await File.WriteAllBytesAsync(filePath, fileBytes);

            // ✅ Relative path for returning
            var relativePath = Path.Combine(
                isSecure ? "/secure_uploads" : "/uploads/Media",
                $"owner{ownerPkid}",
                !string.IsNullOrEmpty(documentType) ? documentType : "",
                (documentType.Equals("Staff", StringComparison.OrdinalIgnoreCase) && entityPkid.HasValue)
                    ? $"staff{entityPkid.Value}"
                    : "",
                finalFileName
            ).Replace(Path.DirectorySeparatorChar, '/');

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


        public void DeleteFilePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            // The path from the DB might start with a '/', remove it to correctly join with the root path.
            var pathToDelete = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));

            if (File.Exists(pathToDelete))
            {
                try
                {
                    File.Delete(pathToDelete);
                }
                catch (Exception ex)
                {
                    // Log the exception, but don't let it crash the request.
                    Console.WriteLine($"Error deleting file {pathToDelete}: {ex.Message}");
                }
            }
        }

        public async Task<string> SaveFormFileAsync(IFormFile file, long ownerPkid, string documentType, bool isSecure, string fileName = null, long? entityPkid = null)
        {
            if (file == null || file.Length == 0)
                return null;

            // Base upload directory
            var basePath = isSecure
                ? Path.Combine(_env.WebRootPath, "secure_uploads")
                : Path.Combine(_env.WebRootPath, "uploads", "Media");

            // Start with owner folder
            var directoryPath = Path.Combine(basePath, $"owner{ownerPkid}");

            // Document type and optional hierarchical folder (e.g., Staff/staff{N})
            if (!string.IsNullOrEmpty(documentType))
            {
                directoryPath = Path.Combine(directoryPath, documentType);

                if (documentType.Equals("Staff", StringComparison.OrdinalIgnoreCase) && entityPkid.HasValue && entityPkid.Value > 0)
                {
                    directoryPath = Path.Combine(directoryPath, $"staff{entityPkid.Value}");
                }
            }

            // Ensure directory exists
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            // Helper to clean file name
            string CleanFileName(string name)
            {
                var invalidChars = Path.GetInvalidFileNameChars();
                return string.Concat(name.Where(c => !invalidChars.Contains(c))).Trim();
            }

            // Determine extension (prefer original file extension)
            var originalName = fileName ?? file.FileName;
            var extension = Path.GetExtension(originalName);
            if (string.IsNullOrEmpty(extension))
            {
                // fallback: try to map from ContentType (optional)
                extension = GetFileExtension(file.ContentType) ?? "";
            }

            // Final file name logic (match SaveFileAsync behavior)
            string finalFileName;
            if (isSecure)
            {
                var cleanedName = CleanFileName(Path.GetFileNameWithoutExtension(originalName));
                finalFileName = $"{cleanedName}{extension}";
            }
            else
            {
                // use GUID name for non-secure uploads
                finalFileName = $"{Guid.NewGuid()}{extension}";
            }

            var fullPath = Path.Combine(directoryPath, finalFileName);

            // Save file to disk asynchronously
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Build relative path (same shape as SaveFileAsync)
            var relativePathParts = new List<string>
            {
                isSecure ? "secure_uploads" : "uploads/Media",
                $"owner{ownerPkid}"
            };

            if (!string.IsNullOrEmpty(documentType))
                relativePathParts.Add(documentType);

            if (documentType.Equals("Staff", StringComparison.OrdinalIgnoreCase) && entityPkid.HasValue)
                relativePathParts.Add($"staff{entityPkid.Value}");

            relativePathParts.Add(finalFileName);

            var relativePath = "/" + Path.Combine(relativePathParts.ToArray()).Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');

            return relativePath;
        }

    }
}
