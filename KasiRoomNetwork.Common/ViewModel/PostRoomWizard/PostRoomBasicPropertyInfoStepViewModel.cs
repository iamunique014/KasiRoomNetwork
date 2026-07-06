using KasiRoomNetwork.Common.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace KasiRoomNetwork.Common.ViewModel.PostRoomWizard
{
    public class PostRoomBasicPropertyInfoStepViewModel
    {
        [Required(ErrorMessage = "Property Type is required.")]
        [EnumDataType(typeof(PropertyType), ErrorMessage = "Invalid Property Type.")]
        public PropertyType? PropertyType { get; set; }

        [StringLength(150)]
        public string? PropertyName { get; set; }

        [Range(0, int.MaxValue)]
        public int? TotalRooms { get; set; }
    }
}
