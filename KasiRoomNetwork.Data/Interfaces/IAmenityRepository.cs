using KasiRoomNetwork.Common.Models;
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
    }
}
