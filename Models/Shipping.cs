using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWeb.Models
{
    public class Shipping
    {
        [Key]
        public int ShippingID { get; set; }

        public int OrderID { get; set; }
        [ForeignKey("OrderID")]
        public Order Order { get; set; }

        [StringLength(100)]
        public string ShippingCompany { get; set; }

        [StringLength(100)]
        public string TrackingCode { get; set; }

        [StringLength(50)]
        public string ShippingStatus { get; set; } = "Đang xử lý";

        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}