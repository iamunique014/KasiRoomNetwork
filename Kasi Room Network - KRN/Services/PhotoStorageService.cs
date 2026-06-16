using Kasi_Room_Network___KRN.Constants;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Kasi_Room_Network___KRN.Services
{
    public class PhotoStorageService : IPhotoStorageService
    {
        private const long MaxPhotoSizeBytes = 5 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
        private readonly IWebHostEnvironment _webHostEnvironment;
        private string UploadRootFolder =>
            _webHostEnvironment.IsEnvironment("Test")
        ? "uploads-test"
        : "uploads";

        public PhotoStorageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveTemporaryPhotoAsync(IFormFile? photo, string landlordUserId)
        {
            ValidatePhoto(photo);

            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            
            var safeLandlordUserId = SanitizePathSegment(landlordUserId);
            var uploadsFolder = Path.Combine(
                _webHostEnvironment.WebRootPath,
                UploadRootFolder,
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

            return $"/{UploadRootFolder}/wizard-temp/{safeLandlordUserId}/{fileName}";
        }


        public async Task<string> CopyTemporaryPhotoToPermanentAsync(string tempRelativePath, string permanentFolderName)
        {
            if (string.IsNullOrWhiteSpace(tempRelativePath))
            {
                throw new InvalidOperationException("Photo path is missing.");
            }

            if (string.IsNullOrWhiteSpace(permanentFolderName))
            {
                throw new InvalidOperationException("Permanent photo folder is missing.");
            }

            var normalizedRelativePath = tempRelativePath.TrimStart('/', '\\')
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            var sourcePath = Path.GetFullPath(Path.Combine(_webHostEnvironment.WebRootPath, normalizedRelativePath));
            var tempRootPath = Path.GetFullPath(Path.Combine(_webHostEnvironment.WebRootPath, UploadRootFolder, "wizard-temp"));
            var tempRootWithSeparator = tempRootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!sourcePath.StartsWith(tempRootWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only temporary wizard photos can be submitted.");
            }

            if (!System.IO.File.Exists(sourcePath))
            {
                throw new InvalidOperationException("One of your uploaded photos could not be found. Please upload it again.");
            }

            var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Only JPG, JPEG and PNG images are allowed.");
            }

            var safePermanentFolderName = SanitizePathSegment(permanentFolderName);
            var uploadsFolder = Path.Combine(
                _webHostEnvironment.WebRootPath,
                UploadRootFolder,
                safePermanentFolderName);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var destinationPath = Path.Combine(uploadsFolder, fileName);

            await using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            await using (var destinationStream = new FileStream(destinationPath, FileMode.CreateNew))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }

            return $"/{UploadRootFolder}/{safePermanentFolderName}/{fileName}";
        }

        public void DeleteTemporaryWizardFolder(string landlordUserId)
        {
            DeleteLandlordTemporaryPhotos(landlordUserId);
        }

        public void DeleteTemporaryPhoto(string tempRelativePath)
        {
            DeletePhoto(tempRelativePath);
        }

        public void DeleteTemporaryPhotos(IEnumerable<string>? tempRelativePaths)
        {
            DeletePhotos(tempRelativePaths);
        }

        public void DeletePhoto(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            var normalizedRelativePath = relativePath.TrimStart('/', '\\')
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

        public void DeletePhotos(IEnumerable<string>? relativePaths)
        {
            if (relativePaths == null)
            {
                return;
            }

            foreach (var path in relativePaths)
            {
                try
                {
                    DeletePhoto(path);
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
                UploadRootFolder,
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
                UploadRootFolder,
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
        public async Task<string> SaveOptimizedImageAsync(IFormFile photo,ImageCategory category)
        {
            ValidatePhoto(photo);

            var folderName = category switch
            {
                ImageCategory.Listing => "listings",
                ImageCategory.Property => "properties",
                _ => throw new InvalidOperationException(
                    "Unsupported image category.")
            };

            var uploadsFolder = Path.Combine(
                _webHostEnvironment.WebRootPath,
                UploadRootFolder,
                folderName);

            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}.jpg";

            var filePath = Path.Combine(
                uploadsFolder,
                fileName);

            using var image = await Image.LoadAsync(
                photo.OpenReadStream());

            image.Mutate(x =>
                x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(1200, 1200)
                }));

            await image.SaveAsJpegAsync(
                filePath,
                new JpegEncoder
                {
                    Quality = 80
                });

            return $"/{UploadRootFolder}/{folderName}/{fileName}";
        }

        private void ValidatePhoto(IFormFile? photo)
        {
            if (photo == null || photo.Length == 0)
            {
                throw new InvalidOperationException(
                    "Please upload a photo.");
            }

            var extension = Path
                .GetExtension(photo.FileName)
                .ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    "Only JPG, JPEG and PNG images are allowed.");
            }

            if (photo.Length > MaxPhotoSizeBytes)
            {
                throw new InvalidOperationException(
                    "Image size cannot exceed 5MB.");
            }
        }
    }
}
