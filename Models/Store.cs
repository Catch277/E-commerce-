using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.Models
{
    public class Store
    {
        [Key]
        public int StoreID { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(100)]
        public string District { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(50)]
        public string OpenHours { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public bool IsActive { get; set; } = true;

        // ---- Cột chuẩn hóa phục vụ search không dấu, không phân biệt hoa/thường ----
        // Được tự động tính lại mỗi khi lưu (xem ApplicationDbContext.SaveChanges override)
        [StringLength(150)]
        public string NameNormalized { get; set; }

        [StringLength(255)]
        public string AddressNormalized { get; set; }

        [StringLength(100)]
        public string DistrictNormalized { get; set; }
    }
}