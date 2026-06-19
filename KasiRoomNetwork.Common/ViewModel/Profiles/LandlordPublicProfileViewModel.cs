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

        [StringLength(20)]
        [Display(Name = "WhatsApp Number")]
        public string? WhatsAppNumber { get; set; }

        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
