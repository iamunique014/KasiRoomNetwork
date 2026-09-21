using KasiRoomNetwork.Common.Models;

namespace KasiRoomNetwork.Common.ViewModel.CreatePropertyWizard
{
    public class AmenitiesStepViewModel
    {
        public List<int> SelectedAmenityIds { get; set; } = new();

        public List<AmenityModel> Amenities { get; set; } = new();
    }
}
