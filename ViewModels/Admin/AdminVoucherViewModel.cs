using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.ViewModels.Admin
{
    public class AdminVoucherViewModel
    {
        public int VoucherID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã khuyến mãi.")]
        [StringLength(
            50,
            ErrorMessage = "Mã khuyến mãi không được vượt quá 50 ký tự.")]
        [Display(Name = "Mã khuyến mãi")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên chương trình.")]
        [StringLength(
            150,
            ErrorMessage = "Tên chương trình không được vượt quá 150 ký tự.")]
        [Display(Name = "Tên chương trình")]
        public string Title { get; set; } = string.Empty;

        [StringLength(
            255,
            ErrorMessage = "Mô tả không được vượt quá 255 ký tự.")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại giảm giá.")]
        [Display(Name = "Loại giảm giá")]
        public string DiscountType { get; set; } = "Fixed";

        [Required(ErrorMessage = "Vui lòng nhập giá trị giảm.")]
        [Range(
            0.01,
            1000000000,
            ErrorMessage = "Giá trị giảm phải lớn hơn 0.")]
        [Display(Name = "Giá trị giảm")]
        public decimal DiscountValue { get; set; }

        [Range(
            0,
            1000000000,
            ErrorMessage = "Mức giảm tối đa không hợp lệ.")]
        [Display(Name = "Mức giảm tối đa")]
        public decimal? MaxDiscountAmount { get; set; }

        [Range(
            0,
            1000000000,
            ErrorMessage = "Giá trị đơn tối thiểu không hợp lệ.")]
        [Display(Name = "Giá trị đơn tối thiểu")]
        public decimal MinOrderValue { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày hết hạn.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày hết hạn")]
        public DateTime ExpiryDate { get; set; }
            = DateTime.Today.AddDays(30);

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Giới hạn lượt dùng phải lớn hơn 0.")]
        [Display(Name = "Giới hạn lượt sử dụng")]
        public int? UsageLimit { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}