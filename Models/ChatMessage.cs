using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWeb.Models
{
    public class ChatMessage
    {
        [Key]
        public int ChatMessageID { get; set; }

        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        // "User" hoặc "Admin" - dùng string cho đơn giản, dễ mở rộng sau này
        [Required]
        [StringLength(20)]
        public string SenderType { get; set; } = "User";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}