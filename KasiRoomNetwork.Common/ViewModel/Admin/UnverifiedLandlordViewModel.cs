using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Admin
{
    public class UnverifiedLandlordViewModel
    {
        public int LandlordProfileId { get; set; }
        public string LandlordUserId { get; set; }
        public string FullName { get; set; }
        public string Suburb { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Bio { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsVerified { get; set; }
        public bool VerificationStatus { get; set; }
        public int MyProperty { get; set; }
        public DateTime VerifiedAt { get; set; }

    }
}
