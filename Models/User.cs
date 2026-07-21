using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        [Required]
        [StringLength(100)]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Role { get; set; } = "Customer";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int Points { get; set; } = 0; // Điểm hiện tại
        public int TierID { get; set; } = 1; // Mặc định hạng 1

        public MembershipTier Tier { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? PasswordUpdatedAt { get; set; }
    }
}