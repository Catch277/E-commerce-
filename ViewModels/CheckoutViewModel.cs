using System.ComponentModel.DataAnnotations;
using ECommerceWeb.Models;

namespace ECommerceWeb.ViewModels
{
    public class CheckoutViewModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(255)]
        public string ShippingAddress { get; set; } = string.Empty;

        // ---- Dữ liệu phục vụ tính phí ship qua GHN ----
        [Required(ErrorMessage = "Vui lòng chọn tỉnh/thành phố")]
        public string? ProvinceName { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn quận/huyện")]
        public int? DistrictId { get; set; }
        public string? DistrictName { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phường/xã")]
        public string? WardCode { get; set; }

        // Phí ship hiển thị cho người dùng xem trước (server sẽ tự tính lại khi submit)
        public decimal ShippingFee { get; set; } = 0;

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "COD";

        [StringLength(50)]
        public string? VoucherCode { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        // Chỉ phục vụ hiển thị UI, hiện tại KHÔNG xử lý xuất hóa đơn công ty
        public bool WantCompanyInvoice { get; set; } = false;

        // ---- Dữ liệu chỉ dùng để hiển thị (không bind khi submit) ----
        public List<CartItem> CartItems { get; set; } = new();
        public List<Voucher> AvailableVouchers { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal OriginalTotal { get; set; }
    }
}