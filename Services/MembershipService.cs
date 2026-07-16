using ECommerceWeb.Data;
using ECommerceWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWeb.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly ApplicationDbContext _context;

        public MembershipService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddPointsAsync(int userId, int orderId, decimal totalAmount)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Tính điểm: 1 điểm = 10.000đ
            int earnedPoints = (int)Math.Floor(totalAmount / 10000);
            if (earnedPoints <= 0) return true;

            user.Points += earnedPoints;

            // Lưu lịch sử
            _context.PointsHistories.Add(new PointsHistory
            {
                UserID = userId,
                OrderID = orderId,
                PointsChange = earnedPoints,
                Reason = $"Tích điểm từ đơn hàng #{orderId}"
            });

            // Kiểm tra thăng hạng (Lấy hạng cao nhất mà user đủ điểm)
            var eligibleTier = await _context.MembershipTiers
                .Where(t => t.MinPoints <= user.Points)
                .OrderByDescending(t => t.MinPoints)
                .FirstOrDefaultAsync();

            if (eligibleTier != null && user.TierID != eligibleTier.TierID)
            {
                user.TierID = eligibleTier.TierID;

                // Lưu lịch sử lên hạng
                _context.PointsHistories.Add(new PointsHistory
                {
                    UserID = userId,
                    PointsChange = 0,
                    Reason = $"Thăng hạng lên {eligibleTier.TierName}"
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}