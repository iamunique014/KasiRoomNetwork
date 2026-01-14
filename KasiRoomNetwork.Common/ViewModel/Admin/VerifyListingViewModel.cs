using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Admin
{
    public class VerifyListingViewModel
    {
        public int ListingId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        [StringLength(255)]
        public string? Notes { get; set; }
    }
}
