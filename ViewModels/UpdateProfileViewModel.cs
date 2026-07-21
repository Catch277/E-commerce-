using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.ViewModels
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string FullName { get; set; }

        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "Số điện thoại phải gồm 9 chữ số (sau +84)")]
        public string Phone { get; set; }

        public string Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(255)]
        public string Address { get; set; }
    }
}