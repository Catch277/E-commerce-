using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWeb.Models
{
    public class EmailLog
    {
        [Key]
        public int EmailID { get; set; }

        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }

        // OrderID có thể Null nếu email không liên kết với đơn hàng cụ thể
        public int? OrderID { get; set; }
        [ForeignKey("OrderID")]
        public Order Order { get; set; }

        [StringLength(255)]
        public string EmailSubject { get; set; }

        public string EmailContent { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string EmailStatus { get; set; } = "Đã gửi";
    }
}