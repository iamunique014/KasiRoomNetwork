namespace KasiRoomNetwork.Common.ViewModel.PostRoomWizard
{
    public class PostRoomUploadedPhotoViewModel
    {
        public string TempPhotoId { get; set; } = string.Empty;

        public string TempRelativePath { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public bool IsSelectedForRoom { get; set; } = false;
    }
}
