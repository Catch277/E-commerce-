using System;
using System.Collections.Generic;
using ECommerceWeb.Models;

namespace ECommerceWeb.ViewModels
{
    public class MembershipVM
    {
        public int CurrentPoints { get; set; }
        public MembershipTier CurrentTier { get; set; }
        public MembershipTier NextTier { get; set; }
        public List<MembershipTier> AllTiers { get; set; }

        // Tiến độ thanh Progress Bar (phần trăm)
        public double ProgressPercentage
        {
            get
            {
                if (NextTier == null || CurrentTier == null) return 100;

                var range = NextTier.MinPoints - CurrentTier.MinPoints;
                if (range <= 0) return 100; // tránh chia cho 0 nếu dữ liệu tier bị trùng MinPoints

                var currentProgress = CurrentPoints - CurrentTier.MinPoints;
                var percent = (double)currentProgress / range * 100;

                // Kẹp giá trị trong khoảng 0-100 để tránh progress bar tràn/âm
                return percent < 0 ? 0 : (percent > 100 ? 100 : percent);
            }
        }

        // Số điểm còn thiếu để lên hạng kế tiếp, dùng trực tiếp trong View
        public int PointsToNextTier => NextTier == null ? 0 : Math.Max(0, NextTier.MinPoints - CurrentPoints);
    }
}