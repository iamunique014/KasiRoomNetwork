using Microsoft.AspNetCore.Http;

namespace Kasi_Room_Network___KRN.Services
{
    public interface IPhotoStorageService
    {
        Task<string> SaveTemporaryPhotoAsync(IFormFile? photo, string landlordUserId);

        void DeleteTemporaryPhoto(string tempRelativePath);
    }
}
