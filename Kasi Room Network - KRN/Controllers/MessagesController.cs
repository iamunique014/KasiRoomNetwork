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
        private readonly IListingRepository _listingRepository;
        private readonly ILogger<MessagesController> _logger;
        public MessagesController(UserManager<ApplicationUser> userManager, IProfileRepository profileRepository, IMessagingRepository messagingRepository, IListingRepository listingRepository, ILogger<MessagesController> logger)
        {
            _userManager = userManager;
            _profileRepository = profileRepository;
            _messagingRepository = messagingRepository;
            _listingRepository = listingRepository;
            _logger = logger;
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

            try
            {
                var conversationId =
                    await _messagingRepository.CreateConversation(
                        listingId,
                        userId,
                        landlordId);

                var alreadyLogged =
                    await _messagingRepository.HasInAppContactLog(
                        listingId,
                        userId);

                if (!alreadyLogged)
                {
                    await _messagingRepository.CreateContactLog(
                        listingId,
                        userId,
                        "InApp",
                        conversationId);
                }

                _logger.LogInformation("User {UserId} started conversation {ConversationId} for Listing {ListingId}.",
                    userId,
                    conversationId,
                    listingId
                );

                return RedirectToAction("Conversation", new { conversationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "User {UserId} unable to start conversation for Listing {ListingId}",
                    userId,
                    listingId
                );
                TempData["Error"] = "Unable to start conversation at this time. Please try again later.";
                return RedirectToAction("ListingDetails", "Listing", new { listingId });
            }
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

            var header =
                await _messagingRepository
                    .GetConversationHeader(
                        conversationId,
                        userId
                    );

            var messages =
                await _messagingRepository
                    .GetConversationMessages(
                        conversationId,
                        userId);

            await _messagingRepository
                .MarkConversationRead(conversationId, userId);

            ViewBag.ConversationId = conversationId;
            ViewBag.ConversationHeader = header;
            ViewBag.ReturnUrl = Request.Headers["Referer"].ToString();
            return View(messages);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(SendMessageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(
                    nameof(Conversation),
                    new { conversationId = model.ConversationId });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var hasAccess = await _messagingRepository
            .UserOwnsConversation(model.ConversationId, userId);

            if (!hasAccess)
            {
                return NotFound();
            }

            model.MessageText = model.MessageText?.Trim();

            if (string.IsNullOrWhiteSpace(model.MessageText))
            {
                ModelState.AddModelError(
                    nameof(model.MessageText),
                    "Message cannot be empty.");
            }

            model.SenderId = userId;

            try
            {
                await _messagingRepository.SendMessage(model);
                _logger.LogInformation("User {UserId} Sent a Message in conversation {ConversationId}.",
                    userId,
                    model.ConversationId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "User {UserId} Could not send a Message in conversation {ConversationId}.",
                    userId, 
                    model.ConversationId
                );

                TempData["Error"] = "Unable to send message at this time. Please try again later.";
            }

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

        [AllowAnonymous]
        public async Task<IActionResult> WhatsAppContact(int listingId)
        {
            var userId = _userManager.GetUserId(User);

            var listing = await _listingRepository.GetListingById(listingId);

            if (listing == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(listing.WhatsAppNumber))
            {
                return RedirectToAction(
                    "ListingDetails",
                    "Listing",
                    new { listingId });
            }

            try
            {
                await _messagingRepository.CreateContactLog(
                    listingId,
                    userId,
                    "WhatsApp",
                    null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                "Listing {ListingId} Contact log could not be created",
                listingId
                );
                // We don't want to block the redirect if logging fails, 
                // but we also don't want to crash.
            }

            var number = listing.WhatsAppNumber
                .Replace(" ", "")
                .Replace("-", "");

            if (number.StartsWith("0"))
            {
                number = "27" + number.Substring(1);
            }

            if (number.StartsWith("+"))
            {
                number = number.Substring(1);
            }

            var message =
                $"Hi, I'm interested in your listing '{listing.Title}' Listed on KRN - KasiRoomNetwork. Is it still available?";

            var encodedMessage = Uri.EscapeDataString(message);

            var whatsappUrl =
                $"https://wa.me/{number}?text={encodedMessage}";

            return Redirect(whatsappUrl);
        }
    }
}
