using KasiRoomNetwork.Common.ViewModel.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Admin
{
    public class AdminPropertyReviewViewModel
    {
        // Property
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public int TotalRooms { get; set; }
        public DateTime CreatedAt { get; set; }

        // Property Verification
        public bool IsVerified { get; set; }
        public string? VerificationNotes { get; set; }
        public DateTime? VerifiedOn { get; set; }

        // Landlord
        public string LandlordUserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Bio { get; set; }
        public bool LandlordVerified { get; set; }

        // Address
        public string Province { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }
        public string Street { get; set; }

        // Stats
        public int TotalPhotos { get; set; }
        public int TotalListings { get; set; }

        // Photos
        public List<PropertyPhotoViewModel> Photos { get; set; } = new();
    }
}
