using ECommerceWeb.Data;
using ECommerceWeb.Models;
using ECommerceWeb.Services;
using ECommerceWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWeb.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly ApplicationDbContext _context;
        private readonly IMomoService _momoService;
        private readonly IMembershipService _membershipService;

        public PaymentController(
            IVnPayService vnPayService,
            IMomoService momoService,
            ApplicationDbContext context,
            IMembershipService membershipService)
        {
            _vnPayService = vnPayService;
            _momoService = momoService;
            _context = context;
            _membershipService = membershipService;
        }

        public IActionResult Index(int? orderId)
        {
            if (orderId.HasValue)
            {
                var order = _context.Orders.Find(orderId.Value);
                if (order == null) return NotFound();

                ViewBag.Order = order;
            }

            return View();
        }

        [HttpGet]
        public IActionResult CreatePaymentUrl(int orderId)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(PaymentFail));
            }

            var model = new PaymentInformationModel
            {
                OrderType = "other",
                Amount = (double)order.TotalAmount,
                OrderDescription = $"Thanh toan don hang: {order.OrderID}",
                Name = order.CustomerName ?? "Khach hang",
                OrderId = order.OrderID.ToString()
            };

            string url =
                _vnPayService.CreatePaymentUrl(HttpContext, model);

            return Redirect(url);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            int.TryParse(response?.OrderId, out int orderId);

            if (response == null ||
                !response.Success ||
                response.VnPayResponseCode != "00" ||
                orderId <= 0)
            {
                TempData["ErrorMessage"] =
                    $"Thanh toán VNPay không thành công hoặc đã bị hủy. " +
                    $"Mã kết quả: {response?.VnPayResponseCode}";

                if (orderId > 0)
                {
                    await MarkPaymentFailedAsync(orderId, "VNPay");
                }

                return RedirectToAction(
                    nameof(PaymentFail),
                    new
                    {
                        orderId = orderId > 0
                            ? orderId
                            : (int?)null
                    });
            }

            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.OrderID == orderId &&
                    p.PaymentMethod == "VNPay");

            if (payment == null)
            {
                payment = new Payment
                {
                    OrderID = orderId,
                    PaymentMethod = "VNPay",
                    PaymentStatus = "Chưa thanh toán",
                    TransactionCode = string.Empty
                };

                _context.Payments.Add(payment);
            }

            bool wasAlreadyPaid =
                payment.PaymentStatus == "Thành công";

            bool completed = await CompleteOnlineOrderAsync(order ,payment, response.TransactionId ?? string.Empty);

            if (!completed)
            {
                return RedirectToAction(
                    nameof(PaymentFail),
                    new { orderId });
            }

            // Chỉ cộng điểm lần đầu giao dịch được xử lý.
            if (!wasAlreadyPaid)
            {
                await _membershipService.AddPointsAsync(
                    order.UserID,
                    order.OrderID,
                    order.TotalAmount);
            }

            TempData["SuccessMessage"] =
                "Thanh toán VNPay thành công!";

            return RedirectToAction(
                nameof(PaymentSuccess),
                new { orderId });
        }

        [HttpGet]
        public async Task<IActionResult> CreateMoMoPayment(int orderId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(PaymentFail));
            }

            try
            {
                var model = new PaymentInformationModel
                {
                    Amount = (double)order.TotalAmount,
                    OrderDescription =
                        $"Thanh toan MoMo don hang: {order.OrderID}",
                    Name = order.CustomerName ?? "Khach hang",
                    OrderId = order.OrderID.ToString()
                };

                var response =
                    await _momoService.CreatePaymentAsync(model);

                return Redirect(response.PayUrl!);
            }
            catch (Exception ex)
            {
                await MarkPaymentFailedAsync(orderId, "MoMo");
                TempData["ErrorMessage"] =
                    $"Không thể khởi tạo thanh toán MoMo: {ex.Message}";
                return RedirectToAction(nameof(PaymentFail), new { orderId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MoMoCallback()
        {
            var response = _momoService.PaymentExecuteAsync(Request.Query);

            // MoMo orderId có dạng OrderID-Timestamp.
            // Ta tách OrderID nội bộ kể cả khi giao dịch thất bại.
            string internalOrderId =
                response?.OrderId?
                    .Split('-', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault()
                ?? string.Empty;

            int.TryParse(internalOrderId, out int orderId);

            if (response == null || !response.Success)
            {
                TempData["ErrorMessage"] =
                    $"Giao dịch MoMo không thành công hoặc đã bị hủy. " +
                    $"Mã kết quả: {response?.ResponseCode}";

                if (orderId > 0)
                {
                    await MarkPaymentFailedAsync(orderId, "MoMo");
                }

                return RedirectToAction(
                    nameof(PaymentFail),
                    new
                    {
                        orderId = orderId > 0 ? orderId : (int?)null
                    });
            }

            if (orderId <= 0)
            {
                TempData["ErrorMessage"] =
                    "Không xác định được mã đơn hàng MoMo.";

                return RedirectToAction(nameof(PaymentFail));
            }

            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound();
            }

            long expectedAmount = Convert.ToInt64(
                Math.Round(
                    order.TotalAmount,
                    MidpointRounding.AwayFromZero));

            if (response.Amount != expectedAmount)
            {
                TempData["ErrorMessage"] =
                    "Số tiền MoMo trả về không khớp với đơn hàng.";

                return RedirectToAction(
                    nameof(PaymentFail),
                    new { orderId });
            }

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.OrderID == orderId &&
                    p.PaymentMethod == "MoMo");

            if (payment == null)
            {
                payment = new Payment
                {
                    OrderID = orderId,
                    PaymentMethod = "MoMo",
                    PaymentStatus = "Chưa thanh toán",
                    TransactionCode = string.Empty
                };

                _context.Payments.Add(payment);
            }
            bool wasAlreadyPaid =
                payment.PaymentStatus == "Thành công";
            bool completed = await CompleteOnlineOrderAsync(
                order,
                payment,
                response.TransactionId ?? string.Empty);
            if (!completed)
            {
                return RedirectToAction(
                    nameof(PaymentFail),
                    new { orderId });
            }
            if (!wasAlreadyPaid)
            {
                await _membershipService.AddPointsAsync(
                    order.UserID,
                    order.OrderID,
                    order.TotalAmount);
            }
            TempData["SuccessMessage"] =
                "Thanh toán MoMo thành công!";
            return RedirectToAction(
                nameof(PaymentSuccess),
                new { orderId });
        }

        [HttpGet]
        public IActionResult PaymentSuccess(int? orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PaymentFail(int? orderId)
        {
            Order? order = null;

            if (orderId.HasValue)
            {
                order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderID == orderId.Value);
            }

            ViewBag.OrderId = order?.OrderID;
            ViewBag.PaymentMethod = order?.PaymentMethod;

            return View();
        }

        private async Task<bool> CompleteOnlineOrderAsync(Order order, Payment payment, string transactionCode)
        {
            // Callback có thể bị gửi lại nhiều lần.
            // Nếu đơn đã được xử lý thành công thì không làm lại.
            if (payment.PaymentStatus == "Thành công")
            {
                return true;
            }

            var orderDetails = await _context.OrderDetails
                .Include(detail => detail.Product)
                .Where(detail => detail.OrderID == order.OrderID)
                .ToListAsync();

            if (!orderDetails.Any())
            {
                TempData["ErrorMessage"] =
                    "Đơn hàng không có sản phẩm để hoàn tất.";

                return false;
            }

            // Kiểm tra lại tồn kho trước khi trừ.
            foreach (var detail in orderDetails)
            {
                if (detail.Product == null ||
                    detail.Product.Quantity < detail.Quantity)
                {
                    TempData["ErrorMessage"] =
                        $"Sản phẩm {detail.Product?.ProductName ?? detail.ProductID.ToString()} " +
                        "không còn đủ số lượng để hoàn tất đơn hàng.";

                    return false;
                }
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Trừ tồn kho
                foreach (var detail in orderDetails)
                {
                    detail.Product.Quantity -= detail.Quantity;
                }

                // 2. Tăng lượt dùng voucher nếu đơn có voucher
                if (order.VoucherID.HasValue)
                {
                    var voucher = await _context.Vouchers
                        .FirstOrDefaultAsync(v =>
                            v.VoucherID == order.VoucherID.Value);

                    if (voucher != null)
                    {
                        voucher.UsedCount += 1;
                    }
                }

                // 3. Xóa đúng các sản phẩm thuộc đơn khỏi giỏ hàng
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserID == order.UserID);

                if (cart != null)
                {
                    foreach (var detail in orderDetails)
                    {
                        var cartItem = cart.CartItems
                            .FirstOrDefault(ci =>
                                ci.ProductID == detail.ProductID);

                        if (cartItem == null)
                        {
                            continue;
                        }

                        // Nếu giỏ đang có nhiều hơn số đã mua,
                        // chỉ trừ phần số lượng thuộc đơn hàng.
                        if (cartItem.Quantity > detail.Quantity)
                        {
                            cartItem.Quantity -= detail.Quantity;
                        }
                        else
                        {
                            _context.CartItems.Remove(cartItem);
                        }
                    }
                }

                // 4. Cập nhật đơn và thanh toán
                order.OrderStatus = "Chờ xác nhận";

                payment.PaymentStatus = "Thành công";
                payment.PaymentDate = DateTime.Now;
                payment.TransactionCode =
                    transactionCode ?? string.Empty;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task MarkPaymentFailedAsync(int orderId, string paymentMethod)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null)
            {
                return;
            }

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.OrderID == orderId &&
                    p.PaymentMethod == paymentMethod);

            // Không ghi đè một giao dịch đã thành công.
            if (payment?.PaymentStatus == "Thành công")
            {
                return;
            }

            order.OrderStatus = "Thanh toán thất bại";

            if (payment == null)
            {
                payment = new Payment
                {
                    OrderID = orderId,
                    PaymentMethod = paymentMethod,
                    TransactionCode = string.Empty
                };

                _context.Payments.Add(payment);
            }

            payment.PaymentStatus = "Thất bại";
            payment.PaymentDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<IActionResult> RetryPayment(int orderId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null)
            {
                return NotFound();
            }

            bool alreadyPaid = await _context.Payments
                .AnyAsync(p =>
                    p.OrderID == orderId &&
                    p.PaymentStatus == "Thành công");

            if (alreadyPaid)
            {
                return RedirectToAction(
                    nameof(PaymentSuccess),
                    new { orderId });
            }

            order.OrderStatus = "Chờ thanh toán";
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.OrderID == orderId &&
                    p.PaymentMethod == order.PaymentMethod);
            if (payment != null)
            {
                payment.PaymentStatus = "Chưa thanh toán";
                payment.PaymentDate = null;
                payment.TransactionCode = string.Empty;
            }
            await _context.SaveChangesAsync();
            if (string.Equals(
                order.PaymentMethod,
                "VNPay",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(
                    nameof(CreatePaymentUrl),
                    new { orderId });
            }
            if (string.Equals(order.PaymentMethod, "MoMo", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(
                    nameof(CreateMoMoPayment),
                    new { orderId });
            }
            return RedirectToAction("Index", "Checkout");
        }
    }
}
