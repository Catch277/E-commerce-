using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.ViewModels
{
    public class CheckoutViewModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = string.Empty;
    }
}