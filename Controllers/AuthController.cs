using ECommerceWeb.Data;
using ECommerceWeb.Models;
using ECommerceWeb.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        /* ========================
           LUỒNG ĐĂNG KÝ (REGISTER)
           ======================== */
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }
                if (string.IsNullOrWhiteSpace(model.Gender))
                {
                    model.Gender = "";
                }
                if (string.IsNullOrWhiteSpace(model.Address))
                {
                    model.Address = "";
                }
                if (string.IsNullOrWhiteSpace(model.Phone))
                {
                    model.Phone = "";
                }
                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Address = model.Address,
                    Phone = model.Phone,
                    Gender = model.Gender,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "Customer"
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";

                return RedirectToAction("Login");
            }

            return View(model);
        }

        /* ========================
           LUỒNG ĐĂNG NHẬP (LOGIN)
           ======================== */
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.SingleOrDefault(u => u.Email == model.Email);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal,
                        authProperties);

                    TempData["Success"] = $"Chào mừng trở lại, {user.FullName}!";

                    // if (user.Role == "Admin")
                    // {
                    //     return RedirectToAction("Index", "Admin");
                    // }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            }

            return View(model);
        }

        /* ========================
           LUỒNG ĐĂNG XUẤT (LOGOUT)
           ======================== */
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }
    }
}
