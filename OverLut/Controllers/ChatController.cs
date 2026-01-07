using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OverLut.Models.Repositories;

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

        // GET: /Chat/Index
        // Optional query parameters: userId, channelId
        [HttpGet]
        public IActionResult Index(Guid? userId = null, Guid? channelId = null)
        {
            // If not provided, attempt to infer userId from authenticated user claims
            if (!userId.HasValue && User?.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
                if (idClaim != null && Guid.TryParse(idClaim.Value, out var parsed))
                {
                    userId = parsed;
                }
            }

            // Provide values to the view; view may still use its own defaults if these are null
            ViewData["UserId"] = userId?.ToString();
            ViewData["ChannelId"] = channelId?.ToString();

            return View();
        }
    }
}