using KasiRoomNetwork.Common.ViewModel.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Admin
{
    public class AdminListingReviewViewModel
    {
        public int ListingId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }

        public string LandlordName { get; set; }
        public string PhoneNumber { get; set; }

        public string Province { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }
        public string Street { get; set; }

        // Property
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public int? TotalRooms { get; set; }
        public bool PropertyVerified { get; set; }

        public List<ListingPhotoViewModel> Photos { get; set; } = new();
    }
}
