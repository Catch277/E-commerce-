using ECommerceWeb.Data;
using ECommerceWeb.Models;
using ECommerceWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using ECommerceWeb.Services;
using System.Security.Claims;

namespace ECommerceWeb.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly IGhnService _ghnService;
        private readonly IDataProtector _cartIdentityProtector;

        public CheckoutController(
            ApplicationDbContext context,
            EmailService emailService,
            IGhnService ghnService,
            IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _emailService = emailService;
            _ghnService = ghnService;
            _cartIdentityProtector = dataProtectionProvider.CreateProtector("ECommerceWeb.CartIdentity.v1");
        }

        /// <summary>
        /// Helper: Lấy UserId từ Session — đồng bộ với cách CartController đang làm,
        /// tránh việc tin tưởng userId truyền qua query string (không an toàn, dễ lệch giỏ hàng).
        /// </summary>
        private int GetUserIdFromSession()
        {
            // Ưu tiên 1: User đã đăng nhập thật (Cookie Authentication + Claims)
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
            if (!string.IsNullOrEmpty(userEmail))
            {
                var loggedInUserId = _context.Users
                    .Where(u => u.Email == userEmail)
                    .Select(u => u.UserID)
                    .FirstOrDefault();

                if (loggedInUserId != 0)
                {
                    return loggedInUserId;
                }
            }

            // Ưu tiên 2 (fallback): Session cũ - giữ lại phòng trường hợp có luồng nào
            // khác trong project vẫn đang set Session["UserId"] theo cách cũ
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value != 0)
            {
                return sessionUserId.Value;
            }

            // Ưu tiên 3 (fallback cuối): Guest identity qua cookie CartUserId
            if (TryGetPersistedUserId(out var cookieUserId) &&
                _context.Users.Any(u => u.UserID == cookieUserId && u.Role == "Guest"))
            {
                HttpContext.Session.SetInt32("UserId", cookieUserId);
                return cookieUserId;
            }

            return 0;
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

        // Hiển thị form đặt hàng
        [HttpGet]
        public IActionResult Index()
        {
            int userId = GetUserIdFromSession();
            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserID == userId);

            var user = _context.Users
                .Where(u => u.UserID == userId)
                .Select(u => new
                {
                    FullName = u.FullName ?? string.Empty,
                    Phone = u.Phone ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Address = u.Address ?? string.Empty
                })
                .FirstOrDefault();

            var cartItems = cart?.CartItems?.ToList() ?? new List<CartItem>();

            // ---- check tồn kho ngay khi load trang, không đợi submit ----
            foreach (var item in cartItems)
            {
                if (item.Quantity > item.Product.Quantity)
                {
                    ModelState.AddModelError("", item.Product.Quantity > 0
                        ? $"Sản phẩm {item.Product.ProductName} chỉ còn {item.Product.Quantity}, giỏ hàng đang chọn {item.Quantity}. Vui lòng quay lại giỏ hàng để điều chỉnh."
                        : $"Sản phẩm {item.Product.ProductName} đã hết hàng. Vui lòng quay lại giỏ hàng để điều chỉnh.");
                }
            }

            decimal subTotal = cartItems.Sum(i => i.Quantity * i.UnitPrice);
            decimal originalTotal = cartItems.Sum(i => i.Quantity * (i.Product.OldPrice ?? i.Product.Price));

            //Lấy danh sách voucher của user
            var availableVouchers = _context.Vouchers
                .Where(v =>
                    v.IsActive &&
                    v.ExpiryDate >= DateTime.Now &&
                    (!v.UsageLimit.HasValue ||
                     v.UsedCount < v.UsageLimit.Value))
                .OrderBy(v => v.ExpiryDate)
                .ToList();

            var model = new CheckoutViewModel
            {
                UserID = userId,
                FullName = user?.FullName ?? "",
                Phone = user?.Phone ?? "",
                Email = user?.Email ?? "",
                ShippingAddress = user?.Address ?? "",
                CartItems = cartItems,
                SubTotal = subTotal,
                OriginalTotal = originalTotal,
                AvailableVouchers = availableVouchers
            };

            return View(model);
        }

        // AJAX: tính phí vận chuyển qua GHN khi người dùng chọn Quận/Huyện
        // TODO: khi có bảng danh mục quận/huyện/phường thật của GHN, đổi tham số
        // districtId/wardCode cho khớp với dữ liệu thật thay vì danh sách mẫu ở view.
        //[HttpGet]
        //public async Task<IActionResult> GetShippingFee(int districtId, string wardCode)
        //{
        //    try
        //    {
        //        int fee = await _ghnService.CalculateFeeAsync(districtId, wardCode);
        //        return Json(new { success = true, fee });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, fee = 0, message = "Không tính được phí ship: " + ex.Message });
        //    }
        //}

        // Xử lý đặt hàng
        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            // Luôn lấy UserID thật từ Session, không tin giá trị hidden field client gửi lên
            model.UserID = GetUserIdFromSession();

            // Không xử lý xuất hóa đơn công ty ở giai đoạn này
            model.WantCompanyInvoice = false;

            if (!ModelState.IsValid)
            {
                return await ReloadCartIntoModel(model);
            }

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserID == model.UserID);

            if (cart == null || !cart.CartItems.Any())
            {
                ModelState.AddModelError("", "Giỏ hàng trống.");
                return await ReloadCartIntoModel(model);
            }

            decimal subTotal = 0;

            // Kiểm tra tồn kho
            foreach (var item in cart.CartItems)
            {
                if (item.Quantity > item.Product.Quantity)
                {
                    ModelState.AddModelError("", $"Sản phẩm {item.Product.ProductName} không đủ số lượng.");
                    return await ReloadCartIntoModel(model);
                }

                subTotal += item.Quantity * item.UnitPrice;
            }

            // ---- Áp mã giảm giá (nếu có) ----
            decimal discountAmount = 0;
            Voucher appliedVoucher = null;

            if (!string.IsNullOrWhiteSpace(model.VoucherCode))
            {
                var code = model.VoucherCode.Trim();
                appliedVoucher = _context.Vouchers.FirstOrDefault(v => v.Code == code);

                if (appliedVoucher == null || !appliedVoucher.IsActive || appliedVoucher.ExpiryDate < DateTime.Now)
                {
                    ModelState.AddModelError(nameof(model.VoucherCode), "Mã giảm giá không hợp lệ hoặc đã hết hạn.");
                    return await ReloadCartIntoModel(model);
                }

                if (appliedVoucher.UsageLimit.HasValue && appliedVoucher.UsedCount >= appliedVoucher.UsageLimit.Value)
                {
                    ModelState.AddModelError(nameof(model.VoucherCode), "Mã giảm giá đã hết lượt sử dụng.");
                    return await ReloadCartIntoModel(model);
                }

                if (subTotal < appliedVoucher.MinOrderValue)
                {
                    ModelState.AddModelError(nameof(model.VoucherCode),
                        $"Đơn hàng cần tối thiểu {appliedVoucher.MinOrderValue:N0}đ để dùng mã này.");
                    return await ReloadCartIntoModel(model);
                }

                discountAmount = appliedVoucher.DiscountType == "Percent"
                    ? subTotal * appliedVoucher.DiscountValue / 100
                    : appliedVoucher.DiscountValue;

                if (appliedVoucher.MaxDiscountAmount.HasValue && discountAmount > appliedVoucher.MaxDiscountAmount.Value)
                {
                    discountAmount = appliedVoucher.MaxDiscountAmount.Value;
                }
            }

            // ---- Tính lại phí ship phía server ----
            decimal shippingFee = 0;
            if (ShippingPolicy.IsFreeShipProvince(model.ProvinceName))
            {
                shippingFee = 0;
            }
            else if (model.DistrictId.HasValue)
            {
                try
                {
                    shippingFee = await _ghnService.CalculateFeeAsync(model.DistrictId.Value, model.WardCode ?? string.Empty);
                }
                catch
                {
                    shippingFee = 0;
                }
            }

            decimal total = subTotal - discountAmount + shippingFee;
            if (total < 0) total = 0;

            string fullAddress = string.Join(", ", new[] { model.ShippingAddress, model.DistrictName, model.ProvinceName }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

            // Tạo Order
            bool isCod = string.Equals(
             model.PaymentMethod,
             "COD",
             StringComparison.OrdinalIgnoreCase);

            Order order = new Order
            {
                UserID = model.UserID,
                ShippingAddress = fullAddress,
                CustomerName = model.FullName,
                CustomerPhone = model.Phone,
                CustomerEmail = model.Email,
                PaymentMethod = model.PaymentMethod,
                Note = model.Note?.Trim() ?? string.Empty,
                ShippingFee = shippingFee,
                DiscountAmount = discountAmount,
                VoucherID = appliedVoucher?.VoucherID,
                TotalAmount = total,
                OrderDate = DateTime.Now,

                // COD có thể chuyển sang chờ xác nhận ngay.
                // VNPay/MoMo phải chờ thanh toán thành công.
                OrderStatus = isCod
                    ? "Chờ xác nhận"
                    : "Chờ thanh toán"
            };

            _context.Orders.Add(order);

            // Cần SaveChanges để database tạo OrderID
            await _context.SaveChangesAsync();

            // Tạo trước một bản ghi Payment cho đơn hàng
            Payment payment = new Payment
            {
                OrderID = order.OrderID,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = isCod
                    ? "Thanh toán khi nhận hàng"
                    : "Chưa thanh toán",

                PaymentDate = null,
                TransactionCode = string.Empty
            };

            _context.Payments.Add(payment);

            // Tạo OrderDetail + cập nhật tồn kho
            // Luôn tạo OrderDetail để lưu lại sản phẩm của đơn hàng.
            // Chỉ trừ kho ngay nếu là COD.
            // VNPay/MoMo sẽ trừ kho sau khi thanh toán thành công.
            foreach (var item in cart.CartItems)
            {
                OrderDetail detail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };

                _context.OrderDetails.Add(detail);

                if (isCod)
                {
                    item.Product.Quantity -= item.Quantity;
                }
            }

            // Ghi nhận lượt dùng voucher
            // COD hoàn tất đặt hàng ngay nên tăng lượt dùng voucher.
            // Thanh toán online chỉ tăng sau khi giao dịch thành công.
            if (isCod && appliedVoucher != null)
            {
                appliedVoucher.UsedCount += 1;
            }

            // Xóa giỏ hàng
            // COD hoàn tất đặt hàng ngay nên được xóa giỏ.
            // VNPay/MoMo phải giữ giỏ nếu khách hủy hoặc thanh toán lỗi.
            if (isCod)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
            }

            await _context.SaveChangesAsync();

            // Gửi email xác nhận
            if (isCod)
            {
                var userId = _context.Users
                    .Where(u => u.UserID == model.UserID)
                    .Select(u => (int?)u.UserID)
                    .FirstOrDefault();

                if (userId.HasValue)
                {
                    string subject = "Xác nhận đơn hàng";

                    string body = $@"
                        Xin chào {model.FullName},

                        Bạn đã đặt hàng thành công.

                        Mã đơn hàng: {order.OrderID}

                        Tổng tiền: {order.TotalAmount:N0} VNĐ

                        Trạng thái: {order.OrderStatus}

                        Cảm ơn bạn đã mua hàng.
                        ";

                    try
                    {
                        _emailService.Send(model.Email, subject, body);

                        EmailLog log = new EmailLog
                        {
                            UserID = userId.Value,
                            OrderID = order.OrderID,
                            EmailSubject = subject,
                            EmailContent = body,
                            SentAt = DateTime.Now,
                            EmailStatus = "Đã gửi"
                        };

                        _context.EmailLogs.Add(log);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        EmailLog log = new EmailLog
                        {
                            UserID = userId.Value,
                            OrderID = order.OrderID,
                            EmailSubject = subject,
                            EmailContent = ex.Message,
                            SentAt = DateTime.Now,
                            EmailStatus = "Gửi thất bại"
                        };

                        _context.EmailLogs.Add(log);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // Điều hướng theo phương thức thanh toán
            if (string.Equals(
                model.PaymentMethod,
                "VNPay",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(
                    "CreatePaymentUrl",
                    "Payment",
                    new { orderId = order.OrderID });
            }

            if (string.Equals(
                model.PaymentMethod,
                "MoMo",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(
                    "CreateMoMoPayment",
                    "Payment",
                    new { orderId = order.OrderID });
            }

            // COD
            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction(
                "Success",
                new { orderId = order.OrderID });
        }

        // Trang xác nhận đặt hàng thành công (áp dụng cho COD)
        [HttpGet]
        public IActionResult Success(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Helper: nạp lại giỏ hàng vào model khi cần render lại view do lỗi validate
        private async Task<IActionResult> ReloadCartIntoModel(CheckoutViewModel model)
        {
            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserID == model.UserID);

            var cartItems = cart?.CartItems?.ToList() ?? new List<CartItem>();
            model.CartItems = cartItems;
            model.SubTotal = cartItems.Sum(i => i.Quantity * i.UnitPrice);
            model.OriginalTotal = cartItems.Sum(i => i.Quantity * (i.Product.OldPrice ?? i.Product.Price));
            model.AvailableVouchers = await _context.Vouchers
                .Where(v => v.IsActive && v.ExpiryDate >= DateTime.Now &&
                    (!v.UsageLimit.HasValue || v.UsedCount < v.UsageLimit.Value))
                .OrderBy(v => v.ExpiryDate)
                .ToListAsync();
            return await Task.FromResult(View(model));
        }
    }
}
