using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Profiles
{
    public class ProfilePageViewModel
    {
        public ProfileViewModel UserProfile { get; set; } = new();
        public LandlordPublicProfileViewModel? LandlordProfile { get; set; }
    }
}
