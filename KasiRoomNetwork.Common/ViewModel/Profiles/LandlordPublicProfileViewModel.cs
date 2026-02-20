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

        [Required]
        [StringLength(500)]
        public string Bio { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
