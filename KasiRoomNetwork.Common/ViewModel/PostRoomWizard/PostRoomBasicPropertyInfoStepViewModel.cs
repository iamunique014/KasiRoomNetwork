using System.ComponentModel.DataAnnotations;

namespace KasiRoomNetwork.Common.ViewModel.PostRoomWizard
{
    public class PostRoomBasicPropertyInfoStepViewModel
    {
        [Required]
        public string PropertyType { get; set; } = string.Empty;

        [StringLength(150)]
        public string? PropertyName { get; set; }

        [Range(0, int.MaxValue)]
        public int? TotalRooms { get; set; }
    }
}
