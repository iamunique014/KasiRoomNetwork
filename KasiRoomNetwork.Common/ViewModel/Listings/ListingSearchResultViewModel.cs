using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Listings
{
    public class ListingSearchResultViewModel
    {
        public int ListingId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }

        public string Province { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }

        public string? PrimaryPhoto { get; set; }
    }
}
