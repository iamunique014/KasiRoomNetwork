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
            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
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

            var uri = new Uri(tempRelativePath);
            var blobNameWithContainer = uri.AbsolutePath.TrimStart('/');
            var parts = blobNameWithContainer.Split('/');
            
            if (parts.Length < 2)
            {
                 throw new InvalidOperationException("Invalid temporary photo path.");
            }
            
            var sourceContainerName = parts[0];
            var sourceBlobName = string.Join("/", parts.Skip(1));

            ImageCategory category = permanentFolderName.ToLowerInvariant() switch
            {
                "listings" => ImageCategory.Listing,
                "properties" => ImageCategory.Property,
                _ => throw new InvalidOperationException("Unsupported permanent folder name.")
            };

            var sourceContainerClient = _blobServiceClient.GetBlobContainerClient(sourceContainerName);
            var sourceBlobClient = sourceContainerClient.GetBlobClient(sourceBlobName);

            if (!await sourceBlobClient.ExistsAsync())
            {
                throw new InvalidOperationException("One of your uploaded photos could not be found. Please upload it again.");
            }

            var destinationContainerName = GetContainerName(category);
            var destinationContainerClient = _blobServiceClient.GetBlobContainerClient(destinationContainerName);
            await destinationContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var newBlobName = $"{permanentFolderName}/{Guid.NewGuid()}{Path.GetExtension(sourceBlobName)}";
            var destinationBlobClient = destinationContainerClient.GetBlobClient(newBlobName);

            await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);
            
            // BUG IDENTIFIED: Deleting the source blob here causes subsequent copies of the same 
            // temporary photo (e.g., when used for both Property and Listing) to fail because 
            // the source no longer exists.
            // await sourceBlobClient.DeleteIfExistsAsync();

            return destinationBlobClient.Uri.ToString();
        }

        public void DeleteTemporaryWizardFolder(string landlordUserId)
        {
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

            try
            {
                var uri = new Uri(relativePath);
                var pathParts = uri.AbsolutePath.TrimStart('/').Split('/');
                if (pathParts.Length < 2) return;

                var containerName = pathParts[0];
                var blobName = string.Join("/", pathParts.Skip(1));

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                containerClient.DeleteBlobIfExists(blobName);
            }
            catch
            {
                // Ignore invalid URIs or deletion failures
            }
        }

        public void DeletePhotos(IEnumerable<string>? relativePaths)
        {
            if (relativePaths == null) return;
            foreach (var path in relativePaths)
            {
                DeletePhoto(path);
            }
        }

        public void DeleteLandlordTemporaryPhotos(string landlordUserId)
        {
            DeleteTemporaryWizardFolder(landlordUserId);
        }

        public void CleanupExpiredTemporaryPhotos(TimeSpan maxAge)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(ImageCategory.WizardTemp));
            if (!containerClient.Exists()) return;

            foreach (var blobItem in containerClient.GetBlobs(BlobTraits.None, BlobStates.None, null, CancellationToken.None))
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

            var folderName = category switch
            {
                ImageCategory.Listing => "listings",
                ImageCategory.Property => "properties",
                _ => throw new InvalidOperationException("Unsupported image category.")
            };

            var fileName = $"{Guid.NewGuid()}.jpg";
            var blobName = $"{folderName}/{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

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

                    await image.SaveAsJpegAsync(memoryStream, new JpegEncoder { Quality = 80 });
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
                throw new InvalidOperationException("Please upload a photo.");
            }

            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Only JPG, JPEG and PNG images are allowed.");
            }

            if (photo.Length > MaxPhotoSizeBytes)
            {
                throw new InvalidOperationException("Image size cannot exceed 5MB.");
            }
        }
    }
}
