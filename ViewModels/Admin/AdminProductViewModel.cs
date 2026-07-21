using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceWeb.ViewModels.Admin
{
    public class AdminProductViewModel
    {
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự.")]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm.")]
        [Range(0, 500000000, ErrorMessage = "Giá sản phẩm không hợp lệ.")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(0, 100000, ErrorMessage = "Số lượng không được âm.")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Display(Name = "Đường dẫn hình ảnh")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        [Display(Name = "Danh mục")]
        public int? CategoryID { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }
            = Enumerable.Empty<SelectListItem>();
    }
}