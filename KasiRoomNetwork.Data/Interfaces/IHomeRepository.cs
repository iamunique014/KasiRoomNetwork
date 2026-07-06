using KasiRoomNetwork.Common.ViewModel.Home;
using KasiRoomNetwork.Common.ViewModel.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.Interfaces
{
    public interface IHomeRepository
    {
        Task<IEnumerable<FeaturedPropertyCardViewModel>>
            GetFeaturedPropertiesAsync();
    }
}