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

                PendingOrders = await _context.Orders
                    .CountAsync(o => o.OrderStatus == "Chờ xác nhận"),

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

        // GET: /Admin/Orders
        [HttpGet]
        public async Task<IActionResult> Orders(
            string? search,
            string? status,
            string? paymentMethod,
            int page = 1)
        {
            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            var query = _context.Orders
                .AsNoTracking()
                .AsQueryable();

            // Tìm theo mã đơn, tên khách hoặc số điện thoại.
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();

                if (int.TryParse(keyword, out int orderId))
                {
                    query = query.Where(o =>
                        o.OrderID == orderId ||
                        o.CustomerName.Contains(keyword) ||
                        o.CustomerPhone.Contains(keyword));
                }
                else
                {
                    query = query.Where(o =>
                        o.CustomerName.Contains(keyword) ||
                        o.CustomerPhone.Contains(keyword) ||
                        o.CustomerEmail.Contains(keyword));
                }
            }

            // Lọc trạng thái đơn hàng.
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            // Lọc phương thức thanh toán.
            if (!string.IsNullOrWhiteSpace(paymentMethod))
            {
                query = query.Where(o =>
                    o.PaymentMethod == paymentMethod);
            }

            var totalOrders = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalOrders / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new AdminOrderListItemViewModel
                {
                    OrderID = o.OrderID,
                    OrderDate = o.OrderDate,
                    CustomerName = o.CustomerName,
                    CustomerPhone = o.CustomerPhone,
                    TotalAmount = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod,
                    OrderStatus = o.OrderStatus,

                    TotalItems = o.OrderDetails.Sum(
                        detail => detail.Quantity),

                    PaymentStatus = _context.Payments
                        .Where(p => p.OrderID == o.OrderID)
                        .Select(p => p.PaymentStatus)
                        .FirstOrDefault() ?? "Chưa có thông tin"
                })
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedPaymentMethod = paymentMethod;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages == 0 ? 1 : totalPages;
            ViewBag.TotalOrders = totalOrders;

            ViewBag.PendingOrderCount = await _context.Orders
                .CountAsync(o => o.OrderStatus == "Chờ xác nhận");

            return View(orders);
        }

        // GET: /Admin/OrderDetails/5
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .Include(o => o.OrderDetails)
                    .ThenInclude(detail => detail.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OrderID == id);

            var shipping = await _context.Shippings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.OrderID == id);

            var model = new AdminOrderDetailViewModel
            {
                Order = order,
                Payment = payment,
                Shipping = shipping,
                AllowedNextStatuses =
                    GetAllowedNextOrderStatuses(order.OrderStatus)
            };

            return View(model);
        }

        // POST: /Admin/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(
            int orderId,
            string newStatus)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null)
            {
                return NotFound();
            }

            newStatus = newStatus?.Trim() ?? string.Empty;

            var allowedStatuses = GetAllowedNextOrderStatuses(order.OrderStatus);

            if (!allowedStatuses.Contains(newStatus))
            {
                TempData["ErrorMessage"] =
                    $"Không thể chuyển đơn từ " +
                    $"\"{order.OrderStatus}\" sang \"{newStatus}\".";

                return RedirectToAction(
                    nameof(OrderDetails),
                    new { id = orderId });
            }

            var previousStatus = order.OrderStatus;

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                order.OrderStatus = newStatus;

                // Khi bắt đầu giao hàng, tạo bản ghi Shipping nếu chưa có.
                if (newStatus == "Đang giao")
                {
                    var shipping = await _context.Shippings
                        .FirstOrDefaultAsync(s =>
                            s.OrderID == orderId);

                    if (shipping == null)
                    {
                        shipping = new Shipping
                        {
                            OrderID = orderId,
                            ShippingCompany = "Chưa cập nhật",
                            TrackingCode = string.Empty,
                            ShippingStatus = "Đang giao",
                            EstimatedDeliveryDate =
                                DateTime.Now.AddDays(3)
                        };

                        _context.Shippings.Add(shipping);
                    }
                    else
                    {
                        shipping.ShippingStatus = "Đang giao";
                    }
                }

                // Khi giao thành công, cập nhật cả Shipping.
                if (newStatus == "Đã giao")
                {
                    var shipping = await _context.Shippings
                        .FirstOrDefaultAsync(s =>
                            s.OrderID == orderId);

                    if (shipping != null)
                    {
                        shipping.ShippingStatus = "Đã giao";
                    }

                    // COD chỉ được xem là thanh toán thành công
                    // khi khách đã nhận hàng.
                    if (string.Equals(
                        order.PaymentMethod,
                        "COD",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        var payment = await _context.Payments
                            .FirstOrDefaultAsync(p =>
                                p.OrderID == orderId);

                        if (payment != null)
                        {
                            payment.PaymentStatus = "Thành công";
                            payment.PaymentDate = DateTime.Now;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    $"Đã chuyển trạng thái đơn #{orderId} " +
                    $"từ \"{previousStatus}\" sang \"{newStatus}\".";
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["ErrorMessage"] =
                    "Không thể cập nhật trạng thái đơn hàng.";

                throw;
            }

            return RedirectToAction(
                nameof(OrderDetails),
                new { id = orderId });
        }

        private static List<string> GetAllowedNextOrderStatuses(string currentStatus)
        {
            return currentStatus switch
            {
                "Chờ xác nhận" => new List<string>
            {
                "Đã xác nhận"
            },

                "Đã xác nhận" => new List<string>
            {
                "Đang giao"
            },

                "Đang giao" => new List<string>
            {
                "Đã giao"
            },
                    _ => new List<string>()
                };
            }

        // GET: /Admin/Vouchers
        [HttpGet]
        public async Task<IActionResult> Vouchers(
            string? search,
            string? status)
        {
            var query = _context.Vouchers
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();

                query = query.Where(v =>
                    v.Code.Contains(keyword) ||
                    v.Title.Contains(keyword));
            }

            var now = DateTime.Now;

            query = status switch
            {
                "active" => query.Where(v =>
                    v.IsActive &&
                    v.ExpiryDate >= now &&
                    (!v.UsageLimit.HasValue ||
                     v.UsedCount < v.UsageLimit.Value)),

                "inactive" => query.Where(v => !v.IsActive),

                "expired" => query.Where(v =>
                    v.ExpiryDate < now),

                "out-of-uses" => query.Where(v =>
                    v.UsageLimit.HasValue &&
                    v.UsedCount >= v.UsageLimit.Value),

                _ => query
            };

            var vouchers = await query
                .OrderByDescending(v => v.VoucherID)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.SelectedStatus = status;

            return View(vouchers);
        }

        // GET: /Admin/CreateVoucher
        [HttpGet]
        public IActionResult CreateVoucher()
        {
            var model = new AdminVoucherViewModel
            {
                ExpiryDate = DateTime.Today.AddDays(30),
                DiscountType = "Fixed",
                IsActive = true
            };

            return View(model);
        }

        // POST: /Admin/CreateVoucher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVoucher(
            AdminVoucherViewModel model)
        {
            NormalizeVoucherModel(model);
            ValidateVoucherModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool codeExists = await _context.Vouchers
                .AnyAsync(v => v.Code == model.Code);

            if (codeExists)
            {
                ModelState.AddModelError(
                    nameof(model.Code),
                    "Mã khuyến mãi này đã tồn tại.");

                return View(model);
            }

            var voucher = new Voucher
            {
                Code = model.Code,
                Title = model.Title.Trim(),
                Description = model.Description?.Trim()
                    ?? string.Empty,

                DiscountType = model.DiscountType,
                DiscountValue = model.DiscountValue,

                MaxDiscountAmount =
                    model.DiscountType == "Percent"
                        ? model.MaxDiscountAmount
                        : null,

                MinOrderValue = model.MinOrderValue,
                ExpiryDate = model.ExpiryDate.Date
                    .AddDays(1)
                    .AddTicks(-1),

                UsageLimit = model.UsageLimit,
                UsedCount = 0,
                IsActive = model.IsActive
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Đã tạo mã khuyến mãi {voucher.Code}.";

            return RedirectToAction(nameof(Vouchers));
        }

        // GET: /Admin/EditVoucher/5
        [HttpGet]
        public async Task<IActionResult> EditVoucher(int id)
        {
            var voucher = await _context.Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VoucherID == id);

            if (voucher == null)
            {
                return NotFound();
            }

            var model = new AdminVoucherViewModel
            {
                VoucherID = voucher.VoucherID,
                Code = voucher.Code,
                Title = voucher.Title,
                Description = voucher.Description,
                DiscountType = voucher.DiscountType,
                DiscountValue = voucher.DiscountValue,
                MaxDiscountAmount = voucher.MaxDiscountAmount,
                MinOrderValue = voucher.MinOrderValue,
                ExpiryDate = voucher.ExpiryDate.Date,
                UsageLimit = voucher.UsageLimit,
                IsActive = voucher.IsActive
            };

            return View(model);
        }

        // POST: /Admin/EditVoucher/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVoucher(
            int id,
            AdminVoucherViewModel model)
        {
            if (id != model.VoucherID)
            {
                return BadRequest();
            }

            NormalizeVoucherModel(model);
            ValidateVoucherModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.VoucherID == id);

            if (voucher == null)
            {
                return NotFound();
            }

            bool duplicateCode = await _context.Vouchers
                .AnyAsync(v =>
                    v.VoucherID != id &&
                    v.Code == model.Code);

            if (duplicateCode)
            {
                ModelState.AddModelError(
                    nameof(model.Code),
                    "Mã khuyến mãi này đã tồn tại.");

                return View(model);
            }

            if (model.UsageLimit.HasValue &&
                model.UsageLimit.Value < voucher.UsedCount)
            {
                ModelState.AddModelError(
                    nameof(model.UsageLimit),
                    $"Giới hạn lượt dùng không được nhỏ hơn " +
                    $"số lượt đã sử dụng ({voucher.UsedCount}).");

                return View(model);
            }

            voucher.Code = model.Code;
            voucher.Title = model.Title.Trim();
            voucher.Description =
                model.Description?.Trim() ?? string.Empty;

            voucher.DiscountType = model.DiscountType;
            voucher.DiscountValue = model.DiscountValue;

            voucher.MaxDiscountAmount =
                model.DiscountType == "Percent"
                    ? model.MaxDiscountAmount
                    : null;

            voucher.MinOrderValue = model.MinOrderValue;

            voucher.ExpiryDate = model.ExpiryDate.Date
                .AddDays(1)
                .AddTicks(-1);

            voucher.UsageLimit = model.UsageLimit;
            voucher.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Đã cập nhật mã {voucher.Code}.";

            return RedirectToAction(nameof(Vouchers));
        }

        // POST: /Admin/ToggleVoucherStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVoucherStatus(int id)
        {
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.VoucherID == id);

            if (voucher == null)
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy mã khuyến mãi.";

                return RedirectToAction(nameof(Vouchers));
            }

            voucher.IsActive = !voucher.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                voucher.IsActive
                    ? $"Đã bật mã {voucher.Code}."
                    : $"Đã tắt mã {voucher.Code}.";

            return RedirectToAction(nameof(Vouchers));
        }

        // POST: /Admin/DeleteVoucher/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.VoucherID == id);

            if (voucher == null)
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy mã khuyến mãi.";

                return RedirectToAction(nameof(Vouchers));
            }

            bool usedInOrder = await _context.Orders
                .AnyAsync(o => o.VoucherID == id);

            bool assignedToUser = await _context.UserVouchers
                .AnyAsync(uv => uv.VoucherID == id);

            if (usedInOrder || assignedToUser ||
                voucher.UsedCount > 0)
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa voucher đã được sử dụng hoặc " +
                    "đã cấp cho người dùng. Hãy tắt voucher thay vì xóa.";

                return RedirectToAction(nameof(Vouchers));
            }

            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Đã xóa mã {voucher.Code}.";

            return RedirectToAction(nameof(Vouchers));
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

        private static void NormalizeVoucherModel(AdminVoucherViewModel model)
        {
            model.Code = (model.Code ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            model.Title =
                model.Title?.Trim() ?? string.Empty;

            model.Description =
                model.Description?.Trim() ?? string.Empty;

            model.DiscountType =
                model.DiscountType?.Trim() ?? string.Empty;
        }

        private void ValidateVoucherModel(AdminVoucherViewModel model)
        {
            if (model.DiscountType != "Fixed" &&
                model.DiscountType != "Percent")
            {
                ModelState.AddModelError(
                    nameof(model.DiscountType),
                    "Loại giảm giá không hợp lệ.");
            }

            if (model.DiscountType == "Percent")
            {
                if (model.DiscountValue <= 0 ||
                    model.DiscountValue > 100)
                {
                    ModelState.AddModelError(
                        nameof(model.DiscountValue),
                        "Phần trăm giảm phải lớn hơn 0 và không vượt quá 100.");
                }

                if (model.MaxDiscountAmount.HasValue &&
                    model.MaxDiscountAmount.Value <= 0)
                {
                    ModelState.AddModelError(
                        nameof(model.MaxDiscountAmount),
                        "Mức giảm tối đa phải lớn hơn 0.");
                }
            }

            if (model.DiscountType == "Fixed")
            {
                model.MaxDiscountAmount = null;
            }

            if (model.ExpiryDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.ExpiryDate),
                    "Ngày hết hạn không được nhỏ hơn ngày hiện tại.");
            }

            if (model.UsageLimit.HasValue &&
                model.UsageLimit.Value <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.UsageLimit),
                    "Giới hạn lượt sử dụng phải lớn hơn 0.");
            }
        }
    }
}