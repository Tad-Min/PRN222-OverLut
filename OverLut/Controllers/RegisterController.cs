using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OverLut.Models.DTOs;
using OverLut.Models.Repositories;

namespace OverLut.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IUserRepository _userRepository;
        public RegisterController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: RegisterController
        public ActionResult Index()
        {
            return View();
        }

        // GET: RegisterController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: RegisterController/Create
        public ActionResult Create()
        {

            return View();
        }

        // POST: RegisterController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            try
            {
                var username = collection["username"];
                var password = collection["password"];
                var fullname = collection["FullName"];
                var connfirmPassword = collection["confirmPassword"];
                if (password != connfirmPassword)
                {
                    TempData["HandleError"] = "Password và confirm password không khớp";
                    return RedirectToAction(nameof(Index));
                }

                if (await _userRepository.CreateUserAsync(new UserDTO
                {
                    UserName = username,
                    Name = fullname,
                    Password = password,
                }))
                {
                    TempData["SuccessMessage"] = "Chúc mừng! Bạn đã tạo tài khoản thành công.";
                    return RedirectToAction(nameof(Index));
                }
                TempData["SuccessMessage"] = "Tạo tài khoản không thành công";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: RegisterController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: RegisterController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: RegisterController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: RegisterController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
