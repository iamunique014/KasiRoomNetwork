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
    }
}
