using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.ViewModels
{
    public class FeedbackFormViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại")]
        [StringLength(150)]
        public string ContactInfo { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chủ đề góp ý")]
        public string Topic { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung phản hồi")]
        [StringLength(1000, ErrorMessage = "Nội dung không được vượt quá 1000 ký tự")]
        public string Content { get; set; }
    }
}