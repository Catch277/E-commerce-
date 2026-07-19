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
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
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

            // 2. Mồi dữ liệu (Seed Data) cho Hạng thành viên
            modelBuilder.Entity<MembershipTier>().HasData(
                new MembershipTier { TierID = 1, TierName = "Tech-Newie", MinPoints = 0, ColorHex = "#e0e0e0", IconClass = "fa-solid fa-user" },
                new MembershipTier { TierID = 2, TierName = "Tech-Member", MinPoints = 300, ColorHex = "#f5c3a6", IconClass = "fa-solid fa-star" },
                new MembershipTier { TierID = 3, TierName = "Tech-VIP", MinPoints = 1500, ColorHex = "#f5d17e", IconClass = "fa-solid fa-crown" }
            );

            // 3. Mồi dữ liệu cho Ưu đãi (gắn với S-NEW)
            modelBuilder.Entity<TierBenefit>().HasData(
                new TierBenefit { BenefitID = 1, TierID = 2, Description = "Tặng voucher 50K khi lên hạng", IconClass = "fa-solid fa-ticket" },
                new TierBenefit { BenefitID = 2, TierID = 2, Description = "Giảm thêm 0.5% khi mua linh kiện PC", IconClass = "fa-solid fa-gift" }
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