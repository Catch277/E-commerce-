using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [RegularExpression("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$", ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        public string? Address { get; set; }

        [RegularExpression(@"^\+84[0-9]{9}$", ErrorMessage = "Số điện thoại phải gồm 9 chữ số theo sau")]
        public string? Phone { get; set; }
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Mật khẩu phải trên 6 ký tự", MinimumLength = 6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }
}
