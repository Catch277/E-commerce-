using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWeb.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackID { get; set; }

        public int? UserID { get; set; } // nullable phòng trường hợp cho phép khách vãng lai gửi sau này
        [ForeignKey("UserID")]
        public User? User { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(150)]
        public string ContactInfo { get; set; } // Email hoặc SĐT

        [Required]
        [StringLength(100)]
        public string Topic { get; set; } // "Chất lượng sản phẩm", "Vận chuyển"...

        [Required]
        [StringLength(2000)]
        public string Content { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Chờ xử lý"; // Chờ xử lý / Đã tiếp nhận / Đã xử lý

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}