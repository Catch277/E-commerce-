using ECommerceWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            var query = _context.Products
                                .Include(p => p.Category)
                                .OrderByDescending(p => p.CreatedAt);

            int totalProducts = query.Count();

            ViewBag.TotalProducts = totalProducts;

            ViewBag.CurrentPage = page;

            ViewBag.TotalPages =
                (int)Math.Ceiling((double)totalProducts / pageSize);

            var products = query.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return View(products);
        }
        public IActionResult Details(int id)
        {
            var product = _context.Products
                                  .Include(p => p.Category)
                                  .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
                return NotFound();

            return View(product);
        }
        public IActionResult Category(int id)
        {
            var products = _context.Products
                                   .Include(p => p.Category)
                                   .Where(p => p.CategoryID == id)
                                   .ToList();

            ViewBag.CategoryName =
                products.FirstOrDefault()?.Category?.CategoryName;

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
    }
}