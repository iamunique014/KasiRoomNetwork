using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Profiles
{
    public class LandlordPublicProfileViewModel
    {
        public string FullName { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Bio { get; set; }
        public bool IsVerified { get; set; }
    }
}
