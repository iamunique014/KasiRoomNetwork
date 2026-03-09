using KasiRoomNetwork.Common.ViewModel.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.Interfaces
{
    public interface IMessagingRepository
    {
        Task<int> CreateConversation(int listingId, string tenantId, string landlordId);
        Task SendMessage(SendMessageViewModel model);
        Task<IEnumerable<MessageViewModel>> GetConversationMessages(int conversationId);
        Task<IEnumerable<ConversationViewModel>> GetInbox(string userId);
        Task MarkConversation(int conversationId, string userId);
    }
}
