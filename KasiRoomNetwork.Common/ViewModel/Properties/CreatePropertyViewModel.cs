using System.ComponentModel.DataAnnotations;

namespace KasiRoomNetwork.Common.ViewModel.Properties
{
    public class CreatePropertyViewModel
    {
        [Required]
        [StringLength(150)]
        public string PropertyType { get; set; }

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
    }
}
