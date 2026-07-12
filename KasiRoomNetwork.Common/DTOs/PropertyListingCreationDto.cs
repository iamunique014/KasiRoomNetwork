using KasiRoomNetwork.Common.Models.Enums;
using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Common.ViewModel.Properties;

namespace KasiRoomNetwork.Common.DTOs
{
    public class PropertyListingCreationDto
    {
        // Property Details
        public string LandlordUserId { get; set; }
        public PropertyType? PropertyType { get; set; }
        public int TotalRooms { get; set; }
        public string PropertyName { get; set; }

        // Address Details
        public string Street { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }

        // Amenities
        public List<int> AmenityIds { get; set; } = new List<int>();

        // Photos
        public List<string> TemporaryPhotoPaths { get; set; } = new List<string>();
        public string PrimaryPhotoPath { get; set; }

        // Listing Details (Optional, for wizard flow)
        public string ListingTitle { get; set; }
        public string ListingDescription { get; set; }
        public int AvailableUnits { get; set; }
        public decimal Price { get; set; }
        public List<string> SelectedListingPhotoPaths { get; set; } = new List<string>();
    }
}
