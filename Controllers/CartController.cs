using System;
using System.Collections.Generic;
using System.Linq;
using ECommerceWeb.Data;
using ECommerceWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceWeb.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDataProtector _cartIdentityProtector;

        public CartController(ApplicationDbContext context, IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _cartIdentityProtector = dataProtectionProvider.CreateProtector("ECommerceWeb.CartIdentity.v1");
        }

        /// <summary>
        /// Helper: Lấy UserId từ Session. Tự động tạo Guest User hợp lệ nếu chưa có.
        /// </summary>
        private int GetUserIdFromSession()
        {
            // Ưu tiên 1: User đã đăng nhập thật (Cookie Authentication + Claims)
            // - Đồng bộ với ProfileController/CheckoutController, tránh giỏ hàng
            //   bị gán nhầm sang UserID khác khi user đã đăng nhập.
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
            if (!string.IsNullOrEmpty(userEmail))
            {
                var loggedInUserId = _context.Users
                    .Where(u => u.Email == userEmail)
                    .Select(u => u.UserID)
                    .FirstOrDefault();

                if (loggedInUserId != 0)
                {
                    // Đồng bộ luôn vào Session để các nơi khác lỡ còn đọc Session cũ vẫn ra đúng
                    HttpContext.Session.SetInt32("UserId", loggedInUserId);
                    return loggedInUserId;
                }
            }

            // Ưu tiên 2: Session đã có sẵn (trường hợp Guest đã từng được gán UserID trong phiên này)
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || userId == 0)
            {
                if (TryGetPersistedUserId(out var cookieUserId) &&
                    _context.Users.Any(u => u.UserID == cookieUserId && u.Role == "Guest"))
                {
                    userId = cookieUserId;
                    HttpContext.Session.SetInt32("UserId", userId.Value);
                }
            }

            // Ưu tiên 3 (fallback cuối): Tạo Guest User mới - CHỈ áp dụng cho khách chưa đăng nhập
            if (userId == null || userId == 0)
            {
                string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);

                var guestUser = new User
                {
                    FullName = "Khách vãng lai",
                    Email = $"guest_{uniqueId}@temp.local",
                    PasswordHash = "GUEST_NO_PASSWORD_HASH",
                    Phone = string.Empty,
                    Address = string.Empty,
                    Gender = string.Empty,
                    Role = "Guest",
                    CreatedAt = DateTime.UtcNow,
                };

                _context.Users.Add(guestUser);
                _context.SaveChanges();

                userId = guestUser.UserID;
                HttpContext.Session.SetInt32("UserId", userId.Value);
                HttpContext.Response.Cookies.Append("CartUserId", _cartIdentityProtector.Protect(userId.Value.ToString()), new CookieOptions
                {
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = HttpContext.Request.IsHttps,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });
            }

            return userId.Value;
        }

        private bool TryGetPersistedUserId(out int userId)
        {
            userId = 0;
            var protectedUserId = HttpContext.Request.Cookies["CartUserId"];
            if (string.IsNullOrWhiteSpace(protectedUserId)) return false;

            try
            {
                return int.TryParse(_cartIdentityProtector.Unprotect(protectedUserId), out userId);
            }
            catch
            {
                return false;
            }
        }

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            int userId = GetUserIdFromSession();

            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserID == userId);

            return View(cart);
        }

        // Thêm vào giỏ
        // Thêm using System.Text.Json nếu cần

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            try
            {
                int userId = GetUserIdFromSession();
                quantity = quantity <= 0 ? 1 : quantity;

                var product = _context.Products
                    .FirstOrDefault(p => p.ProductID == productId);

                if (product == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Sản phẩm không tồn tại."
                    });
                }

                var cart = _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefault(c => c.UserID == userId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserID = userId,
                        CreatedAt = DateTime.UtcNow,
                        CartItems = new List<CartItem>()
                    };

                    _context.Carts.Add(cart);
                    _context.SaveChanges();
                }

                // Lấy tất cả dòng của cùng một sản phẩm.
                // Việc này giúp xử lý dữ liệu trùng đã phát sinh trước đó.
                var duplicateItems = cart.CartItems
                    .Where(item => item.ProductID == productId)
                    .OrderBy(item => item.CartItemID)
                    .ToList();

                var cartItem = duplicateItems.FirstOrDefault();
                int currentQtyInCart = duplicateItems.Sum(item => item.Quantity);
                int desiredQty = currentQtyInCart + quantity;

                if (desiredQty > product.Quantity)
                {
                    int canAddMore = Math.Max(0, product.Quantity - currentQtyInCart);

                    int currentCartCount = _context.CartItems
                        .Where(item => item.CartID == cart.CartID)
                        .Sum(item => (int?)item.Quantity) ?? 0;

                    return Json(new
                    {
                        success = false,
                        message = canAddMore > 0
                            ? $"Chỉ còn {product.Quantity} sản phẩm, bạn đã có {currentQtyInCart} trong giỏ. Chỉ có thể thêm tối đa {canAddMore}."
                            : $"Sản phẩm {product.ProductName} đã hết hàng hoặc bạn đã thêm tối đa số lượng còn lại.",
                        cartCount = currentCartCount,
                        availableStock = product.Quantity
                    });
                }

                if (cartItem == null)
                {
                    cartItem = new CartItem
                    {
                        CartID = cart.CartID,
                        ProductID = productId,
                        Quantity = quantity,
                        UnitPrice = product.Price
                    };

                    _context.CartItems.Add(cartItem);
                }
                else
                {
                    // Gộp số lượng vào dòng đầu tiên.
                    cartItem.Quantity = desiredQty;
                    cartItem.UnitPrice = product.Price;

                    // Xóa các dòng trùng còn lại.
                    if (duplicateItems.Count > 1)
                    {
                        _context.CartItems.RemoveRange(duplicateItems.Skip(1));
                    }
                }

                _context.SaveChanges();

                // Luôn lấy tổng số lượng trực tiếp từ database.
                int totalQuantity = _context.CartItems
                    .Where(item => item.CartID == cart.CartID)
                    .Sum(item => (int?)item.Quantity) ?? 0;

                return Json(new
                {
                    success = true,
                    cartCount = totalQuantity,
                    message = $"Đã thêm {product.ProductName} vào giỏ hàng."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể thêm sản phẩm vào giỏ hàng. " + ex.Message
                });
            }
        }

        // Cập nhật số lượng
        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            int userId = GetUserIdFromSession();

            var cartItem = _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefault(ci => ci.CartItemID == cartItemId && ci.Cart.UserID == userId);

            if (cartItem == null)
            {
                return NotFound();
            }

            int requestedQty = quantity <= 0 ? 1 : quantity;

            if (requestedQty > cartItem.Product.Quantity)
            {
                TempData["CartWarning"] = $"{cartItem.Product.ProductName} chỉ còn {cartItem.Product.Quantity} sản phẩm.";
                requestedQty = cartItem.Product.Quantity > 0 ? cartItem.Product.Quantity : 1;
            }

            cartItem.Quantity = requestedQty;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // Xóa 1 item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int cartItemId)
        {
            var item = _context.CartItems
                .FirstOrDefault(x => x.CartItemID == cartItemId);

            if (item == null)
            {
                TempData["CartWarning"] =
                    "Không tìm thấy sản phẩm trong giỏ hàng.";

                return RedirectToAction(nameof(Index));
            }

            _context.CartItems.Remove(item);
            _context.SaveChanges();

            TempData["Success"] =
                "Đã xóa sản phẩm khỏi giỏ hàng.";

            return RedirectToAction(nameof(Index));
        }

        // Xóa nhiều item đã chọn (checkbox) cùng lúc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveSelected(List<int> cartItemIds)
        {
            if (cartItemIds == null || cartItemIds.Count == 0)
            {
                TempData["CartWarning"] =
                    "Bạn chưa chọn sản phẩm nào để xóa.";

                return RedirectToAction(nameof(Index));
            }

            var items = _context.CartItems
                .Where(x => cartItemIds.Contains(x.CartItemID))
                .ToList();

            if (items.Count > 0)
            {
                _context.CartItems.RemoveRange(items);
                _context.SaveChanges();
            }

            TempData["Success"] =
                $"Đã xóa {items.Count} sản phẩm khỏi giỏ hàng.";

            return RedirectToAction(nameof(Index));
        }

        // Xóa toàn bộ giỏ hàng
        public IActionResult Clear()
        {
            int userId = GetUserIdFromSession();

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .Where(c => c.UserID == userId)
                .OrderByDescending(c => c.CartID)
                .FirstOrDefault();

            if (cart != null && cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}