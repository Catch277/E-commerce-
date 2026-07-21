using ECommerceWeb.Data;
using ECommerceWeb.Models;
using ECommerceWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var favoriteProductIds = new HashSet<int>();
            if (User.Identity?.IsAuthenticated == true)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;
                if (!string.IsNullOrWhiteSpace(userEmail))
                {
                    favoriteProductIds = (await _context.Favorites
                        .Where(f => f.User.Email == userEmail)
                        .Select(f => f.ProductID)
                        .ToListAsync())
                        .ToHashSet();
                }
            }

            // Deal: sản phẩm đang có giảm giá (OldPrice > Price)
            var dealProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.OldPrice != null && p.OldPrice > p.Price)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .Select(p => MapToProductCard(p))
                .ToListAsync();

            // Sản phẩm mới về: IsNew = true
            var newArrivals = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsNew)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .Select(p => MapToProductCard(p))
                .ToListAsync();

            foreach (var product in dealProducts.Concat(newArrivals))
            {
                product.IsFavorite = favoriteProductIds.Contains(product.ProductID);
            }

            var prebuiltPcs = await _context.PrebuiltPcs
                .Include(p => p.Specs)
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new PrebuiltPcViewModel
                {
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    Price = p.Price,
                    IsBestSeller = p.IsBestSeller,
                    Specs = p.Specs.OrderBy(s => s.DisplayOrder).Select(s => s.SpecText).ToList()
                })
                .ToListAsync();

            var newsArticles = await _context.NewsArticles
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.PublishedAt)
                .Take(3)
                .Select(a => new NewsArticleViewModel
                {
                    Category = a.Category,
                    Title = a.Title,
                    Excerpt = a.Excerpt,
                    ImageUrl = a.ImageUrl
                })
                .ToListAsync();

            var model = new HomeIndexViewModel
            {
                DealProducts = dealProducts,
                NewArrivals = newArrivals,
                PrebuiltPcs = prebuiltPcs,
                NewsArticles = newsArticles
            };

            ViewBag.StoreName = "PC Store";
            ViewBag.StoreDescription = "Chuyên cung cấp linh kiện máy tính, gaming gear và phụ kiện chính hãng với giá tốt, bảo hành uy tín.";
            ViewBag.CartCount = 0;
            ViewBag.CountdownHours = "12";
            ViewBag.CountdownMinutes = "45";
            ViewBag.CountdownSeconds = "30";
            ViewBag.PromoTitle1 = "Laptop Gaming Mới";
            ViewBag.PromoImage1 = "Laptop+Gaming";
            ViewBag.PromoTitle2 = "Linh kiện đỉnh cao";
            ViewBag.PromoSubtitle2 = "VGA RTX 40 Series";
            ViewBag.PromoImage2 = "VGA+RTX+40";

            return View(model);
        }

        private static ProductCardViewModel MapToProductCard(Product p)
        {
            int? discountPercent = null;
            if (p.OldPrice.HasValue && p.OldPrice.Value > 0 && p.OldPrice.Value > p.Price)
            {
                discountPercent = (int)Math.Round((1 - (p.Price / p.OldPrice.Value)) * 100);
            }

            return new ProductCardViewModel
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                CategoryName = p.Category?.CategoryName ?? "",
                ImageUrl = p.ImageUrl,
                Price = p.Price,
                OldPrice = p.OldPrice,
                Rating = p.Rating,
                ReviewCount = p.ReviewCount,
                IsNew = p.IsNew,
                DiscountPercent = discountPercent,
                DetailUrl = "#"
            };
        }
    }
}
