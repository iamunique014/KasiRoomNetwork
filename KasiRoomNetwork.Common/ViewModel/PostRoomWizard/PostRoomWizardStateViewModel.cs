namespace KasiRoomNetwork.Common.ViewModel.PostRoomWizard
{
    public class PostRoomWizardStateViewModel
    {
        public string LandlordUserId { get; set; } = string.Empty;

        public PostRoomBasicPropertyInfoStepViewModel BasicPropertyInfo { get; set; } = new();

        public PostRoomAddressStepViewModel Address { get; set; } = new();

        public DateTime StartedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
