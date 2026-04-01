using KasiRoomNetwork.Common.ViewModel.Properties;

namespace KasiRoomNetwork.Data.Interfaces
{
    public interface IPropertyRepository
    {
        Task<int> CreateProperty(CreatePropertyViewModel model, string landlordUserId);
    }
}
