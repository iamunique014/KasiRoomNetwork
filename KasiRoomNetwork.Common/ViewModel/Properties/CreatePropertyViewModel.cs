using KasiRoomNetwork.Common.Models;
using KasiRoomNetwork.Common.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace KasiRoomNetwork.Common.ViewModel.Properties
{
    public class CreatePropertyViewModel
    {
        [Required(ErrorMessage = "Property Type is required.")]
        [EnumDataType(typeof(PropertyType), ErrorMessage = "Invalid Property Type.")]
        public PropertyType? PropertyType { get; set; }

        [Range(0, int.MaxValue)]
        public int? TotalRooms { get; set; }

        [StringLength(150)]
        public string? PropertyName { get; set; }

        [Required]
        [StringLength(150)]
        public string Street { get; set; }

        [Required]
        [StringLength(100)]
        public string Province { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(100)]
        public string Suburb { get; set; }

        public List<int> SelectedAmenityIds { get; set; } = new();

        public List<AmenityModel> Amenities { get; set; } = new();
    }
}
