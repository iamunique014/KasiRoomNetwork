using KasiRoomNetwork.Common.ViewModel.Landlord;
using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Data.DataAccess;
using KasiRoomNetwork.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.Repositories
{
    public class LandlordRepository : ILandlordRepository
    {
        private readonly ISqlDataAccess _db;

        public LandlordRepository(ISqlDataAccess db)
        {
            _db = db;
        }
        // ===============================
        // All Landlord Listing
        // ===============================
        public async Task<IEnumerable<LandlordListingViewModel>> GetAllLandlordListings(string landlordId)
        {
            var result = await _db.GetData<LandlordListingViewModel, dynamic>(
                "sp_Landlord_Get_All_Listings",
                new { LandlordId = landlordId }
            );

            return result.ToList();
        }
    }
}
