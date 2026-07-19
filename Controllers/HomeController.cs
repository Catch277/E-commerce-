using ECommerceWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ECommerceWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // TODO: thay bằng query thật từ DbContext khi có bảng Product/Order
            // Dữ liệu dưới đây chỉ là placeholder để dựng layout, không phải sản phẩm thật

            var model = new HomeIndexViewModel
            {
                DealProducts = new List<ProductCardViewModel>
                {
                    new() { ProductName = "Tên sản phẩm", CategoryName = "Danh mục", Price = 0, OldPrice = 0, DiscountPercent = 0, ReviewCount = 0, ImageUrl = "/images/no-image.png" },
                    new() { ProductName = "Tên sản phẩm", CategoryName = "Danh mục", Price = 0, OldPrice = 0, DiscountPercent = 0, ReviewCount = 0, ImageUrl = "/images/no-image.png" },
                    new() { ProductName = "Tên sản phẩm", CategoryName = "Danh mục", Price = 0, OldPrice = 0, DiscountPercent = 0, ReviewCount = 0, ImageUrl = "/images/no-image.png" },
                    new() { ProductName = "Tên sản phẩm", CategoryName = "Danh mục", Price = 0, OldPrice = 0, DiscountPercent = 0, ReviewCount = 0, ImageUrl = "/images/no-image.png" },
                },
                NewArrivals = new List<ProductCardViewModel>
                {
                    new() { ProductName = "Tên sản phẩm", CategoryName = "Danh mục", Price = 0, IsNew = true, ReviewCount = 0, ImageUrl = "/images/no-image.png" },
                    new() { ProductName = "Tên sản phẩm", CategoryName = "Danh mục", Price = 0, IsNew = true, ReviewCount = 0, ImageUrl = "/images/no-image.png" },
                    new() { ProductName = "Tên sản phẩm", CategoryName = "Danh mục", Price = 0, IsNew = true, ReviewCount = 0, ImageUrl = "/images/no-image.png" },
                    new() { ProductName = "Tên sản phẩm", CategoryName = "Danh mục", Price = 0, IsNew = true, ReviewCount = 0, ImageUrl = "/images/no-image.png" },
                },
                PrebuiltPcs = new List<PrebuiltPcViewModel>
                {
                    new() { Name = "Bộ PC 1", Price = 0, ImageUrl = "/images/no-image.png", Specs = new() { "Thông số 1", "Thông số 2", "Thông số 3", "Thông số 4" } },
                    new() { Name = "Bộ PC 2", Price = 0, ImageUrl = "/images/no-image.png", IsBestSeller = true, Specs = new() { "Thông số 1", "Thông số 2", "Thông số 3", "Thông số 4" } },
                    new() { Name = "Bộ PC 3", Price = 0, ImageUrl = "/images/no-image.png", Specs = new() { "Thông số 1", "Thông số 2", "Thông số 3", "Thông số 4" } },
                },
                NewsArticles = new List<NewsArticleViewModel>
                {
                    new() { Category = "Chủ đề", Title = "Tiêu đề bài viết", Excerpt = "Mô tả ngắn bài viết...", ImageUrl = "/images/no-image.png" },
                    new() { Category = "Chủ đề", Title = "Tiêu đề bài viết", Excerpt = "Mô tả ngắn bài viết...", ImageUrl = "/images/no-image.png" },
                    new() { Category = "Chủ đề", Title = "Tiêu đề bài viết", Excerpt = "Mô tả ngắn bài viết...", ImageUrl = "/images/no-image.png" },
                }
            };

            ViewBag.StoreName = "PC Store";
            ViewBag.StoreDescription = "Mô tả cửa hàng";
            ViewBag.CartCount = 0;
            ViewBag.CountdownHours = "00";
            ViewBag.CountdownMinutes = "00";
            ViewBag.CountdownSeconds = "00";
            ViewBag.PromoTitle1 = "Tiêu đề khuyến mãi 1";
            ViewBag.PromoImage1 = "Promo+1";
            ViewBag.PromoTitle2 = "Tiêu đề khuyến mãi 2";
            ViewBag.PromoSubtitle2 = "Mô tả phụ";
            ViewBag.PromoImage2 = "Promo+2";

            return View(model);
        }
    }
}
