using System.Collections.Generic;

namespace KasiRoomNetwork.Common.ViewModel.Listings
{
    public class ManageListingPhotosViewModel
    {
        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<ListingPhotoViewModel> Photos { get; set; } = [];
    }
}
