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

        public async Task<EditPropertyViewModel?> GetPropertyForEditAsync(int propertyId, string landlordId)
        {
            var results = await _db.GetData<EditPropertyViewModel, dynamic>(
                "sp_Property_Get_For_Edit",
                new
                {
                    PropertyId = propertyId,
                    LandlordId = landlordId
                });

            return results.FirstOrDefault();
        }

        public async Task UpdatePropertyAsync(EditPropertyViewModel model, string landlordId)
        {
            await _db.SaveData(
                "sp_Property_Update",
                new
                {
                    model.PropertyId,
                    LandlordId = landlordId,

                    model.PropertyName,
                    model.PropertyType,
                    model.TotalRooms,

                    model.Province,
                    model.City,
                    model.Suburb,
                    model.Street
                });
        }

        // sp_Property_Delete
        public async Task DeletePropertyAsync(int propertyId, string landlordId)
        {
            await _db.SaveData(
                "sp_Property_Delete",
                new
                {
                    PropertyId = propertyId,
                    LandlordId = landlordId
                });
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
        public async Task<bool> AddPropertyPhoto(int propertyId, string dbPath, bool isPrimary, string landlordUserId)
        {
            var result = await _db.GetData<int, dynamic>("sp_Property_Add_Photo", new
            {
                propertyId,
                PhotoPath = dbPath,
                IsPrimary = isPrimary,
                LandlordUserId = landlordUserId
            });
            return result.FirstOrDefault() == 1;
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

            return result
                .OrderByDescending(photo => photo.IsPrimary)
                .ThenBy(photo => photo.PhotoId)
                .ToList();
        }

        public async Task DeletePropertyPhoto(int photoId, int propertyId)
        {
            await _db.SaveData("sp_PropertyPhoto_Delete", new
            {
                PhotoId = photoId,
                PropertyId = propertyId
            });
        }

        public async Task SetPrimaryPropertyPhoto(int propertyId, int photoId)
        {
            await _db.SaveData("sp_PropertyPhoto_Set_Primary", new
            {
                PropertyId = propertyId,
                PhotoId = photoId
            });
        }
    }
}
