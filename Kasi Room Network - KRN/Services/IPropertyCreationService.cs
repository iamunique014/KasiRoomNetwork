using KasiRoomNetwork.Common.DTOs;

namespace KasiRoomNetwork.KRN.Services
{
    public interface IPropertyCreationService
    {
        Task<(int propertyId, int? listingId)> CreatePropertyAndListingAsync(PropertyListingCreationDto dto);
    }
}
