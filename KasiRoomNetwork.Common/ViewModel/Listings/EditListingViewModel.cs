using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Listings
{
    public class EditListingViewModel
    {
        [Required]
        public int ListingId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 100000)]
        public decimal Price { get; set; }
        public int AvailableUnits { get; set; }

        public bool IsAvailable { get; set; }

        // Read-only display information
        public string? PropertyName { get; set; }
        public string? City { get; set; }
        public string? Suburb { get; set; }
    }
}
