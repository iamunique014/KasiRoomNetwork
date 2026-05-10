using KasiRoomNetwork.Common.Models;
using KasiRoomNetwork.Data.DataAccess;
using KasiRoomNetwork.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.Repositories
{
    public class AmenityRepository : IAmenityRepository
    {
        private readonly ISqlDataAccess _db;

        public AmenityRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<IEnumerable<AmenityModel>> GetAllAmenities()
        {
            return await _db.GetData<AmenityModel, dynamic>(
                "sp_Amenity_Get_All",
                new { });
        }

        public async Task AddPropertyAmenity(int propertyId, int amenityId)
        {
            await _db.SaveData(
                "sp_PropertyAmenity_Add",
                new
                {
                    PropertyId = propertyId,
                    AmenityId = amenityId
                });
        }

        public async Task RemovePropertyAmenity(int propertyId, int amenityId)
        {
            await _db.SaveData(
                "sp_PropertyAmenity_Remove",
                new
                {
                    PropertyId = propertyId,
                    AmenityId = amenityId
                });
        }

        public async Task<IEnumerable<AmenityModel>> GetAmenitiesByPropertyId(int propertyId)
        {
            return await _db.GetData<AmenityModel, dynamic>(
                "sp_PropertyAmenity_Get_By_PropertyId",
                new
                {
                    PropertyId = propertyId
                });
        }
    }
}
