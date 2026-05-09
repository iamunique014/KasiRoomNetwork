using KasiRoomNetwork.Common.ViewModel.Properties;

namespace KasiRoomNetwork.Data.Interfaces
{
    public interface IPropertyRepository
    {
        Task<int> CreateProperty(CreatePropertyViewModel model, string landlordUserId);
        Task<List<LandlordPropertyViewModel>> GetPropertiesByUser(string landlordUserId);
        // sp_Add_Property_Photo
        Task AddPropertyPhoto(int propertyId, string photoPath, bool isPrimary);

        // sp_Get_Property_By_Id
        Task<PropertyDetailsViewModel?> GetPropertyById(int propertyId);

        // sp_Get_Property_Photos
        Task<List<PropertyPhotoViewModel>> GetPropertyPhotos(int propertyId);

        //SP_Get_Property_Photo_Count_By_Property
        Task<int> GetPropertyPhotoCount(int propertyId);
    }
}
