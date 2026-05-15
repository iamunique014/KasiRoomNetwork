using Microsoft.AspNetCore.Http;

namespace Kasi_Room_Network___KRN.Services
{
    public class PhotoStorageService : IPhotoStorageService
    {
        private const long MaxPhotoSizeBytes = 2 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PhotoStorageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveTemporaryPhotoAsync(IFormFile? photo, string landlordUserId)
        {
            if (photo == null || photo.Length == 0)
            {
                throw new InvalidOperationException("Please upload a photo.");
            }

            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Only JPG and PNG images are allowed.");
            }

            if (photo.Length > MaxPhotoSizeBytes)
            {
                throw new InvalidOperationException("Image size cannot exceed 2MB.");
            }

            var safeLandlordUserId = SanitizePathSegment(landlordUserId);
            var uploadsFolder = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "wizard-temp",
                safeLandlordUserId);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            return $"/uploads/wizard-temp/{safeLandlordUserId}/{fileName}";
        }

        public void DeleteTemporaryPhoto(string tempRelativePath)
        {
            if (string.IsNullOrWhiteSpace(tempRelativePath))
            {
                return;
            }

            var normalizedRelativePath = tempRelativePath.TrimStart('/', '\\')
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(_webHostEnvironment.WebRootPath, normalizedRelativePath));
            var webRootPath = Path.GetFullPath(_webHostEnvironment.WebRootPath);

            var webRootPathWithSeparator = webRootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(webRootPathWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        public void DeleteTemporaryPhotos(IEnumerable<string>? tempRelativePaths)
        {
            if (tempRelativePaths == null)
            {
                return;
            }

            foreach (var path in tempRelativePaths)
            {
                try
                {
                    DeleteTemporaryPhoto(path);
                }
                catch
                {
                    // Ignore individual failures
                }
            }
        }

        public void DeleteLandlordTemporaryPhotos(string landlordUserId)
        {
            var safeLandlordUserId = SanitizePathSegment(landlordUserId);

            var folderPath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "wizard-temp",
                safeLandlordUserId);

            if (!Directory.Exists(folderPath))
            {
                return;
            }

            try
            {
                Directory.Delete(folderPath, true);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
        private static string SanitizePathSegment(string value)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(value.Select(character => invalidChars.Contains(character) ? '-' : character).ToArray());
            return string.IsNullOrWhiteSpace(sanitized) ? "unknown" : sanitized;
        }
        public void CleanupExpiredTemporaryPhotos(TimeSpan maxAge)
        {
            var tempRoot = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "wizard-temp");

            if (!Directory.Exists(tempRoot))
            {
                return;
            }

            var directories = Directory.GetDirectories(tempRoot);

            foreach (var directory in directories)
            {
                try
                {
                    var creationTime = Directory.GetCreationTimeUtc(directory);

                    if (DateTime.UtcNow - creationTime > maxAge)
                    {
                        Directory.Delete(directory, true);
                    }
                }
                catch
                {
                    // Ignore cleanup failures
                }
            }
        }
    }
}
