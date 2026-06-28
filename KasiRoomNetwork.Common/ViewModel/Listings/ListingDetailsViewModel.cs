using KasiRoomNetwork.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Listings
{
    public class ListingDetailsViewModel
    {
        public int ListingId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsNew => CreatedAt >= DateTime.Now.AddDays(-30);

        //property
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public int TotalRooms { get; set; }
        public bool PropertyVerified { get; set; }

        // Address
        public string Province { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }
        public string? Street { get; set; }

        // Landlord
        public string LandlordUserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string LandlordBio { get; set; }
        public bool LandlordVerified { get; set; }

        // Photos
        public List<ListingPhotoViewModel> Photos { get; set; } = [];

        //Amenities
        public List<AmenityModel> Amenities { get; set; } = [];
    }
}
