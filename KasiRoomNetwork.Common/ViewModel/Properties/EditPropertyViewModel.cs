using KasiRoomNetwork.Common.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace KasiRoomNetwork.Common.ViewModel.Properties
{
    public class EditPropertyViewModel
    {
        public int PropertyId { get; set; }

        // Property
        [Required]
        [StringLength(150)]
        [Display(Name = "Property Name")]
        public string PropertyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Property Type is required.")]
        [EnumDataType(typeof(PropertyType), ErrorMessage = "Invalid Property Type.")]
        [Display(Name = "Property Type")]
        public PropertyType? PropertyType { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Total rooms must be at least 1.")]
        [Display(Name = "Total Rooms")]
        public int? TotalRooms { get; set; }

        // Address
        [Required]
        [StringLength(100)]
        public string Province { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Suburb { get; set; }

        [StringLength(150)]
        public string? Street { get; set; }
    }
}
