using KasiRoomNetwork.Common.ViewModel.Properties;
using KasiRoomNetwork.Data.DataAccess;
using KasiRoomNetwork.Data.Interfaces;

namespace KasiRoomNetwork.Data.Repositories
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly ISqlDataAccess _db;

        public PropertyRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        // sp_Landlord_Create_Property
        public async Task<int> CreateProperty(CreatePropertyViewModel model, string landlordUserId)
        {
            var result = await _db.GetData<int, dynamic>("sp_Landlord_Create_Property", new
            {
                LandlordUserId = landlordUserId,
                model.PropertyType,
                model.TotalRooms,
                model.PropertyName,
                model.Street,
                model.Province,
                model.City,
                model.Suburb
            });

            return result.First();
        }
        // sp_Landlord_Get_Properties_By_User
        public async Task<List<LandlordPropertyViewModel>> GetPropertiesByUser(string landlordUserId)
        {
            var result = await _db.GetData<LandlordPropertyViewModel, dynamic>("sp_Landlord_Get_Properties_By_User", new
            {
                LandlordUserId = landlordUserId
            });

            return result.ToList();
        }

        // sp_Add_Property_Photo
        public async Task AddPropertyPhoto(int propertyId, string photoPath, bool isPrimary)
        {
            await _db.SaveData("sp_Property_Add_Photo", new
            {
                propertyId,
                PhotoPath = photoPath,
                IsPrimary = isPrimary
            });
        }

        // sp_Get_Property_By_Id
        public async Task<PropertyDetailsViewModel?> GetPropertyById(int propertyId)
        {
            var result = await _db.GetData<PropertyDetailsViewModel, dynamic>(
                "sp_Property_Get_By_Id",
                new { propertyId });

            return result.FirstOrDefault();
        }

        public async Task<int> GetPropertyPhotoCount(int propertyId)
        {
            var result = await _db.GetData<int, dynamic>("sp_Property_Get_Photo_Count", new
            {
                propertyId
            });

            return result.FirstOrDefault();
        }

        // sp_Get_Property_Photos
        public async Task<List<PropertyPhotoViewModel>> GetPropertyPhotos(int propertyId)
        {
            var result = await _db.GetData<PropertyPhotoViewModel, dynamic>(
                "sp_Property_Get_Photos",
                new { propertyId });

            return result.ToList();
        }
    }
}
