using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.Models
{
    public class PointsHistory
    {
        [Key]
        public int PointsHistoryID { get; set; }
        public int UserID { get; set; }
        public int? OrderID { get; set; }
        public int PointsChange { get; set; } // Số điểm thay đổi (+ hoặc -)
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User User { get; set; }
    }
}