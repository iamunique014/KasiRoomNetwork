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
        Task<bool> UserOwnsConversation(int conversationId, string userId);
        Task SendMessage(SendMessageViewModel model);
        Task<IEnumerable<MessageViewModel>> GetConversationMessages(int conversationId, string userId);
        Task<IEnumerable<ConversationViewModel>> GetInbox(string userId);
        Task MarkConversationRead(int conversationId, string userId);
        Task<int> GetUnreadCount(string userId);
    }
}
