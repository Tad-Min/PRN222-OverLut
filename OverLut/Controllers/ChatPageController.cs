using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            if(Guid.TryParse(User.FindFirst("UserId")?.Value, out Guid userGuid))
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


    }
}
