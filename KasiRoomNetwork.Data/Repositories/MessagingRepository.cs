using KasiRoomNetwork.Common.ViewModel.Messaging;
using KasiRoomNetwork.Data.DataAccess;
using KasiRoomNetwork.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.Repositories
{
    public class MessagingRepository : IMessagingRepository
    {
        private readonly ISqlDataAccess _db;

        public MessagingRepository(ISqlDataAccess db)
        {
            _db = db;
        }

       
        public async Task<int> CreateConversation(int listingId, string tenantId, string landlordId)
        {
            var result = await _db.GetData<int, dynamic>(
                "sp_Messaging_Create_Conversation",
                new 
                { 
                    listingId, tenantId, landlordId 
                } 
               
            );

            return result.FirstOrDefault();
        }

        public async Task<ConversationViewModel?> GetConversationHeader(
            int conversationId,
            string userId)
        {
            var result =
                await _db.GetData<ConversationViewModel, dynamic>(
                    "sp_Messaging_Get_Conversation_Header",
                    new
                    {
                        ConversationId = conversationId,
                        UserId = userId
                    });

            return result.FirstOrDefault();
        }

        public async Task SendMessage(SendMessageViewModel model)
        {
            await _db.SaveData("sp_Messaging_Send_Message", new
            {
                model.ConversationId,
                model.SenderId,
                model.MessageText
            });
        }

        public async Task<IEnumerable<MessageViewModel>> GetConversationMessages(int conversationId, string userId)
        {
            return await _db.GetData<MessageViewModel, dynamic>(
                "sp_Messaging_Get_Conversation_Messages",
                new { ConversationId = conversationId, UserId = userId });
        }

        public async Task<IEnumerable<ConversationViewModel>> GetInbox(string userId)
        {
            return await _db.GetData<ConversationViewModel, dynamic>(
               "sp_Messaging_Get_Inbox",
               new { UserId = userId });
        }

        public async Task MarkConversationRead(int conversationId, string userId)
        {
            await _db.SaveData("sp_Messaging_Mark_Conversation_Read", new { 
                    ConversationId = conversationId, UserId = userId

            });
        }

        public async Task<int> GetUnreadCount(string userId)
        {
            var result = await _db.GetData<int, dynamic>(
                "sp_Messaging_Get_Unread_Count",
                new { UserId = userId }
            );

            return result.FirstOrDefault();
        }

        public async Task<bool> UserOwnsConversation(int conversationId, string userId)
        {
            var result = await _db.GetData<int, dynamic>(
                "sp_Messaging_ValidateConversation",
                new { 
                    ConversationId = conversationId,
                    UserId = userId }
            );

            return result.FirstOrDefault() == 1;
        }
        public async Task CreateContactLog(
            int listingId,
            string userId,
            string contactMethod,
            int? conversationId = null)
        {
            await _db.SaveData("sp_ListingContactLog_Create", new
            {
                ListingId = listingId,
                UserId = userId,
                ContactMethod = contactMethod,
                ConversationId = conversationId
            });
        }
        public async Task<bool> HasInAppContactLog(int listingId, string userId)
        {
            var result = await _db.GetData<int, dynamic>(
                "sp_ListingContactLog_Exists",
                new
                {
                    ListingId = listingId,
                    UserId = userId,
                    ContactMethod = "InApp"
                });

            return result.FirstOrDefault() > 0;
        }
    }
}
