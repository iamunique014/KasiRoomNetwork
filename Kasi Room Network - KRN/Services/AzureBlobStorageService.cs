using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Kasi_Room_Network___KRN.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Kasi_Room_Network___KRN.Services
{
    public class AzureBlobStorageService : IPhotoStorageService
    {
        private const long MaxPhotoSizeBytes = 5 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = {".jpg", ".jpeg", ".png"};
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AzureBlobStorageService(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            var connectionString = _configuration.GetConnectionString("AzureStorage");
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        private string GetContainerName(ImageCategory category)
        {
            return category switch
            {
                ImageCategory.Listing => "listing-images",
                ImageCategory.Property => "property-images",
                ImageCategory.WizardTemp => "wizard-temp-images",
                _ => throw new InvalidOperationException("Unsupported image category.")
            };
        }

        private string GetBlobName(string relativePath)
        {
            // Remove leading slash if present
            if (relativePath.StartsWith('/'))
            {
                relativePath = relativePath.Substring(1);
            }
            // Replace wwwroot/uploads or wwwroot/uploads-test with empty string
            if (relativePath.StartsWith("uploads/"))
            {
                relativePath = relativePath.Substring("uploads/".Length);
            }
            else if (relativePath.StartsWith("uploads-test/"))
            {
                relativePath = relativePath.Substring("uploads-test/".Length);
            }
            return relativePath;
        }

        public async Task<string> SaveTemporaryPhotoAsync(IFormFile? photo, string landlordUserId)
        {
            ValidatePhoto(photo);
            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            var blobName = $"wizard-temp/{landlordUserId}/{Guid.NewGuid()}{extension}";
            var containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(ImageCategory.WizardTemp));
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
            var blobClient = containerClient.GetBlobClient(blobName);
            using (var stream = photo.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }
            return blobClient.Uri.ToString();
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

            // Extract blob name from URL
            var uri = new Uri(tempRelativePath);
            var blobNameWithContainer = uri.Segments.Skip(uri.Segments.Length - 2).Aggregate((a, b) => a + b);
            var parts = blobNameWithContainer.Split('/');
            var originalBlobName = parts[1]; // e.g., {landlordUserId}/{Guid.NewGuid()}.jpg

            ImageCategory category = permanentFolderName.ToLowerInvariant() switch
            {
                "listings" => ImageCategory.Listing,
                "properties" => ImageCategory.Property,
                _ => throw new InvalidOperationException("Unsupported permanent folder name.")
            };

            var sourceContainerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(ImageCategory.WizardTemp));
            var sourceBlobClient = sourceContainerClient.GetBlobClient($"wizard-temp/{originalBlobName}");

            if (!await sourceBlobClient.ExistsAsync())
            {
                throw new InvalidOperationException("One of your uploaded photos could not be found. Please upload it again.");
            }

            var destinationContainerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(category));
            await destinationContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var newBlobName = $"{permanentFolderName}/{Guid.NewGuid()}{Path.GetExtension(originalBlobName)}";
            var destinationBlobClient = destinationContainerClient.GetBlobClient(newBlobName);

            await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);

            // Delete the temporary blob after copying
            await sourceBlobClient.DeleteIfExistsAsync();

            return destinationBlobClient.Uri.ToString();
        }

        public void DeleteTemporaryWizardFolder(string landlordUserId)
        {
            // In Azure Blob Storage, there are no actual folders, so we delete blobs with the specified prefix.
            var containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(ImageCategory.WizardTemp));
            var prefix = $"wizard-temp/{landlordUserId}/";
            var blobs = containerClient.GetBlobs(BlobTraits.None, BlobStates.None, prefix, CancellationToken.None);
            foreach (var blob in blobs)
            {
                containerClient.DeleteBlobIfExists(blob.Name);
            }
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

            var uri = new Uri(relativePath);
            var containerName = uri.Segments[1].TrimEnd('/');
            var blobName = string.Join("", uri.Segments.Skip(2));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            containerClient.DeleteBlobIfExists(blobName);
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
            DeleteTemporaryWizardFolder(landlordUserId);
        }

        public void CleanupExpiredTemporaryPhotos(TimeSpan maxAge)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(ImageCategory.WizardTemp));
            if (!containerClient.Exists())
            {
                return;
            }

            foreach (var blobItem in containerClient.GetBlobs())
            {
                if (blobItem.Properties.CreatedOn.HasValue && (DateTimeOffset.UtcNow - blobItem.Properties.CreatedOn.Value) > maxAge)
                {
                    containerClient.DeleteBlobIfExists(blobItem.Name);
                }
            }
        }

        public async Task<string> SaveOptimizedImageAsync(IFormFile photo, ImageCategory category)
        {
            ValidatePhoto(photo);

            var containerName = GetContainerName(category);
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileName = $"{Guid.NewGuid()}.jpg";
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var memoryStream = new MemoryStream())
            {
                using (var image = await Image.LoadAsync(photo.OpenReadStream()))
                {
                    image.Mutate(x =>
                        x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(1200, 1200)
                        }));

                    await image.SaveAsJpegAsync(
                        memoryStream,
                        new JpegEncoder
                        {
                            Quality = 80
                        });
                }
                memoryStream.Position = 0;
                await blobClient.UploadAsync(memoryStream, true);
            }

            return blobClient.Uri.ToString();
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
