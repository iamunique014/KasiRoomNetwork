using KasiRoomNetwork.Common.ViewModel.Home;
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
    public class HomeRepository : IHomeRepository
    {
        private readonly ISqlDataAccess _db;

        public HomeRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<IEnumerable<FeaturedPropertyCardViewModel>>
            GetFeaturedPropertiesAsync()
        {
            return await _db.GetData<
                FeaturedPropertyCardViewModel,
                dynamic>(
                    "sp_Home_Get_Featured_Properties",
                    new { });
        }
    }
}