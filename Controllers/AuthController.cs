using ECommerceWeb.Data;
using ECommerceWeb.Models;
using ECommerceWeb.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

                    // Gộp giỏ hàng Guest (nếu có) vào tài khoản vừa đăng nhập
                    MergeGuestCartIntoUser(user.UserID);

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

        /// <summary>
        /// Gộp giỏ hàng Guest (được lưu qua Session trước khi đăng nhập) vào giỏ hàng
        /// của tài khoản thật vừa đăng nhập thành công. Nếu 2 giỏ có cùng sản phẩm,
        /// cộng dồn số lượng nhưng không vượt quá tồn kho hiện có.
        /// </summary>
        private void MergeGuestCartIntoUser(int realUserId)
        {
            int? guestUserId = HttpContext.Session.GetInt32("UserId");

            // Không có giỏ Guest nào trong session, hoặc session đang trỏ thẳng vào
            // chính user thật (trường hợp đăng nhập lại) -> không cần merge
            if (!guestUserId.HasValue || guestUserId.Value == 0 || guestUserId.Value == realUserId)
            {
                HttpContext.Session.SetInt32("UserId", realUserId);
                return;
            }

            // Chỉ merge nếu UserID trong session thực sự là 1 tài khoản Guest
            // (tránh trường hợp hi hữu session trỏ nhầm sang UserID của người khác)
            bool isActualGuest = _context.Users.Any(u => u.UserID == guestUserId.Value && u.Role == "Guest");
            if (!isActualGuest)
            {
                HttpContext.Session.SetInt32("UserId", realUserId);
                return;
            }

            var guestCart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserID == guestUserId.Value);

            if (guestCart != null && guestCart.CartItems.Any())
            {
                var realCart = _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefault(c => c.UserID == realUserId);

                if (realCart == null)
                {
                    realCart = new Cart
                    {
                        UserID = realUserId,
                        CreatedAt = DateTime.UtcNow,
                        CartItems = new List<CartItem>()
                    };
                    _context.Carts.Add(realCart);
                    _context.SaveChanges(); // để có CartID trước khi gán CartItem
                }

                foreach (var guestItem in guestCart.CartItems.ToList())
                {
                    var product = _context.Products.FirstOrDefault(p => p.ProductID == guestItem.ProductID);
                    if (product == null)
                    {
                        // Sản phẩm không còn tồn tại nữa -> bỏ qua, không merge item này
                        continue;
                    }

                    var existingItem = realCart.CartItems.FirstOrDefault(ci => ci.ProductID == guestItem.ProductID);

                    if (existingItem != null)
                    {
                        int mergedQty = existingItem.Quantity + guestItem.Quantity;
                        existingItem.Quantity = Math.Min(mergedQty, product.Quantity > 0 ? product.Quantity : mergedQty);
                    }
                    else
                    {
                        int qty = Math.Min(guestItem.Quantity, product.Quantity > 0 ? product.Quantity : guestItem.Quantity);
                        if (qty <= 0) continue;

                        realCart.CartItems.Add(new CartItem
                        {
                            CartID = realCart.CartID,
                            ProductID = guestItem.ProductID,
                            Quantity = qty,
                            UnitPrice = guestItem.UnitPrice
                        });
                    }
                }

                // Xóa sạch giỏ Guest sau khi đã merge xong
                _context.CartItems.RemoveRange(guestCart.CartItems);
                _context.SaveChanges();
            }

            // Session giờ trỏ về đúng UserID thật cho các request tiếp theo
            HttpContext.Session.SetInt32("UserId", realUserId);
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
