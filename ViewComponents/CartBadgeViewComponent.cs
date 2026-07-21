using System.Security.Claims;
using ECommerceWeb.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWeb.ViewComponents
{
    public class CartBadgeViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IDataProtector _cartIdentityProtector;

        public CartBadgeViewComponent(
            ApplicationDbContext context,
            IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _cartIdentityProtector =
                dataProtectionProvider.CreateProtector("ECommerceWeb.CartIdentity.v1");
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int? userId = await GetCurrentUserIdAsync();

            if (!userId.HasValue || userId.Value <= 0)
            {
                return View(0);
            }

            // Chỉ lấy đúng giỏ hàng hiện tại của user.
            // Dùng CartID mới nhất để đồng bộ với CartController.
            int? cartId = await _context.Carts
                .Where(c => c.UserID == userId.Value)
                .OrderByDescending(c => c.CartID)
                .Select(c => (int?)c.CartID)
                .FirstOrDefaultAsync();

            if (!cartId.HasValue)
            {
                return View(0);
            }

            int cartCount = await _context.CartItems
                .Where(ci => ci.CartID == cartId.Value)
                .SumAsync(ci => (int?)ci.Quantity) ?? 0;

            return View(cartCount);
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            // 1. Nếu đã đăng nhập, luôn ưu tiên UserID theo tài khoản đăng nhập.
            string? userEmail =
                UserClaimsPrincipal.FindFirstValue(ClaimTypes.Email)
                ?? UserClaimsPrincipal.Identity?.Name;

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                int loggedInUserId = await _context.Users
                    .Where(u => u.Email == userEmail)
                    .Select(u => u.UserID)
                    .FirstOrDefaultAsync();

                if (loggedInUserId > 0)
                {
                    HttpContext.Session.SetInt32("UserId", loggedInUserId);
                    return loggedInUserId;
                }
            }

            // 2. Nếu là khách, lấy UserID trong Session.
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
            {
                return sessionUserId.Value;
            }

            // 3. Nếu Session hết hạn, khôi phục Guest UserID từ cookie.
            if (TryGetPersistedUserId(out int cookieUserId))
            {
                bool guestExists = await _context.Users
                    .AnyAsync(u => u.UserID == cookieUserId && u.Role == "Guest");

                if (guestExists)
                {
                    HttpContext.Session.SetInt32("UserId", cookieUserId);
                    return cookieUserId;
                }
            }

            return null;
        }

        private bool TryGetPersistedUserId(out int userId)
        {
            userId = 0;

            string? protectedUserId =
                HttpContext.Request.Cookies["CartUserId"];

            if (string.IsNullOrWhiteSpace(protectedUserId))
            {
                return false;
            }

            try
            {
                string unprotectedUserId =
                    _cartIdentityProtector.Unprotect(protectedUserId);

                return int.TryParse(unprotectedUserId, out userId);
            }
            catch
            {
                return false;
            }
        }
    }
}