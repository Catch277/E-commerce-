using ECommerceWeb.Data;
using ECommerceWeb.Models;
using ECommerceWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceWeb.Services;

namespace ECommerceWeb.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public CheckoutController(
            ApplicationDbContext context,
            EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Hiển thị form đặt hàng
        [HttpGet]
        public IActionResult Index(int userId)
        {
            var model = new CheckoutViewModel
            {
                UserID = userId
            };

            return View(model);
        }

        // Xử lý đặt hàng
        [HttpPost]
        public IActionResult Index(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserID == model.UserID);

            if (cart == null || !cart.CartItems.Any())
            {
                ModelState.AddModelError("", "Giỏ hàng trống.");
                return View(model);
            }

            decimal total = 0;

            // Kiểm tra tồn kho
            foreach (var item in cart.CartItems)
            {
                if (item.Quantity > item.Product.Quantity)
                {
                    ModelState.AddModelError("", $"Sản phẩm {item.Product.ProductName} không đủ số lượng.");
                    return View(model);
                }

                total += item.Quantity * item.UnitPrice;
            }

            // Tạo Order
            Order order = new Order
            {
                UserID = model.UserID,
                ShippingAddress = model.ShippingAddress,
                TotalAmount = total,
                OrderDate = DateTime.Now,
                OrderStatus = "Chờ xác nhận"
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Tạo OrderDetail + cập nhật tồn kho
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

                item.Product.Quantity -= item.Quantity;
            }

            // Xóa giỏ hàng
            _context.CartItems.RemoveRange(cart.CartItems);

            _context.SaveChanges();

            // Lấy thông tin người dùng
            var user = _context.Users.FirstOrDefault(u => u.UserID == model.UserID);

            if (user != null)
            {
                string subject = "Xác nhận đơn hàng";

                string body = $@"
Xin chào {user.FullName},

Bạn đã đặt hàng thành công.

Mã đơn hàng: {order.OrderID}

Tổng tiền: {order.TotalAmount:N0} VNĐ

Trạng thái: {order.OrderStatus}

Cảm ơn bạn đã mua hàng.
";

                try
                {
                    _emailService.Send(user.Email, subject, body);

                    EmailLog log = new EmailLog
                    {
                        UserID = user.UserID,
                        OrderID = order.OrderID,
                        EmailSubject = subject,
                        EmailContent = body,
                        SentAt = DateTime.Now,
                        EmailStatus = "Đã gửi"
                    };

                    _context.EmailLogs.Add(log);
                }
                catch (Exception ex)
                {
                    EmailLog log = new EmailLog
                    {
                        UserID = user.UserID,
                        OrderID = order.OrderID,
                        EmailSubject = subject,
                        EmailContent = ex.Message,
                        SentAt = DateTime.Now,
                        EmailStatus = "Gửi thất bại"
                    };

                    _context.EmailLogs.Add(log);
                    _context.SaveChanges();

                    throw;
                }
            }
            ;

                    _context.EmailLogs.Add(log);
                }

                _context.SaveChanges();
            }

            TempData["Success"] = "Đặt hàng thành công!";

            return RedirectToAction("Index", "Home");
        }
    }
}