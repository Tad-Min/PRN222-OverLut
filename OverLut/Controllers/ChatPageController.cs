using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OverLut.Models.DTOs;
using OverLut.Models.Repositories;
using System.Threading.Tasks;

namespace OverLut.Controllers
{
    public class ChatPageController : Controller
    {
        private readonly IChatRepository _chatRepository;
        public ChatPageController(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }
        // GET: ChatPageController
        [Authorize]
        public async Task<ActionResult> Index()
        {
            if (Guid.TryParse(User.FindFirst("UserId")?.Value, out Guid userGuid))
            {
                var lsChannel = await _chatRepository.GetAllChannelByUserIDAsync(userGuid);
                return View(lsChannel);
            }
            else
            {
                return Unauthorized();
            }
        }
        // GET: ChatPage/logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Login");
        }

        // GET: /ChatPage/GetMessage/
        [HttpGet]
        [Route("ChatPage/GetMessage/{id}")]
        public async Task<IActionResult> GetMessage(Guid id)
        {
            var msg = await _chatRepository.GetMessagesByChannelIDAsync(id);
            return Json(msg);
        }

        [HttpPost]
        [Route("ChatPage/SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (request == null) return BadRequest();

            if (Guid.TryParse(User.FindFirst("UserId")?.Value, out Guid userGuid))
            {
                var messageDto = new MessageDTO
                {
                    UserId = userGuid,
                    Content = request.Content,
                    ChannelId = request.ChannelId,
                    MessageType = 0,
                };

                var success = await _chatRepository.SendMessageAsync(messageDto);

                if (success)
                {
                    // return saved message back to the caller (client can use it as confirmation)
                    return Ok(messageDto);
                }
                else
                {
                    return BadRequest("Failed to send message (maybe not a channel member).");
                }
            }
            return Unauthorized();
        }

        public class SendMessageRequest
        {
            public Guid ChannelId { get; set; }
            public string Content { get; set; }
        }


    }
}
