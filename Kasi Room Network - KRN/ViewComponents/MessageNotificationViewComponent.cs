using KasiRoomNetwork.Common.ViewModel.Messaging;
using KasiRoomNetwork.Data.Domain.Models;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kasi_Room_Network___KRN.ViewComponents
{
    public class MessageNotificationViewComponent : ViewComponent
    {
        private readonly IMessagingRepository _messagingRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageNotificationViewComponent(
            IMessagingRepository messagingRepository,
            UserManager<ApplicationUser> userManager)
        {
            _messagingRepository = messagingRepository;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new MessageNotificationViewModel();

            if (User.Identity != null &&
                User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(HttpContext.User);

                model.UnreadCount =
                    await _messagingRepository.GetUnreadCount(userId);
            }

            return View(model);
        }
    }
}
