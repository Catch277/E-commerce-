using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWeb.Models
{
    public class Voucher
    {
        [Key]
        public int VoucherID { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } // Mã nhập vào, ví dụ WELCOME50

        [Required]
        [StringLength(150)]
        public string Title { get; set; } // "Giảm 50.000đ"

        [StringLength(255)]
        public string Description { get; set; }

        // Kiểu giảm: "Percent" hoặc "Fixed"
        [Required]
        [StringLength(20)]
        public string DiscountType { get; set; } = "Fixed";

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        // Giảm tối đa (áp dụng khi DiscountType = Percent), null = không giới hạn
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxDiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinOrderValue { get; set; } = 0;

        public DateTime ExpiryDate { get; set; }

        // Tổng lượt sử dụng tối đa toàn hệ thống, null = không giới hạn
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public ICollection<UserVoucher> UserVouchers { get; set; }
    }
}