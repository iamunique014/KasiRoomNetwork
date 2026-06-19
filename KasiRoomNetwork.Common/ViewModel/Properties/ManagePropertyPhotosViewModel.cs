using System.Collections.Generic;

namespace KasiRoomNetwork.Common.ViewModel.Properties
{
    public class ManagePropertyPhotosViewModel
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public List<PropertyPhotoViewModel> Photos { get; set; } = new();
    }
}
