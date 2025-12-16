using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Listings
{
    public class ListingPhotoViewModel
    {
        public int PhotoId { get; set; }
        public string PhotoPath { get; set; }
        public bool IsPrimary { get; set; }
    }
}
