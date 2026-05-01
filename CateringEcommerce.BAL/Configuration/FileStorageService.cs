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

        public async Task<string> SaveRoleBaseFormFileAsync(IFormFile file, long pkid, string role, string documentType, bool isSecure, string fileName = null, long? entityPkid = null)
        {
            if (file == null || file.Length == 0)
                return null;

            // Base upload directory
            var basePath = isSecure
                ? Path.Combine(_env.WebRootPath, "secure_uploads")
                : Path.Combine(_env.WebRootPath, "uploads", "Media");

            // ✅ Pluralize role name for folder (User -> Users, Supervisor -> Supervisors, Admin -> Admins)
            var pluralRole = string.IsNullOrEmpty(role) ? "Users" : role + "s";

            // ✅ Start with pluralized role folder
            var directoryPath = Path.Combine(basePath, pluralRole);

            // ✅ Add role{userPkid} subfolder
            directoryPath = Path.Combine(directoryPath, $"{role?.ToLower() ?? "user"}{pkid}");

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

            // ✅ Build relative path
            var relativePathParts = new List<string>
            {
                isSecure ? "secure_uploads" : "uploads/Media",
                pluralRole,
                $"{role?.ToLower() ?? "user"}{pkid}"
            };

            if (!string.IsNullOrEmpty(documentType))
                relativePathParts.Add(documentType);

            if (documentType.Equals("Staff", StringComparison.OrdinalIgnoreCase) && entityPkid.HasValue)
                relativePathParts.Add($"staff{entityPkid.Value}");

            relativePathParts.Add(finalFileName);

            var relativePath = "/" + Path.Combine(relativePathParts.ToArray()).Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');

            return relativePath;
        }

        //public async Task<string> SaveRoleBaseFormFileAsync(IFormFile file, long userPkid, string role, bool isSecure, string documentType, string fileName = null)
        //{
        //    if (file == null || file.Length == 0)
        //        return null;

        //    // ✅ Base upload directory based on security flag
        //    var basePath = isSecure
        //        ? Path.Combine(_env.WebRootPath, "secure_uploads")
        //        : Path.Combine(_env.WebRootPath, "uploads", "Media");

        //    // ✅ Pluralize role name for folder (User -> Users, Supervisor -> Supervisors, Admin -> Admins)
        //    var pluralRole = string.IsNullOrEmpty(role) ? "Users" : role + "s";

        //    // ✅ Start with pluralized role folder
        //    var directoryPath = Path.Combine(basePath, pluralRole);

        //    // ✅ Add role{userPkid} subfolder
        //    directoryPath = Path.Combine(directoryPath, $"{role?.ToLower() ?? "user"}{userPkid}");

        //    // ✅ Add document type subfolder
        //    if (!string.IsNullOrEmpty(documentType))
        //    {
        //        directoryPath = Path.Combine(directoryPath, documentType);
        //    }

        //    // ✅ Ensure directory exists
        //    if (!Directory.Exists(directoryPath))
        //        Directory.CreateDirectory(directoryPath);

        //    // ✅ Clean filename helper
        //    string CleanFileName(string name)
        //    {
        //        var invalidChars = Path.GetInvalidFileNameChars();
        //        return string.Concat(name.Where(c => !invalidChars.Contains(c))).Trim();
        //    }

        //    // ✅ Determine extension
        //    var originalName = fileName ?? file.FileName;
        //    var extension = Path.GetExtension(originalName);
        //    if (string.IsNullOrEmpty(extension))
        //    {
        //        extension = GetFileExtension(file.ContentType) ?? "";
        //    }

        //    // ✅ Generate final filename with GUID for uniqueness
        //    string finalFileName = $"{Guid.NewGuid()}{extension}";

        //    var fullPath = Path.Combine(directoryPath, finalFileName);

        //    // ✅ Save file to disk asynchronously
        //    using (var stream = new FileStream(fullPath, FileMode.Create))
        //    {
        //        await file.CopyToAsync(stream);
        //    }

        //    // ✅ Build relative path
        //    var relativePathParts = new List<string>
        //    {
        //        isSecure ? "secure_uploads" : "uploads/Media",
        //        pluralRole,
        //        $"{role?.ToLower() ?? "user"}{userPkid}"
        //    };

        //    if (!string.IsNullOrEmpty(documentType))
        //        relativePathParts.Add(documentType);

        //    relativePathParts.Add(finalFileName);

        //    var relativePath = "/" + Path.Combine(relativePathParts.ToArray()).Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');

        //    return relativePath;
        //}

    }
}
