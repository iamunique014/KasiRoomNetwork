using KasiRoomNetwork.Common.Models;
using KasiRoomNetwork.Common.ViewModel.Properties;
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

        public async Task AddPropertyAmenity(int propertyId, int amenityId, string landlordUserId)
        {
            await _db.SaveData(
                "sp_PropertyAmenity_Add",
                new
                {
                    PropertyId = propertyId,
                    AmenityId = amenityId,
                    LandlordUserId = landlordUserId
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

        public async Task<EditPropertyAmenitiesViewModel?>GetPropertyAmenitiesForEditAsync(
            int propertyId,
            string landlordId)
        {
            var amenities = await _db.GetData<AmenitySelectionViewModel, dynamic>(
                "sp_PropertyAmenities_Get_For_Edit",
                new
                {
                    PropertyId = propertyId,
                    LandlordId = landlordId
                });

            return new EditPropertyAmenitiesViewModel
            {
                PropertyId = propertyId,
                Amenities = amenities.ToList()
            };
        }

        public async Task UpdatePropertyAmenitiesAsync(
            int propertyId,
            List<int> amenityIds,
            string landlordId)
        {
            await _db.SaveData(
                "sp_PropertyAmenities_Clear",
                new
                {
                    PropertyId = propertyId,
                    LandlordId = landlordId
                });

            foreach (var amenityId in amenityIds)
            {
                await _db.SaveData(
                    "sp_PropertyAmenity_Add",
                    new
                    {
                        PropertyId = propertyId,
                        AmenityId = amenityId,
                        LandlordId = landlordId
                    });
            }
        }
    }
}
