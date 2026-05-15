namespace KasiRoomNetwork.Common.ViewModel.PostRoomWizard
{
    public class SelectRoomPhotosStepViewModel
    {
        public List<RoomPhotoSelectionItemViewModel> Photos { get; set; } = new();
    }

    public class RoomPhotoSelectionItemViewModel
    {
        public Guid TempPhotoId { get; set; }

        public string PhotoPath { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public bool UseForRoom { get; set; }
    }
}
