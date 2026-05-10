using KasiRoomNetwork.Common.ViewModel.Landlord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.Interfaces
{
    public interface ILandlordRepository
    {
        Task<IEnumerable<LandlordListingViewModel>> GetAllLandlordListings(string landlordId);
        Task<IEnumerable<PropertyCardViewModel>> GetAllPropertiesByLandlord(string landlordId);
    }
}
