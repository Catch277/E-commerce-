using ECommerceWeb.Data;
using ECommerceWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWeb.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị giỏ hàng
        public IActionResult Index(int userId)
        {
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserID == userId);

            return View(cart);
        }

        public IActionResult AddToCart(int userId, int productId, int quantity = 1)
        {
            // Kiểm tra sản phẩm
            var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);

            if (product == null)
            {
                return NotFound();
            }

            // Tìm giỏ hàng của người dùng
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserID == userId);

            // Nếu chưa có giỏ thì tạo mới
            if (cart == null)
            {
                cart = new Cart
                {
                    UserID = userId,
                    CreatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };

                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var cartItem = _context.CartItems.FirstOrDefault(c =>
                c.CartID == cart.CartID &&
                c.ProductID == productId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartID = cart.CartID,
                    ProductID = product.ProductID,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };

                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            _context.SaveChanges();

            return RedirectToAction("Index", new { userId });
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            var cartItem = _context.CartItems
                .Include(c => c.Cart)
                .FirstOrDefault(c => c.CartItemID == cartItemId);

            if (cartItem == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                quantity = 1;
            }

            cartItem.Quantity = quantity;

            _context.SaveChanges();

            return RedirectToAction("Index", new { userId = cartItem.Cart.UserID });
        }

        public IActionResult Remove(int cartItemId)
        {
            var cartItem = _context.CartItems
                .Include(c => c.Cart)
                .FirstOrDefault(c => c.CartItemID == cartItemId);

            if (cartItem == null)
            {
                return NotFound();
            }

            int userId = cartItem.Cart.UserID;

            _context.CartItems.Remove(cartItem);

            _context.SaveChanges();

            return RedirectToAction("Index", new { userId });
        }

        public IActionResult Clear(int userId)
        {
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserID == userId);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);

                _context.SaveChanges();
            }

            return RedirectToAction("Index", new { userId });
        }
    }
}