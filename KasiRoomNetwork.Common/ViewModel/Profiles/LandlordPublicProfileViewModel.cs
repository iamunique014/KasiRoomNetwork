using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Profiles
{
    public class LandlordPublicProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Bio { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^(?:\+27|0)\s?(6|7|8)\d{1}\s?\d{3}\s?\d{4}$", ErrorMessage = "Please enter a valid South African phone number.")]
        public string? WhatsAppNumber { get; set; }

        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerificationNotes { get; set; }
    }
}
