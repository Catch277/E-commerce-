using ECommerceWeb.Data;
using ECommerceWeb.Models;
using ECommerceWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceWeb.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userOrders = await _context.Orders
                .Where(o => o.UserID == user.UserID && o.OrderStatus != "Đã hủy")
                .ToListAsync();

            var totalOrders = userOrders.Count;
            var totalSpent = userOrders.Sum(o => o.TotalAmount);

            string tier = totalSpent >= 5000000 ? "VIP" : "Thành viên";

            var model = new ProfileViewModel
            {
                ProfileId = user.UserID,
                FullName = user.FullName,
                Address = user.Address ?? "Chưa cập nhật",
                Phone = user.Phone ?? "Chưa cập nhật",
                Email = user.Email,
                TotalOrders = totalOrders,
                TotalSpent = (decimal)totalSpent,
                Role = user.Role,
                MemberTier = tier,
            };

            return View(model);
        }

        private const int PageSize = 5;

        public async Task<IActionResult> OrderHistory(string status, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var query = _context.Orders
                .Where(o => o.UserID == user.UserID)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            var effectiveFromDate = fromDate ?? new DateTime(2026, 1, 1);
            var effectiveToDate = toDate ?? DateTime.Now;

            query = query.Where(o => o.OrderDate.Date >= effectiveFromDate.Date
                                   && o.OrderDate.Date <= effectiveToDate.Date);

            var totalOrders = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalOrders / (double)PageSize);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var model = new OrderHistoryViewModel
            {
                FullName = user.FullName,
                CurrentFilter = status,
                FromDate = effectiveFromDate,
                ToDate = effectiveToDate,
                CurrentPage = page,
                TotalPages = totalPages == 0 ? 1 : totalPages,
                Orders = orders.Select(o => new OrderHistoryItemViewModel
                {
                    OrderID = o.OrderID,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus,
                    TotalProducts = o.OrderDetails.Sum(od => od.Quantity),
                    FirstProductName = o.OrderDetails.FirstOrDefault()?.Product?.ProductName,
                    ThumbnailUrl = o.OrderDetails.FirstOrDefault()?.Product?.ImageUrl
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Membership()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;

            var user = await _context.Users
                .Include(u => u.Tier)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null) return RedirectToAction("Login", "Auth");

            var allTiers = await _context.MembershipTiers
                .Include(t => t.Benefits)
                .OrderBy(t => t.MinPoints)
                .ToListAsync();

            var nextTier = allTiers.FirstOrDefault(t => t.MinPoints > user.Points);

            var model = new MembershipVM
            {
                CurrentPoints = user.Points,
                CurrentTier = user.Tier,
                NextTier = nextTier,
                AllTiers = allTiers
            };

            return View(model);
        }

        public async Task<IActionResult> AccountInfo()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;

            var user = await _context.Users
                .Include(u => u.Tier)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userOrders = await _context.Orders
                .Where(o => o.UserID == user.UserID && o.OrderStatus != "Đã hủy")
                .ToListAsync();

            var totalOrders = userOrders.Count;
            var totalSpent = userOrders.Sum(o => o.TotalAmount);

            var nextTier = await _context.MembershipTiers
                .Where(t => t.MinPoints > user.Tier.MinPoints)
                .OrderBy(t => t.MinPoints)
                .FirstOrDefaultAsync();

            const decimal amountPerPoint = 10000m;
            var pointsToNext = nextTier == null ? 0 : Math.Max(0, nextTier.MinPoints - user.Points);
            var amountToNext = pointsToNext * amountPerPoint;

            var myVouchers = await _context.UserVouchers
                .Where(uv => uv.UserID == user.UserID && uv.UsedInOrderID == null)
                .Include(uv => uv.Voucher)
                .OrderByDescending(uv => uv.RedeemedAt)
                .Select(uv => new VoucherItemViewModel
                {
                    VoucherID = uv.Voucher.VoucherID,
                    Title = uv.Voucher.Title,
                    Description = uv.Voucher.Description,
                    Code = uv.Voucher.Code,
                    ExpiryDate = uv.Voucher.ExpiryDate
                })
                .ToListAsync();

            var model = new AccountInfoViewModel
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Phone = user.Phone,
                PhoneMasked = MaskPhone(user.Phone),
                Email = user.Email,
                Gender = user.Gender ?? "Chưa cập nhật",
                DateOfBirth = user.DateOfBirth,
                DefaultAddress = user.Address ?? "Chưa cập nhật",

                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                CurrentTierCode = user.Tier?.TierName ?? "Thành viên",
                CurrentTierName = user.Tier?.TierName ?? "Thành viên",
                IsStudentVerified = false,
                TierRefreshDate = new DateTime(DateTime.Now.Year + 1, 1, 1),
                AmountToNextTier = amountToNext,
                NextTierCode = nextTier?.TierName ?? "",

                Addresses = new List<AddressItemViewModel>
                {
                    new AddressItemViewModel
                    {
                        Label = "Home",
                        TypeTag = "Nhà",
                        IsDefault = true,
                        ReceiverName = user.FullName,
                        Phone = user.Phone,
                        FullAddress = user.Address ?? "Chưa cập nhật"
                    }
                },
                Vouchers = myVouchers,

                LinkedAccounts = new List<LinkedAccountItemViewModel>
                {
                    new LinkedAccountItemViewModel { ProviderName = "Google", IconClass = "fa-brands fa-google", IconColorHex = "#DB4437", IsLinked = false },
                    new LinkedAccountItemViewModel { ProviderName = "Zalo", IconClass = "fa-solid fa-comment", IconColorHex = "#0068FF", IsLinked = false }
                },

                PasswordLastUpdated = user.PasswordUpdatedAt
            };

            return View(model);
        }

        // ==== ACTION BỊ THIẾU #1: Cập nhật thông tin cá nhân ====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            if (!ModelState.IsValid)
            {
                TempData["ProfileError"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                return RedirectToAction("AccountInfo");
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            user.FullName = model.FullName;
            user.Phone = model.Phone;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;
            user.Address = model.Address;

            await _context.SaveChangesAsync();

            TempData["ProfileSuccess"] = "Cập nhật thông tin cá nhân thành công.";
            return RedirectToAction("AccountInfo");
        }

        // ==== ACTION BỊ THIẾU #2: Đổi mật khẩu ====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["PasswordError"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                return RedirectToAction("AccountInfo");
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            bool isCurrentPasswordCorrect = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);
            if (!isCurrentPasswordCorrect)
            {
                TempData["PasswordError"] = "Mật khẩu hiện tại không đúng.";
                return RedirectToAction("AccountInfo");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.PasswordUpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["PasswordSuccess"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("AccountInfo");
        }

        // ==== ACTION BỊ THIẾU #3: Áp dụng mã giảm giá ====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyVoucher(string voucherCode)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(voucherCode))
            {
                TempData["VoucherError"] = "Vui lòng nhập mã giảm giá.";
                return RedirectToAction("AccountInfo");
            }

            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == voucherCode.Trim().ToUpper() && v.IsActive);

            if (voucher == null)
            {
                TempData["VoucherError"] = "Mã giảm giá không tồn tại hoặc đã ngừng áp dụng.";
                return RedirectToAction("AccountInfo");
            }

            if (voucher.ExpiryDate < DateTime.Now)
            {
                TempData["VoucherError"] = "Mã giảm giá đã hết hạn.";
                return RedirectToAction("AccountInfo");
            }

            if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit.Value)
            {
                TempData["VoucherError"] = "Mã giảm giá đã hết lượt sử dụng.";
                return RedirectToAction("AccountInfo");
            }

            var alreadyRedeemed = await _context.UserVouchers
                .AnyAsync(uv => uv.UserID == user.UserID && uv.VoucherID == voucher.VoucherID);

            if (alreadyRedeemed)
            {
                TempData["VoucherError"] = "Bạn đã đổi mã giảm giá này rồi.";
                return RedirectToAction("AccountInfo");
            }

            _context.UserVouchers.Add(new UserVoucher
            {
                UserID = user.UserID,
                VoucherID = voucher.VoucherID,
                RedeemedAt = DateTime.Now
            });

            voucher.UsedCount += 1;

            await _context.SaveChangesAsync();

            TempData["VoucherSuccess"] = $"Áp dụng thành công mã \"{voucher.Code}\"!";
            return RedirectToAction("AccountInfo");
        }

        private string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 5) return phone;
            return phone.Substring(0, 3) + new string('*', phone.Length - 5) + phone.Substring(phone.Length - 2);
        }
    }
}