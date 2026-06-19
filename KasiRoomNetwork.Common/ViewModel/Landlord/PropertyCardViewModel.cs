using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Landlord
{
    public class PropertyCardViewModel
    {
        public int PropertyId { get; set; }

        public string PropertyName { get; set; }

        public string PropertyType { get; set; }

        public int? TotalRooms { get; set; }

        public string Street { get; set; }

        public string Province { get; set; }

        public string City { get; set; }

        public string Suburb { get; set; }

        public string PrimaryPhoto { get; set; }

        public int TotalListings { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
