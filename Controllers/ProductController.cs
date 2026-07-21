using ECommerceWeb.Data;
using ECommerceWeb.Models;
using ECommerceWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 8;
            int? userId = null;

            // Lấy UserID nếu người dùng đã đăng nhập (cải tiến: lấy từ claim NameIdentifier)
            if (User.Identity.IsAuthenticated)
            {
                // Cách tốt hơn: dùng ClaimTypes.NameIdentifier nếu bạn đã lưu UserId khi login
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int id))
                {
                    userId = id;
                }
                else
                {
                    // Fallback: tìm theo email
                    var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;
                    var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                    if (user != null) userId = user.UserID;
                }
            }

            // Lấy danh sách ProductID mà user này đã thích
            var favoriteProductIds = userId.HasValue
                ? _context.Favorites.Where(f => f.UserID == userId).Select(f => f.ProductID).ToHashSet()
                : new HashSet<int>();

            // Query cơ bản
            var query = _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt);

            // Tính tổng số sản phẩm (để phân trang)
            int totalProducts = query.Count();
            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            // Lấy dữ liệu cho trang hiện tại
            var products = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Ánh xạ sang ViewModel
            var productViewModels = products.Select(p => new ProductCardViewModel
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                Price = p.Price,
                OldPrice = p.OldPrice,
                ImageUrl = p.ImageUrl,
                CategoryName = p.Category?.CategoryName,
                Rating = p.Rating,
                ReviewCount = p.ReviewCount,
                IsNew = p.IsNew,
                IsFavorite = favoriteProductIds.Contains(p.ProductID)
            }).ToList();

            // Gán ViewBag cho phân trang
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            // Trả về View với ViewModel
            return View(productViewModels);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        //Danh mục
        [HttpGet]
        public async Task<IActionResult> Category(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound();
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryID == id)
                .OrderByDescending(p => p.ProductID)
                .ToListAsync();

            ViewBag.CategoryName = category.CategoryName;
            ViewBag.CategoryId = category.CategoryID;

            return View(products);
        }

        public IActionResult Search(string keyword)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Where(p =>
                    string.IsNullOrEmpty(keyword)
                    || p.ProductName.Contains(keyword)
                    || p.Description.Contains(keyword)
                    || p.Category.CategoryName.Contains(keyword)
                )
                .ToList();

            ViewBag.Keyword = keyword;

            return View(products);
        }
        public IActionResult AddToCart(int id)
        {
            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize] // Yêu cầu đăng nhập mới được thả tim
        public async Task<IActionResult> ToggleFavorite(int productId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập." });

            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserID == user.UserID && f.ProductID == productId);

            bool isNowFavorited;

            if (existingFavorite != null)
            {
                // Đã thích rồi -> Hủy thích
                _context.Favorites.Remove(existingFavorite);
                isNowFavorited = false;
            }
            else
            {
                // Chưa thích -> Thêm vào yêu thích
                _context.Favorites.Add(new Favorite
                {
                    UserID = user.UserID,
                    ProductID = productId
                });
                isNowFavorited = true;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, isFavorited = isNowFavorited });
        }
    }
}