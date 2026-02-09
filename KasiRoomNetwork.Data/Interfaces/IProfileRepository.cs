using KasiRoomNetwork.Common.ViewModel.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.Interfaces
{
    public interface IProfileRepository
    {
        Task<ProfileViewModel?> GetByUserId(string userId);
        Task SaveProfile(ProfileViewModel profile, string userId);
        Task<LandlordPublicProfileViewModel?> GetLandlordPublic(string userId);
    }

}
