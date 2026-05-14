using System.ComponentModel.DataAnnotations;

namespace KasiRoomNetwork.Common.ViewModel.PostRoomWizard
{
    public class PostRoomAddressStepViewModel
    {
        [Required]
        [StringLength(100)]
        public string Province { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Suburb { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Street { get; set; } = string.Empty;
    }
}
