using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OverLut.Models.Repositories;
using System.Security.Claims;

//Claims: UserId

namespace OverLut.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserRepository _userRepository;
        public LoginController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }
        // POST: Login/Login
        [HttpPost]
        public async Task<IActionResult> Login(IFormCollection collection)
        {
            try
            {
                var username = collection["username"];
                var password = collection["password"];

                if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                {
                    TempData["ErrorHandle"] = "Username hoặc password không được để trống";
                    return RedirectToAction(nameof(Index));
                }
                var userguid = await _userRepository.LoginUser(username, password);
                if (String.IsNullOrEmpty(userguid.ToString()))
                {
                    TempData["ErrorHandle"] = "không tìm thấy thông tin người dùng";
                    return RedirectToAction(nameof(Index));
                }

                var claims = new List<Claim>
                {
                    new Claim("UserId", userguid.ToString()??"")
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return RedirectToAction("Index", "ChatPage");
            }
            catch
            {
                return View();
            }
        }
    }
}
