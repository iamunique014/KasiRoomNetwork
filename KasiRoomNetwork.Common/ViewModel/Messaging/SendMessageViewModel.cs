using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Messaging
{
    public class SendMessageViewModel
    {
        public int ConversationId { get; set; }
        public string SenderId { get; set; }
        public string MessageText { get; set; }
    }
}
