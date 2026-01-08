using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OverLut.Models.DTOs;
using OverLut.Models.Repositories;
using System;
using System.Threading.Tasks;

namespace OverLut.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatRepository chatRepository, ILogger<ChatController> logger)
        {
            _chatRepository = chatRepository;
            _logger = logger;
        }

        // POST: /Chat/CreateChannel
        // Creates a new channel using repository. Returns simple JSON success result.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChannel()
        {
            try
            {
                var ok = await _chatRepository.CreateChannelAsync();
                if (ok) return Ok(new { success = true });
                return BadRequest(new { success = false, error = "Could not create channel" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateChannel failed");
                return StatusCode(500, new { success = false, error = "Server error" });
            }
        }

        // POST: /Chat/AddMember
        // Body: ChannelMemberDTO JSON
        [HttpPost]
        public async Task<IActionResult> AddMember([FromBody] ChannelMemberDTO member)
        {
            if (member == null) return BadRequest(new { success = false, error = "Missing member payload" });

            try
            {
                var ok = await _chatRepository.AddMemberToChannelAsync(member);
                if (ok) return Ok(new { success = true });
                return BadRequest(new { success = false, error = "Could not add member" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddMember failed for ChannelId={ChannelId} UserId={UserId}", member.ChannelId, member.UserId);
                return StatusCode(500, new { success = false, error = "Server error" });
            }
        }

        // GET: /Chat/Channels/{userId}
        // Returns channels the user is member of.
        [HttpGet]
        public async Task<IActionResult> Channels(Guid userId)
        {
            if (userId == Guid.Empty) return BadRequest(new { success = false, error = "Invalid userId" });

            try
            {
                var channels = await _chatRepository.GetAllChannelByUserIDAsync(userId);
                return Ok(channels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Channels lookup failed for user {UserId}", userId);
                return StatusCode(500, new { success = false, error = "Server error" });
            }
        }

        // GET: /Chat/Messages/{channelId}?page=1&page_size=20
        [HttpGet]
        public async Task<IActionResult> Messages(Guid channelId, int page = 1, int page_size = 20)
        {
            if (channelId == Guid.Empty) return BadRequest(new { success = false, error = "Invalid channelId" });

            try
            {
                var messages = await _chatRepository.GetMessagesByChannelIDAsync(channelId, page, page_size);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Messages lookup failed for channel {ChannelId}", channelId);
                return StatusCode(500, new { success = false, error = "Server error" });
            }
        }

        // GET: /Chat/Room?channelId={channelId}&userId={userId}
        // Simple endpoint that can return a view where client will connect to the websocket:
        // connect to: ws(s)://{host}/ws?userId={userId}
        // The view is optional in the repo — this action returns minimal JSON when no view exists.
        [HttpGet]
        public IActionResult Room(Guid channelId, Guid userId)
        {
            if (channelId == Guid.Empty || userId == Guid.Empty)
            {
                return BadRequest(new { success = false, error = "channelId and userId are required" });
            }

            // If you have a Razor view to host the chat UI, return it here:
            // return View(new ChatRoomViewModel { ChannelId = channelId, UserId = userId });
            // For now return minimal info for client to open websocket and load messages.
            return Ok(new
            {
                success = true,
                channelId,
                userId,
                websocketUrl = $"/ws?userId={userId}"
            });
        }
    }
}