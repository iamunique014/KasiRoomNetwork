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
        Task<ProfilePageViewModel?> GetByUserId(string userId);
        //Task<bool> Exists(string userId);
        Task<bool> IsComplete(string userId);
        Task SaveProfile(ProfilePageViewModel profile, string userId);
        Task SaveLandlordProfile(ProfilePageViewModel profile, string userId);
        //Task<LandlordPublicProfileViewModel?> GetLandlordPublic(string userId);
    }

}
