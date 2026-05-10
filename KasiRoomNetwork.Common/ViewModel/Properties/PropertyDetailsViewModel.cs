using KasiRoomNetwork.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Properties
{
    public class PropertyDetailsViewModel
    {
        public int PropertyId { get; set; }

        public string PropertyName { get; set; }
        public int TotalRooms { get; set; }

        public string PropertyType { get; set; }
        
        //public bool IsVerified { get; set; }

        public DateTime CreatedAt { get; set; }

        // Address
        public string Province { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }
        public string? Street { get; set; }

        // Landlord
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string LandlordUserId { get; set; }

        // Photos
        public List<PropertyPhotoViewModel> Photos { get; set; } = new();

        //Amenities
        public List<AmenityModel> Amenities { get; set; } = new();
    }
}
