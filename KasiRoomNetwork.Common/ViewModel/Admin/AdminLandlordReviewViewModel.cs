using KasiRoomNetwork.Common.ViewModel.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Admin
{
    public class AdminLandlordReviewViewModel
    {
        public int LandlordProfileId { get; set; }

        public string LandlordUserId { get; set; }


        public string Email { get; set; }
        public string AccountPhoneNumber { get; set; }

        public DateTime AccountCreatedAt { get; set; }

        // Profile
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }

        public string Province { get; set; }
        public string City { get; set; }

        public string Bio { get; set; }
        public string? WhatsAppNumber { get; set; }

        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }

        // Activity
        public int PropertyCount { get; set; }
        public int ListingCount { get; set; }
        public int VerifiedListingCount { get; set; }
    }
}
