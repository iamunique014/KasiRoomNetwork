using KasiRoomNetwork.Common.DTOs;

namespace Kasi_Room_Network___KRN.Services
{
    public interface ICreatePropertyService
    {
        Task<int> CreatePropertyAsync(CreatePropertyWizardDto dto);
    }
}