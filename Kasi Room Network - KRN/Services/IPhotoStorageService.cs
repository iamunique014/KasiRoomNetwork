using Microsoft.AspNetCore.Http;

namespace Kasi_Room_Network___KRN.Services
{
    public interface IPhotoStorageService
    {
        Task<string> SaveTemporaryPhotoAsync(IFormFile? photo, string landlordUserId);

        void DeleteTemporaryPhoto(string tempRelativePath);
        void DeleteTemporaryPhotos(IEnumerable<string>? tempRelativePaths);
        void DeletePhoto(string relativePath);
        void DeletePhotos(IEnumerable<string>? relativePaths);
        void CleanupExpiredTemporaryPhotos(TimeSpan maxAge);
        Task<string> CopyTemporaryPhotoToPermanentAsync(string tempRelativePath, string permanentFolderName);
        void DeleteTemporaryWizardFolder(string landlordUserId);
        void DeleteLandlordTemporaryPhotos(string landlordUserId);
    }
}
