using System.ComponentModel.DataAnnotations;

namespace KasiRoomNetwork.Common.ViewModel.PostRoomWizard
{
    public class PostRoomDetailsStepViewModel
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(0, 999999)]
        public decimal Price { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
