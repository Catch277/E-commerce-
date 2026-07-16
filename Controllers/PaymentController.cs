using ECommerceWeb.Data;
using ECommerceWeb.Models;
using ECommerceWeb.Services;
using ECommerceWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWeb.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly ApplicationDbContext _context;
        private readonly IMomoService _momoService;
        private readonly IMembershipService _membershipService;

        // Tiêm cả Service VNPay, Service MoMo và DbContext vào Controller
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

        // Trang hiển thị form nhập mã đơn hàng để Test
        public IActionResult Index()
        {
            return View();
        }

        // Lấy thông tin thực tế từ CSDL dựa vào orderId
        [HttpPost]
        public IActionResult CreatePaymentUrl(int orderId)
        {
            // Tìm đơn hàng trong CSDL
            var order = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            // Nạp dữ liệu thực tế vào Model để gửi sang VNPay
            var model = new PaymentInformationModel
            {
                OrderType = "other",
                Amount = (double)order.TotalAmount, // Lấy đúng tổng tiền của đơn hàng
                OrderDescription = $"Thanh toan don hang: {order.OrderID}",
                Name = User.Identity.Name ?? "Khach hang", // Lấy tên user đang đăng nhập
                OrderId = order.OrderID.ToString()
            };

            var url = _vnPayService.CreatePaymentUrl(HttpContext, model);
            return Redirect(url);
        }

        // Đón kết quả và lưu Database
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["ErrorMessage"] = $"Lỗi thanh toán VNPay: {response?.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            // Bắt đầu xử lý lưu Database
            int orderId = Convert.ToInt32(response.OrderId);
            var order = _context.Orders.Find(orderId);

            if (order != null)
            {
                // 1. Cập nhật trạng thái đơn hàng
                order.OrderStatus = "Đã thanh toán";

                // 2. Lưu lịch sử giao dịch vào bảng Payments
                var payment = new Payment
                {
                    OrderID = orderId,
                    PaymentMethod = "VNPay",
                    PaymentStatus = "Thành công",
                    PaymentDate = DateTime.Now,
                    TransactionCode = response.TransactionId
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                await _membershipService.AddPointsAsync(order.UserID, order.OrderID, order.TotalAmount);
            }

            TempData["SuccessMessage"] = "Thanh toán VNPay thành công!";
            return RedirectToAction("PaymentSuccess");
        }

        [HttpPost]
        public async Task<IActionResult> CreateMoMoPayment(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);
            if (order == null) return RedirectToAction("Index");

            var model = new PaymentInformationModel
            {
                Amount = (double)order.TotalAmount,
                OrderDescription = $"Thanh toan MoMo don hang: {order.OrderID}",
                OrderId = order.OrderID.ToString()
            };

            var response = await _momoService.CreatePaymentAsync(model);
            return Redirect(response.PayUrl);
        }

        public async Task<IActionResult> MoMoCallback()
        {
            var response = _momoService.PaymentExecuteAsync(Request.Query);

            if (response == null || !response.Success)
            {
                TempData["ErrorMessage"] = "Giao dịch MoMo thất bại hoặc đã bị hủy.";
                return RedirectToAction("PaymentFail");
            }

            // Lưu Database
            int orderId = Convert.ToInt32(response.OrderId);
            var order = _context.Orders.Find(orderId);

            if (order != null)
            {
                order.OrderStatus = "Đã thanh toán";
                _context.Payments.Add(new Payment
                {
                    OrderID = orderId,
                    PaymentMethod = "MoMo",
                    PaymentStatus = "Thành công",
                    PaymentDate = DateTime.Now,
                    TransactionCode = Request.Query["transId"] // Mã giao dịch của MoMo
                });
                await _context.SaveChangesAsync();
                await _membershipService.AddPointsAsync(order.UserID, order.OrderID, order.TotalAmount);
            }

            TempData["SuccessMessage"] = "Thanh toán MoMo thành công!";
            return RedirectToAction("PaymentSuccess");
        }

        public IActionResult PaymentSuccess()
        {
            return View();
        }

        public IActionResult PaymentFail()
        {
            return View();
        }
    }
}