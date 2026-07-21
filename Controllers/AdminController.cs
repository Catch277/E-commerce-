using ECommerceWeb.Data;
using ECommerceWeb.Models;
using ECommerceWeb.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.Quantity <= 10)
                .OrderBy(p => p.Quantity)
                .Take(5)
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                LowStockProducts = await _context.Products
                    .CountAsync(p => p.Quantity <= 5),
                LowStockProductList = lowStockProducts
            };

            return View(model);
        }

        // GET: /Admin/Products
        private const int AdminPageSize = 10;

        [HttpGet]
        public async Task<IActionResult> Products(string search, int? categoryId, string stockStatus, int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // ---- Lọc theo từ khóa (tên sản phẩm hoặc ID) ----
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                if (int.TryParse(keyword, out int searchId))
                {
                    query = query.Where(p => p.ProductID == searchId || p.ProductName.Contains(keyword));
                }
                else
                {
                    query = query.Where(p => p.ProductName.Contains(keyword));
                }
            }

            // ---- Lọc theo danh mục ----
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryID == categoryId.Value);
            }

            // ---- Lọc theo trạng thái tồn kho ----
            if (!string.IsNullOrWhiteSpace(stockStatus))
            {
                query = stockStatus switch
                {
                    "out-stock" => query.Where(p => p.Quantity <= 0),
                    "low-stock" => query.Where(p => p.Quantity > 0 && p.Quantity <= 10),
                    "in-stock" => query.Where(p => p.Quantity > 10),
                    _ => query
                };
            }

            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)AdminPageSize);

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * AdminPageSize)
                .Take(AdminPageSize)
                .ToListAsync();

            // ---- Đổ danh sách category cho dropdown filter ----
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

            ViewBag.Search = search;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedStockStatus = stockStatus;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages == 0 ? 1 : totalPages;
            ViewBag.TotalProducts = totalProducts;

            return View(products);
        }

        // GET: /Admin/CreateProduct
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            var model = new AdminProductViewModel
            {
                Categories = await GetCategoryOptionsAsync()
            };

            return View(model);
        }

        // POST: /Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(
            AdminProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategoryOptionsAsync(
                    model.CategoryID);

                return View(model);
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryID == model.CategoryID);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryID),
                    "Danh mục được chọn không tồn tại.");

                model.Categories = await GetCategoryOptionsAsync(
                    model.CategoryID);

                return View(model);
            }

            var product = new Product
            {
                ProductName = model.ProductName.Trim(),
                Description = model.Description?.Trim() ?? string.Empty,
                Price = model.Price,
                Quantity = model.Quantity,
                ImageUrl = model.ImageUrl?.Trim() ?? string.Empty,
                CategoryID = model.CategoryID,
                CreatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm sản phẩm thành công.";

            return RedirectToAction(nameof(Products));
        }

        // GET: /Admin/EditProduct/5
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            var model = new AdminProductViewModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                ImageUrl = product.ImageUrl,
                CategoryID = product.CategoryID,
                Categories = await GetCategoryOptionsAsync(
                    product.CategoryID)
            };

            return View(model);
        }

        // POST: /Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(
            int id,
            AdminProductViewModel model)
        {
            if (id != model.ProductID)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategoryOptionsAsync(
                    model.CategoryID);

                return View(model);
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryID == model.CategoryID);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryID),
                    "Danh mục được chọn không tồn tại.");

                model.Categories = await GetCategoryOptionsAsync(
                    model.CategoryID);

                return View(model);
            }

            product.ProductName = model.ProductName.Trim();
            product.Description = model.Description?.Trim() ?? string.Empty;
            product.Price = model.Price;
            product.Quantity = model.Quantity;
            product.ImageUrl = model.ImageUrl?.Trim() ?? string.Empty;
            product.CategoryID = model.CategoryID;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công.";

            return RedirectToAction(nameof(Products));
        }

        // POST: /Admin/DeleteProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(Products));
            }

            var existsInOrder = await _context.OrderDetails
                .AnyAsync(od => od.ProductID == id);

            if (existsInOrder)
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa sản phẩm vì sản phẩm đã xuất hiện trong đơn hàng.";

                return RedirectToAction(nameof(Products));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa sản phẩm thành công.";

            return RedirectToAction(nameof(Products));
        }

        // GET: /Admin/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        // GET: /Admin/CreateCategory
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View(new Category());
        }

        // POST: /Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedName = model.CategoryName.Trim();

            var exists = await _context.Categories
                .AnyAsync(c => c.CategoryName == normalizedName);

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryName),
                    "Tên danh mục đã tồn tại.");

                return View(model);
            }

            model.CategoryName = normalizedName;
            model.Description = model.Description?.Trim() ?? string.Empty;

            _context.Categories.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm danh mục thành công.";

            return RedirectToAction(nameof(Categories));
        }

        // GET: /Admin/EditCategory/5
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories
                .FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: /Admin/EditCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(
            int id,
            Category model)
        {
            if (id != model.CategoryID)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound();
            }

            var normalizedName = model.CategoryName.Trim();

            var duplicate = await _context.Categories.AnyAsync(c =>
                c.CategoryID != id &&
                c.CategoryName == normalizedName);

            if (duplicate)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryName),
                    "Tên danh mục đã tồn tại.");

                return View(model);
            }

            category.CategoryName = normalizedName;
            category.Description = model.Description?.Trim() ?? string.Empty;
            category.IconClass = string.IsNullOrWhiteSpace(model.IconClass) ? "bi-cpu" : model.IconClass;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật danh mục thành công.";

            return RedirectToAction(nameof(Categories));
        }

        // POST: /Admin/DeleteCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(Categories));
            }

            if (category.Products.Any())
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa danh mục đang chứa sản phẩm.";

                return RedirectToAction(nameof(Categories));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa danh mục thành công.";

            return RedirectToAction(nameof(Categories));
        }

        private async Task<IEnumerable<SelectListItem>> GetCategoryOptionsAsync(int? selectedCategoryID = null)
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryID.ToString(),
                    Text = c.CategoryName,
                    Selected = selectedCategoryID == c.CategoryID
                })
                .ToListAsync();
        }
    }
}