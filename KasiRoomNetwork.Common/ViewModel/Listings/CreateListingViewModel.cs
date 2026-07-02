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
        public string? PropertyName { get; set; }
        public string? City { get; set; }
        public string? Suburb { get; set; }

        // Listing
        [Required]
        public int PropertyId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please enter an amount between 500 and 15000")]
        [Range(typeof(decimal), "500", "15000")]
        public decimal Price { get; set; }

        public int AvailableUnits { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
