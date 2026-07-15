namespace KasiRoomNetwork.Common.ViewModel.CreatePropertyWizard
{
    public class PropertyWizardStateViewModel
    {
        public string LandlordUserId { get; set; } = string.Empty;

        public BasicPropertyInfoStepViewModel BasicPropertyInfo { get; set; } = new();

        public AddressStepViewModel Address { get; set; } = new();

        public List<int> SelectedAmenityIds { get; set; } = new();

        public List<UploadedPhotoViewModel> UploadedPhotos { get; set; } = new();

        public DateTime StartedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}