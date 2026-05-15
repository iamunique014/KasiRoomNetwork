namespace KasiRoomNetwork.Common.ViewModel.PostRoomWizard
{
    public class PostRoomWizardStateViewModel
    {
        public string LandlordUserId { get; set; } = string.Empty;

        public PostRoomBasicPropertyInfoStepViewModel BasicPropertyInfo { get; set; } = new();

        public PostRoomAddressStepViewModel Address { get; set; } = new();

        public List<int> SelectedAmenityIds { get; set; } = new();

        public PostRoomDetailsStepViewModel RoomDetails { get; set; } = new();

        public List<PostRoomUploadedPhotoViewModel> UploadedPhotos { get; set; } = new();

        public DateTime StartedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
