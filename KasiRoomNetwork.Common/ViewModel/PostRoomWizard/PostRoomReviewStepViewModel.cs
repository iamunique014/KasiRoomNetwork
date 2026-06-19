using KasiRoomNetwork.Common.Models;

namespace KasiRoomNetwork.Common.ViewModel.PostRoomWizard
{
    public class PostRoomReviewStepViewModel
    {
        public PostRoomBasicPropertyInfoStepViewModel BasicPropertyInfo { get; set; } = new();

        public PostRoomAddressStepViewModel Address { get; set; } = new();

        public List<AmenityModel> SelectedAmenities { get; set; } = new();

        public PostRoomDetailsStepViewModel RoomDetails { get; set; } = new();

        public List<PostRoomUploadedPhotoViewModel> PropertyPhotos { get; set; } = new();

        public List<PostRoomUploadedPhotoViewModel> RoomPhotos { get; set; } = new();
    }
}
