using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OverLut.Models.DTOs;
using OverLut.Models.Repositories;
using OverLut.Models.BusinessObjects;

namespace OverLut.Controllers
{
    public class LoginAndRegisterController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly OverLutContext _db;
        private readonly ILogger<LoginAndRegisterController> _logger;

        public LoginAndRegisterController(
            IUserRepository userRepository,
            OverLutContext db,
            ILogger<LoginAndRegisterController> logger)
        {
            _userRepository = userRepository;
            _db = db;
            _logger = logger;
        }

        // GET: /LoginAndRegister
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /LoginAndRegister/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin hợp lệ.";
                return View("Index", model);
            }

            // Normalize RoleId: avoid sending 0 which violates FK constraint.
            int? roleId = model.RoleId;
            if (!roleId.HasValue || roleId.Value <= 0)
            {
                // Try to find a sensible default role (e.g. "User"). If not found, leave null.
                var defaultRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
                if (defaultRole != null)
                {
                    roleId = defaultRole.RoleId;
                }
                else
                {
                    roleId = null;
                }
            }

            var userDto = new UserDTO
            {
                UserName = model.UserName,
                Password = model.Password,
                Name = model.Name,
                Email = model.Email,
                RoleId = roleId
            };

            var created = await _userRepository.CreateUserAsync(userDto);
            if (created)
            {
                TempData["Success"] = "Đăng ký thành công. Bạn có thể đăng nhập ngay.";
                _logger.LogInformation("User registered: {UserName}", model.UserName);
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Đăng ký thất bại. Vui lòng thử lại.";
            _logger.LogWarning("Register failed for user: {UserName}", model.UserName);
            return View("Index", model);
        }

        // POST: /LoginAndRegister/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng cung cấp tên đăng nhập và mật khẩu.";
                return View("Index", model);
            }

            var ok = await _userRepository.LoginUser(model.UserName, model.Password);
            if (ok)
            {
                TempData["Success"] = "Đăng nhập thành công.";
                _logger.LogInformation("User logged in: {UserName}", model.UserName);
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Tên đăng nhập hoặc mật khẩu không đúng.";
            _logger.LogWarning("Login failed for user: {UserName}", model.UserName);
            return View("Index", model);
        }
    }

    public class RegisterViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        // Use nullable so we can detect "not set" from the form.
        public int? RoleId { get; set; } = null;
    }

    public class LoginViewModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
