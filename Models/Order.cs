using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWeb.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(255)]
        public string ShippingAddress { get; set; }

        [StringLength(50)]
        public string OrderStatus { get; set; } = "Chờ xác nhận";

        // ---- Thông tin khách hàng nhập tại checkout ----
        [StringLength(100)]
        public string CustomerName { get; set; } = "";

        [StringLength(20)]
        public string CustomerPhone { get; set; } = "";

        [StringLength(100)]
        public string CustomerEmail { get; set; } = "";

        [StringLength(20)]
        public string PaymentMethod { get; set; } = "COD";

        [StringLength(500)]
        public string Note { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        public int? VoucherID { get; set; }

        [ForeignKey(nameof(VoucherID))]
        public Voucher? Voucher { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}