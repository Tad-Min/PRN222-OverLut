using Microsoft.AspNetCore.Mvc;

namespace OverLut.Controllers
{
    public class LoginAndRegisterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
