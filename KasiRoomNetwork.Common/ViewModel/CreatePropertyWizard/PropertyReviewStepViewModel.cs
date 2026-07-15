using KasiRoomNetwork.Common.Models;

namespace KasiRoomNetwork.Common.ViewModel.CreatePropertyWizard
{
    public class PropertyReviewStepViewModel
    {
        public BasicPropertyInfoStepViewModel BasicPropertyInfo { get; set; } = new();

        public AddressStepViewModel Address { get; set; } = new();

        public List<AmenityModel> SelectedAmenities { get; set; } = new();

        public List<UploadedPhotoViewModel> PropertyPhotos { get; set; } = new();
    }
}
