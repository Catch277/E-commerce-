using Microsoft.EntityFrameworkCore;
using ECommerceWeb.Models;
using ECommerceWeb.Helpers;

namespace ECommerceWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PrebuiltPc> PrebuiltPcs { get; set; }
        public DbSet<PrebuiltPcSpec> PrebuiltPcSpecs { get; set; }
        public DbSet<NewsArticle> NewsArticles { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Shipping> Shippings { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<MembershipTier> MembershipTiers { get; set; }
        public DbSet<TierBenefit> TierBenefits { get; set; }
        public DbSet<PointsHistory> PointsHistories { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<UserVoucher> UserVouchers { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<WarrantyTicket> WarrantyTickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Tránh lỗi vòng lặp cascade khi xóa dữ liệu
            modelBuilder.Entity<User>()
                .HasOne(u => u.Tier)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TierID)
                .OnDelete(DeleteBehavior.Restrict);

            // Tắt Cascade Delete từ Order -> OrderDetail để tránh lỗi vòng lặp
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderID)
                .OnDelete(DeleteBehavior.Restrict);

            // Tắt Cascade Delete từ User -> EmailLog
            modelBuilder.Entity<EmailLog>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Restrict);
            // Favorite
            modelBuilder.Entity<Favorite>()
                .HasKey(f => new { f.UserID, f.ProductID });

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductID)
                .OnDelete(DeleteBehavior.Cascade);

            // Tắt Cascade Delete
            modelBuilder.Entity<PrebuiltPcSpec>()
                .HasOne(s => s.PrebuiltPc)
                .WithMany(p => p.Specs)
                .HasForeignKey(s => s.PrebuiltPcID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // Tắt Cascade Delete từ Order -> EmailLog
            modelBuilder.Entity<EmailLog>()
                .HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserVoucher>()
                .HasOne(uv => uv.User)
                .WithMany()
                .HasForeignKey(uv => uv.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserVoucher>()
                .HasOne(uv => uv.Voucher)
                .WithMany(v => v.UserVouchers)
                .HasForeignKey(uv => uv.VoucherID)
                .OnDelete(DeleteBehavior.Restrict);

            // Mỗi user chỉ đổi 1 voucher (cùng VoucherID) đúng 1 lần
            modelBuilder.Entity<UserVoucher>()
                .HasIndex(uv => new { uv.UserID, uv.VoucherID })
                .IsUnique();
            //Feedback
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserID)
                .OnDelete(DeleteBehavior.SetNull);
            // ChatMessage
            modelBuilder.Entity<ChatMessage>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // SeedData cho Hạng thành viên
            modelBuilder.Entity<MembershipTier>().HasData(
                new MembershipTier { TierID = 1, TierName = "Tech-Newie", MinPoints = 0, ColorHex = "#e0e0e0", IconClass = "fa-solid fa-user" },
                new MembershipTier { TierID = 2, TierName = "Tech-Member", MinPoints = 300, ColorHex = "#f5c3a6", IconClass = "fa-solid fa-star" },
                new MembershipTier { TierID = 3, TierName = "Tech-VIP", MinPoints = 1500, ColorHex = "#f5d17e", IconClass = "fa-solid fa-crown" }
            );

            //  Mồi dữ liệu cho Ưu đãi
            modelBuilder.Entity<TierBenefit>().HasData(
                new TierBenefit { BenefitID = 1, TierID = 2, Description = "Tặng voucher 50K khi lên hạng", IconClass = "fa-solid fa-ticket" },
                new TierBenefit { BenefitID = 2, TierID = 2, Description = "Giảm thêm 0.5% khi mua linh kiện PC", IconClass = "fa-solid fa-gift" }
            );

            // ==== Category (5 danh mục) ====

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, CategoryName = "CPU", Description = "Bộ vi xử lý trung tâm" },
                new Category { CategoryID = 2, CategoryName = "VGA", Description = "Card đồ họa" },
                new Category { CategoryID = 3, CategoryName = "RAM", Description = "Bộ nhớ truy cập ngẫu nhiên" },
                new Category { CategoryID = 4, CategoryName = "SSD", Description = "Ổ cứng thể rắn" },
                new Category { CategoryID = 5, CategoryName = "Màn hình", Description = "Màn hình máy tính" },
                new Category { CategoryID = 6, CategoryName = "Bàn phím", Description = "Bàn phím máy tính" },
                new Category { CategoryID = 7, CategoryName = "Chuột", Description = "Chuột máy tính" }
            );

            // Seed thử 1-2 voucher để test
            modelBuilder.Entity<Voucher>().HasData(
                new Voucher
                {
                    VoucherID = 1,
                    Code = "WELCOME50",
                    Title = "Giảm 50.000đ",
                    Description = "Chào mừng thành viên mới",
                    DiscountType = "Fixed",
                    DiscountValue = 50000,
                    MinOrderValue = 200000,
                    ExpiryDate = new DateTime(2026, 12, 31),
                    UsageLimit = null,
                    UsedCount = 0,
                    IsActive = true
                },
                new Voucher
                {
                    VoucherID = 2,
                    Code = "SALE10",
                    Title = "Giảm 10%",
                    Description = "Áp dụng cho đơn hàng từ 500.000đ",
                    DiscountType = "Percent",
                    DiscountValue = 10,
                    MaxDiscountAmount = 100000,
                    MinOrderValue = 500000,
                    ExpiryDate = new DateTime(2026, 12, 31),
                    UsageLimit = 1000,
                    UsedCount = 0,
                    IsActive = true
                }
            );

            // ==== SEED: Product (7 sản phẩm - dùng cho cả Deal + New Arrivals qua cờ IsNew/OldPrice) ====

                    modelBuilder.Entity<Product>().HasData(
            new Product
            {
                ProductID = 1,
                ProductName = "CPU Intel Core i5-13400F",
                CategoryID = 1,
                Price = 4290000,
                OldPrice = 4690000,
                Rating = 4.8,
                ReviewCount = 32,
                IsNew = false,
                ImageUrl = "https://cdn2.cellphones.com.vn/x/media/catalog/product/c/p/cpu-intel-core-i5-13400f-tray_2_.png",
                CreatedAt = new DateTime(2026, 6, 1),
                Description = "CPU Intel Core i5-13400F 10 nhân 16 luồng, tốc độ 2.5GHz, hỗ trợ RAM DDR5, socket LGA1700, không tích hợp đồ họa."
            },
            new Product
            {
                ProductID = 2,
                ProductName = "VGA GIGABYTE GeForce RTX 4060 Ti Super",
                CategoryID = 2,
                Price = 9990000,
                OldPrice = 11490000,
                Rating = 4.9,
                ReviewCount = 18,
                IsNew = true,
                ImageUrl = "https://product.hstatic.net/200000722513/product/9899_e1b98e8a5f63e754f57497ecf79488f5_292572e5779f431098d1236845acc095_fb8c3e65086e43388656e5b4ea0c138a_master.jpg",
                CreatedAt = new DateTime(2026, 7, 1),
                Description = "Card đồ họa Gigabyte RTX 4060 Ti Super 8GB GDDR6, 128-bit, hỗ trợ DLSS 3, Ray Tracing, xuất hình 4K."
            },
            new Product
            {
                ProductID = 3,
                ProductName = "RAM Kingston Fury Beast 16GB DDR5 6000MHz",
                CategoryID = 3,
                Price = 1590000,
                OldPrice = null,
                Rating = 4.7,
                ReviewCount = 45,
                IsNew = true,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQyHfoa5rzla1de4bt8URqa5v1qS-QumqoFRGh-FauusKr_2PsO1d8qleM&s=10",
                CreatedAt = new DateTime(2026, 7, 5),
                Description = "RAM Kingston Fury Beast 16GB (1x16GB) DDR5 6000MHz, CL40, hỗ trợ XMP 3.0, tản nhiệt màu đen."
            },
            new Product
            {
                ProductID = 4,
                ProductName = "SSD Samsung 990 PRO 2TB PCIe NVMe 4.0x4",
                CategoryID = 4,
                Price = 4850000,
                OldPrice = 5290000,
                Rating = 5.0,
                ReviewCount = 27,
                IsNew = true,
                ImageUrl = "https://bizweb.dktcdn.net/thumb/1024x1024/100/329/122/products/ssd-samsung-990-pro-pcie-gen-4-0-x4-nvme-with-heatsink-905954f6-13ab-4baf-8150-e682df092cb6.jpg?v=1696582447337",
                CreatedAt = new DateTime(2026, 7, 10),
                Description = "SSD Samsung 990 PRO 2TB, PCIe 4.0 x4, tốc độ đọc lên đến 7450 MB/s, ghi 6900 MB/s, hỗ trợ công nghệ V-NAND và tản nhiệt hiệu quả."
            },
            new Product
            {
                ProductID = 5,
                ProductName = "Màn hình LG UltraGear 27GR95QE-B 27\" OLED",
                CategoryID = 5,
                Price = 22990000,
                OldPrice = null,
                Rating = 4.9,
                ReviewCount = 12,
                IsNew = true,
                ImageUrl = "https://cdn2.cellphones.com.vn/x/media/catalog/product/m/a/man-hinh-lg-ultragear-oled-27gr95qe-b-27-inch-1.png",
                CreatedAt = new DateTime(2026, 7, 12),
                Description = "Màn hình LG UltraGear 27 inch OLED, độ phân giải 2560x1440, tần số quét 240Hz, thời gian phản hồi 0.03ms, hỗ trợ HDR10."
            },
            new Product
            {
                ProductID = 6,
                ProductName = "Bàn phím cơ Logitech G Pro X",
                CategoryID = 6,
                Price = 2490000,
                OldPrice = null,
                Rating = 4.6,
                ReviewCount = 20,
                IsNew = true,
                ImageUrl = "https://www.logitechg.com/content/dam/gaming/en/products/pro-x-keyboard/pro-x-keyboard-gallery-1.png",
                CreatedAt = new DateTime(2026, 7, 15),
                Description = "Bàn phím cơ Logitech G Pro X với switch GX Brown tùy chỉnh, đèn RGB, kích thước nhỏ gọn, chuyên cho game thủ."
            },
            new Product
            {
                ProductID = 7,
                ProductName = "Chuột gaming Logitech G502 HERO",
                CategoryID = 7,
                Price = 1290000,
                OldPrice = 1590000,
                Rating = 4.8,
                ReviewCount = 35,
                IsNew = true,
                ImageUrl = "https://product.hstatic.net/200000722513/product/10001_01736316d2b443d0838e5a0741434420_master.png",
                CreatedAt = new DateTime(2026, 7, 18),
                Description = "Chuột gaming Logitech G502 HERO với cảm biến HERO 25K, 11 nút lập trình, dây cáp, đèn RGB, trọng lượng có thể điều chỉnh."
            }
        );

            // ==== SEED: PrebuiltPc (3-4 bộ) ====

            modelBuilder.Entity<PrebuiltPc>().HasData(
                new PrebuiltPc { PrebuiltPcID = 1, Name = "Budget King", Price = 15990000, ImageUrl = "https://www.tncstore.vn/media/product/11995-pc-gaming-nova-a5070-bl--3-.jpg", IsBestSeller = false, DisplayOrder = 1 },
                new PrebuiltPc { PrebuiltPcID = 2, Name = "Mid-Range Beast", Price = 28500000, ImageUrl = "https://mygear.io.vn/media/product/7830-pc-gaming-cyberpower-rtx-5060-i5-12400f-u-1.png", IsBestSeller = true, DisplayOrder = 2 },
                new PrebuiltPc { PrebuiltPcID = 3, Name = "Ultimate Performance", Price = 95000000, ImageUrl = "https://breunor.com/cdn/shop/files/rossa_4649c942-d446-4369-9cc6-db35e9026377.jpg?v=1771404016&width=1024", IsBestSeller = false, DisplayOrder = 3 }
            );

            modelBuilder.Entity<PrebuiltPcSpec>().HasData(
                // Budget King
                new PrebuiltPcSpec { PrebuiltPcSpecID = 1, PrebuiltPcID = 1, SpecText = "Intel Core i5-12400F", DisplayOrder = 1 },
                new PrebuiltPcSpec { PrebuiltPcSpecID = 2, PrebuiltPcID = 1, SpecText = "RAM 16GB DDR4 3200MHz", DisplayOrder = 2 },
                new PrebuiltPcSpec { PrebuiltPcSpecID = 3, PrebuiltPcID = 1, SpecText = "VGA RTX 3060 12GB", DisplayOrder = 3 },
                new PrebuiltPcSpec { PrebuiltPcSpecID = 4, PrebuiltPcID = 1, SpecText = "SSD 500GB NVMe", DisplayOrder = 4 },
                // Mid-Range Beast
                new PrebuiltPcSpec { PrebuiltPcSpecID = 5, PrebuiltPcID = 2, SpecText = "Intel Core i5-13600K", DisplayOrder = 1 },
                new PrebuiltPcSpec { PrebuiltPcSpecID = 6, PrebuiltPcID = 2, SpecText = "RAM 32GB DDR5 5600MHz", DisplayOrder = 2 },
                new PrebuiltPcSpec { PrebuiltPcSpecID = 7, PrebuiltPcID = 2, SpecText = "VGA RTX 4060 Ti 8GB", DisplayOrder = 3 },
                new PrebuiltPcSpec { PrebuiltPcSpecID = 8, PrebuiltPcID = 2, SpecText = "SSD 1TB NVMe Gen4", DisplayOrder = 4 },
                // Ultimate Performance
                new PrebuiltPcSpec { PrebuiltPcSpecID = 9, PrebuiltPcID = 3, SpecText = "Intel Core i9-14900K", DisplayOrder = 1 },
                new PrebuiltPcSpec { PrebuiltPcSpecID = 10, PrebuiltPcID = 3, SpecText = "RAM 64GB DDR5 6000MHz", DisplayOrder = 2 },
                new PrebuiltPcSpec { PrebuiltPcSpecID = 11, PrebuiltPcID = 3, SpecText = "VGA RTX 4090 24GB", DisplayOrder = 3 },
                new PrebuiltPcSpec { PrebuiltPcSpecID = 12, PrebuiltPcID = 3, SpecText = "SSD 2TB NVMe Gen4", DisplayOrder = 4 }
            );

            // ==== SEED: NewsArticle (3 bài) ====

            modelBuilder.Entity<NewsArticle>().HasData(
                new NewsArticle
                {
                    NewsArticleID = 1,
                    Category = "Kinh nghiệm",
                    Title = "Hướng dẫn chọn Card Màn Hình (VGA) phù hợp cho nhu cầu chơi game 2026",
                    Excerpt = "Lựa chọn card đồ họa luôn là câu hỏi đau đầu nhất khi build PC. Cùng tìm hiểu cách chọn VGA tối ưu...",
                    ImageUrl = "https://cdn2.fptshop.com.vn/unsafe/1920x0/filters:format(webp):quality(75)/nen_mua_card_man_hinh_nao_de_choi_game_2026_thumb_3323eef885.png",
                    PublishedAt = new DateTime(2026, 7, 1)
                },
                new NewsArticle
                {
                    NewsArticleID = 2,
                    Category = "Tin tức công nghệ",
                    Title = "Intel ra mắt dòng CPU thế hệ mới: Có thực sự đáng nâng cấp?",
                    Excerpt = "Đánh giá chi tiết hiệu năng nâng cấp của dòng xử lý mới nhất từ Intel và xem liệu nó có đáng để bạn xuống...",
                    ImageUrl = "https://cdn-media.sforum.vn/storage/app/media/trannghia/trannghia/1/intel-core-series-3-wildcat-lake-01.jpg",
                    PublishedAt = new DateTime(2026, 7, 8)
                },
                new NewsArticle
                {
                    NewsArticleID = 3,
                    Category = "Thủ thuật",
                    Title = "5 Cách tối ưu hóa hệ điều hành để chơi game mượt mà hơn",
                    Excerpt = "Những tinh chỉnh đơn giản trên hệ điều hành giúp tắt các dịch vụ không cần thiết, giải phóng RAM và tăng...",
                    ImageUrl = "https://kimlongcenter.com/upload/image/Thu%20Thuat/C%C3%A1ch%20t%E1%BB%91i%20%C6%B0u%20%C4%91%E1%BB%83%20ch%C6%A1i%20game%20m%C6%B0%E1%BB%A3t%20h%C6%A1n/c%C3%A1ch-t%E1%BB%91i-%C6%B0u-%C4%91%E1%BB%83-ch%C6%A1i-game-m%C6%B0%E1%BB%A3t-h%C6%A1n.jpg",
                    PublishedAt = new DateTime(2026, 7, 15)
                }
            );

            // ==== Seed dữ liệu mẫu trong OnModelCreating (Khu vực TP. Hồ Chí Minh và Hà Nội) ====

            modelBuilder.Entity<Store>().HasData(
            new Store
            {
                StoreID = 1,
                Name = "PC Master - Cầu Giấy",
                Address = "123 Xuân Thủy, Dịch Vọng Hậu, Cầu Giấy, Hà Nội",
                District = "Cầu Giấy",
                Phone = "024 1234 5678",
                OpenHours = "08:00 - 21:00",
                Latitude = 21.0368,
                Longitude = 105.7827,
                NameNormalized = VietnameseTextHelper.RemoveDiacritics("PC Master - Cầu Giấy"),
                AddressNormalized = VietnameseTextHelper.RemoveDiacritics("123 Xuân Thủy, Dịch Vọng Hậu, Cầu Giấy, Hà Nội"),
                DistrictNormalized = VietnameseTextHelper.RemoveDiacritics("Cầu Giấy")
            },
            new Store
            {
                StoreID = 2,
                Name = "PC Master - Đống Đa",
                Address = "45 Thái Hà, Trung Liệt, Đống Đa, Hà Nội",
                District = "Đống Đa",
                Phone = "024 8765 4321",
                OpenHours = "08:30 - 21:30",
                Latitude = 21.0136,
                Longitude = 105.8213,
                NameNormalized = VietnameseTextHelper.RemoveDiacritics("PC Master - Đống Đa"),
                AddressNormalized = VietnameseTextHelper.RemoveDiacritics("45 Thái Hà, Trung Liệt, Đống Đa, Hà Nội"),
                DistrictNormalized = VietnameseTextHelper.RemoveDiacritics("Đống Đa")
            },
            new Store
            {
                StoreID = 3,
                Name = "PC Master - Hai Bà Trưng",
                Address = "78 Lê Thanh Nghị, Bách Khoa, Hai Bà Trưng, Hà Nội",
                District = "Hai Bà Trưng",
                Phone = "024 2468 1357",
                OpenHours = "08:00 - 21:00",
                Latitude = 21.0021,
                Longitude = 105.8437,
                NameNormalized = VietnameseTextHelper.RemoveDiacritics("PC Master - Hai Bà Trưng"),
                AddressNormalized = VietnameseTextHelper.RemoveDiacritics("78 Lê Thanh Nghị, Bách Khoa, Hai Bà Trưng, Hà Nội"),
                DistrictNormalized = VietnameseTextHelper.RemoveDiacritics("Hai Bà Trưng")
            },
            new Store
            {
                StoreID = 4,
                Name = "PC Master - Hà Đông",
                Address = "234 Quang Trung, Hà Đông, Hà Nội",
                District = "Hà Đông",
                Phone = "024 1357 2468",
                OpenHours = "08:30 - 21:30",
                Latitude = 20.9717,
                Longitude = 105.7770,
                NameNormalized = VietnameseTextHelper.RemoveDiacritics("PC Master - Hà Đông"),
                AddressNormalized = VietnameseTextHelper.RemoveDiacritics("234 Quang Trung, Hà Đông, Hà Nội"),
                DistrictNormalized = VietnameseTextHelper.RemoveDiacritics("Hà Đông")
            },
            new Store
            {
                StoreID = 5,
                Name = "PC Master - Bình Thạnh",
                Address = "56 Điện Biên Phủ, Phường 15, Bình Thạnh, TP. Hồ Chí Minh",
                District = "Bình Thạnh",
                Phone = "028 3512 6789",
                OpenHours = "08:00 - 21:30",
                Latitude = 10.8031,
                Longitude = 106.7150,
                NameNormalized = VietnameseTextHelper.RemoveDiacritics("PC Master - Bình Thạnh"),
                AddressNormalized = VietnameseTextHelper.RemoveDiacritics("56 Điện Biên Phủ, Phường 15, Bình Thạnh, TP. Hồ Chí Minh"),
                DistrictNormalized = VietnameseTextHelper.RemoveDiacritics("Bình Thạnh")
            },
            new Store
            {
                StoreID = 6,
                Name = "PC Master - Quận 1",
                Address = "88 Nguyễn Huệ, Bến Nghé, Quận 1, TP. Hồ Chí Minh",
                District = "Quận 1",
                Phone = "028 3822 4567",
                OpenHours = "08:00 - 22:00",
                Latitude = 10.7756,
                Longitude = 106.7025,
                NameNormalized = VietnameseTextHelper.RemoveDiacritics("PC Master - Quận 1"),
                AddressNormalized = VietnameseTextHelper.RemoveDiacritics("88 Nguyễn Huệ, Bến Nghé, Quận 1, TP. Hồ Chí Minh"),
                DistrictNormalized = VietnameseTextHelper.RemoveDiacritics("Quận 1")
            }
            );
        }

        public override int SaveChanges()
        {
            NormalizeStoreFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            NormalizeStoreFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void NormalizeStoreFields()
        {
            var storeEntries = ChangeTracker.Entries<Store>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in storeEntries)
            {
                var store = entry.Entity;
                store.NameNormalized = VietnameseTextHelper.RemoveDiacritics(store.Name);
                store.AddressNormalized = VietnameseTextHelper.RemoveDiacritics(store.Address);
                store.DistrictNormalized = VietnameseTextHelper.RemoveDiacritics(store.District);
            }
        }
    }
}