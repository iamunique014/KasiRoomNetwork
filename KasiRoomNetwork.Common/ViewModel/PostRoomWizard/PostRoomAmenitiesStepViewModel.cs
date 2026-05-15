using KasiRoomNetwork.Common.Models;

namespace KasiRoomNetwork.Common.ViewModel.PostRoomWizard
{
    public class PostRoomAmenitiesStepViewModel
    {
        public List<int> SelectedAmenityIds { get; set; } = new();

        public List<AmenityModel> Amenities { get; set; } = new();
    }
}
