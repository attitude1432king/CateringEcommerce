using Microsoft.AspNetCore.Http;
using System.Text;

namespace CateringEcommerce.BAL.Helpers
{
    /// <summary>
    /// File validation helper for secure file upload handling
    /// Validates file signatures (magic numbers) to prevent malicious file uploads
    /// </summary>
    public static class FileValidationHelper
    {
        // File signature definitions (magic numbers)
        private static readonly Dictionary<string, List<byte[]>> FileSignatures = new()
        {
            // Image formats
            {
                ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },  // JPEG JFIF
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },  // JPEG EXIF
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },  // JPEG (Canon)
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },  // JPEG
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xDB },  // JPEG RAW
                }
            },
            {
                ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xDB },
                }
            },
            {
                ".png", new List<byte[]>
                {
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
                }
            },
            {
                ".gif", new List<byte[]>
                {
                    new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 },  // GIF87a
                    new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }   // GIF89a
                }
            },
            {
                ".webp", new List<byte[]>
                {
                    new byte[] { 0x52, 0x49, 0x46, 0x46 }  // RIFF (check WEBP at offset 8)
                }
            },

            // Video formats
            {
                ".mp4", new List<byte[]>
                {
                    new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 },  // ftyp
                    new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70 },
                    new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70 }
                }
            },
            {
                ".avi", new List<byte[]>
                {
                    new byte[] { 0x52, 0x49, 0x46, 0x46 }  // RIFF (check AVI at offset 8)
                }
            },

            // Document formats
            {
                ".pdf", new List<byte[]>
                {
                    new byte[] { 0x25, 0x50, 0x44, 0x46 }  // %PDF
                }
            }
        };

        // Allowed MIME types mapping
        private static readonly Dictionary<string, List<string>> AllowedMimeTypes = new()
        {
            { ".jpg", new List<string> { "image/jpeg", "image/jpg" } },
            { ".jpeg", new List<string> { "image/jpeg", "image/jpg" } },
            { ".png", new List<string> { "image/png" } },
            { ".gif", new List<string> { "image/gif" } },
            { ".webp", new List<string> { "image/webp" } },
            { ".mp4", new List<string> { "video/mp4" } },
            { ".avi", new List<string> { "video/x-msvideo", "video/avi" } },
            { ".pdf", new List<string> { "application/pdf" } }
        };

        /// <summary>
        /// Maximum file size in bytes (10 MB default)
        /// </summary>
        public const long MaxFileSizeBytes = 10 * 1024 * 1024;

        /// <summary>
        /// Validates file based on signature (magic numbers), extension, and MIME type
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <param name="allowedExtensions">Allowed file extensions (e.g., [".jpg", ".png"])</param>
        /// <param name="maxSizeBytes">Maximum file size in bytes</param>
        /// <returns>FileValidationResult with validation status and error message</returns>
        public static FileValidationResult ValidateFile(
            IFormFile file,
            string[] allowedExtensions,
            long maxSizeBytes = MaxFileSizeBytes)
        {
            if (file == null || file.Length == 0)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "File is required and cannot be empty."
                };
            }

            // 1. Check file size
            if (file.Length > maxSizeBytes)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File size exceeds maximum allowed size of {maxSizeBytes / (1024 * 1024)} MB."
                };
            }

            // 2. Check file extension
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File extension '{fileExtension}' is not allowed. Allowed: {string.Join(", ", allowedExtensions)}"
                };
            }

            // 3. Validate MIME type against extension
            if (AllowedMimeTypes.TryGetValue(fileExtension, out var allowedMimes))
            {
                if (!allowedMimes.Contains(file.ContentType.ToLowerInvariant()))
                {
                    return new FileValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"MIME type '{file.ContentType}' does not match file extension '{fileExtension}'."
                    };
                }
            }

            // 4. Validate file signature (magic numbers) - MOST IMPORTANT!
            if (!ValidateFileSignature(file, fileExtension))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "File signature validation failed. The file may be corrupted or is not a valid file of the specified type."
                };
            }

            // 5. Additional checks for specific file types
            if (fileExtension == ".webp" && !ValidateWebPSignature(file))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid WebP file signature."
                };
            }

            return new FileValidationResult
            {
                IsValid = true,
                ErrorMessage = null
            };
        }

        /// <summary>
        /// Validates file signature by reading magic numbers
        /// </summary>
        private static bool ValidateFileSignature(IFormFile file, string extension)
        {
            if (!FileSignatures.ContainsKey(extension))
            {
                // Extension not in our signature database - rely on other checks
                return true;
            }

            try
            {
                using var reader = new BinaryReader(file.OpenReadStream());
                var signatures = FileSignatures[extension];
                var headerBytes = reader.ReadBytes(signatures.Max(s => s.Length));

                // Check if file header matches any of the allowed signatures
                return signatures.Any(signature =>
                    headerBytes.Take(signature.Length).SequenceEqual(signature));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Special validation for WebP files (RIFF + WEBP string at offset 8)
        /// </summary>
        private static bool ValidateWebPSignature(IFormFile file)
        {
            try
            {
                using var reader = new BinaryReader(file.OpenReadStream());
                var headerBytes = reader.ReadBytes(12);

                // Check RIFF header
                if (!headerBytes.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 }))
                    return false;

                // Check WEBP string at offset 8
                var webpBytes = headerBytes.Skip(8).Take(4).ToArray();
                var webpString = Encoding.ASCII.GetString(webpBytes);
                return webpString == "WEBP";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sanitizes filename to prevent directory traversal and special characters
        /// </summary>
        /// <param name="filename">Original filename</param>
        /// <returns>Sanitized filename</returns>
        public static string SanitizeFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return $"file_{Guid.NewGuid():N}";

            // Remove path information
            filename = Path.GetFileName(filename);

            // Remove invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(filename.Where(c => !invalidChars.Contains(c)).ToArray());

            // Remove potentially dangerous patterns
            sanitized = sanitized
                .Replace("..", "")
                .Replace("./", "")
                .Replace("../", "")
                .Replace("\\", "");

            // Ensure extension is preserved
            var extension = Path.GetExtension(sanitized);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(sanitized);

            // Limit filename length (max 200 characters)
            if (nameWithoutExt.Length > 200)
                nameWithoutExt = nameWithoutExt.Substring(0, 200);

            // Generate unique filename to prevent overwriting
            var uniqueName = $"{nameWithoutExt}_{Guid.NewGuid():N}{extension}";

            return uniqueName;
        }

        /// <summary>
        /// Generates a safe, random filename with original extension
        /// </summary>
        /// <param name="originalFilename">Original filename</param>
        /// <returns>Safe random filename</returns>
        public static string GenerateSafeFilename(string originalFilename)
        {
            var extension = Path.GetExtension(originalFilename).ToLowerInvariant();
            return $"{Guid.NewGuid():N}{extension}";
        }

        /// <summary>
        /// Validates if file is an image
        /// </summary>
        public static bool IsImage(IFormFile file)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return imageExtensions.Contains(extension);
        }

        /// <summary>
        /// Validates if file is a video
        /// </summary>
        public static bool IsVideo(IFormFile file)
        {
            var videoExtensions = new[] { ".mp4", ".avi" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return videoExtensions.Contains(extension);
        }

        /// <summary>
        /// Gets the file extension from MIME type
        /// </summary>
        public static string GetExtensionFromMimeType(string mimeType)
        {
            var mapping = new Dictionary<string, string>
            {
                { "image/jpeg", ".jpg" },
                { "image/jpg", ".jpg" },
                { "image/png", ".png" },
                { "image/gif", ".gif" },
                { "image/webp", ".webp" },
                { "video/mp4", ".mp4" },
                { "video/x-msvideo", ".avi" },
                { "application/pdf", ".pdf" }
            };

            return mapping.TryGetValue(mimeType.ToLowerInvariant(), out var ext) ? ext : string.Empty;
        }
    }

    /// <summary>
    /// Result of file validation
    /// </summary>
    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
