using Microsoft.EntityFrameworkCore;
using ECommerceWeb.Models;

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
        }
    }
}