using KasiRoomNetwork.Common.DTOs;

namespace Kasi_Room_Network___KRN.Services
{
    public interface IPostRoomWizardService
    {
        Task<(int propertyId, int listingId)> CreatePropertyAndListingAsync(PostRoomWizardDto dto);
    }
}