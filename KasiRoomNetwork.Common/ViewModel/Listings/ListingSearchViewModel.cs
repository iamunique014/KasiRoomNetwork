using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Listings
{
    public class ListingSearchViewModel
    {
        // Filters
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Suburb { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Results
        public List<ListingSearchResultViewModel> Results { get; set; } = new();
    }
}
