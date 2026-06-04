using KasiRoomNetwork.Common.Models;
using KasiRoomNetwork.Common.ViewModel.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.Interfaces
{
    public interface IAmenityRepository
    {
        Task<IEnumerable<AmenityModel>> GetAllAmenities();

        Task AddPropertyAmenity(int propertyId, int amenityId);

        Task RemovePropertyAmenity(int propertyId, int amenityId);

        Task<IEnumerable<AmenityModel>> GetAmenitiesByPropertyId(int propertyId);
        Task<EditPropertyAmenitiesViewModel?>GetPropertyAmenitiesForEditAsync(int propertyId, 
            string landlordId);
        Task UpdatePropertyAmenitiesAsync(int propertyId, List<int> amenityIds, string landlordId);
    }
}
