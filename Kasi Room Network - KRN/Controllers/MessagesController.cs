using KasiRoomNetwork.Common.ViewModel.Messaging;
using KasiRoomNetwork.Data.Domain.Models;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kasi_Room_Network___KRN.Controllers
{
    public class MessagesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProfileRepository _profileRepository;
        private readonly IMessagingRepository _messagingRepository;
        public MessagesController(UserManager<ApplicationUser> userManager, IProfileRepository profileRepository, IMessagingRepository messagingRepository)
        {
            _userManager = userManager;
            _profileRepository = profileRepository;
            _messagingRepository = messagingRepository;
        }

        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> ContactLandlord(int listingId, string landlordId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var hasCompleteProfile = await _profileRepository.IsComplete(userId);

            if (!hasCompleteProfile)
            {
                TempData["ProfilePrompt"] = "Before contacting a landlord, complete your profile so they can trust who is reaching out.";
                return RedirectToAction(
                    "MyProfile",
                    "Profile",
                    new { returnUrl = Url.Action("ContactLandlord", new { listingId, landlordId }) }
                );
            }

            // if profile exists continue
            return RedirectToAction("StartConversation", new { listingId, landlordId });
        }

        public async Task<IActionResult> StartConversation(int listingId, string landlordId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var conversationId = await _messagingRepository.CreateConversation(listingId, userId, landlordId);
            return RedirectToAction("Conversation", new { conversationId});
        }

        public async Task<IActionResult> Conversation(int conversationId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var hasAccess = await _messagingRepository
                .UserOwnsConversation(conversationId, userId);

            if (!hasAccess)
            {
                return NotFound();
            }

            var messages = await _messagingRepository
                .GetConversationMessages(conversationId, userId);


            await _messagingRepository
                .MarkConversationRead(conversationId, userId);

            ViewBag.ConversationId = conversationId;

            return View(messages);
        }
        [HttpPost]
        public async Task<IActionResult> Send(SendMessageViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            model.SenderId = userId;

            await _messagingRepository.SendMessage(model);

            return RedirectToAction("Conversation", new { conversationId = model.ConversationId });
        }
        public async Task<IActionResult> Inbox()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var inbox = await _messagingRepository.GetInbox(userId);

            return View(inbox);
        }
    }
}
