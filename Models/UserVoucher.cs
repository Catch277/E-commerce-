using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWeb.Models
{
    public class UserVoucher
    {
        [Key]
        public int UserVoucherID { get; set; }

        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }

        public int VoucherID { get; set; }
        [ForeignKey("VoucherID")]
        public Voucher Voucher { get; set; }

        public DateTime RedeemedAt { get; set; } = DateTime.Now;

        // Đã dùng trong đơn hàng nào chưa (null = chưa dùng)
        public int? UsedInOrderID { get; set; }
    }
}