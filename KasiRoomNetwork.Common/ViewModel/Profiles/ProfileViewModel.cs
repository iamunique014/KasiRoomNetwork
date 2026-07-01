using System.ComponentModel.DataAnnotations;

namespace KasiRoomNetwork.Common.ViewModel.Profiles
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^(?:\+27|0)\s?(6|7|8)\d{1}\s?\d{3}\s?\d{4}$", ErrorMessage = "Please enter a valid South African phone number.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province is required.")]
        public string Province { get; set; } = string.Empty;
    }
}
