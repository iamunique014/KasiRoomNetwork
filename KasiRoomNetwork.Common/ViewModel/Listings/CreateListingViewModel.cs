using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Listings
{
    public class CreateListingViewModel
    {
        // Address
        [Required]
        public string Province { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Suburb { get; set; }

        public string? Street { get; set; }

        // Listing
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0, 100000)]
        public decimal Price { get; set; }
    }
}
