using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWeb.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }
        [Required]
        public string ProductName { get; set; }
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        // Giá gốc trước giảm - null nếu không giảm giá
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }

        // Khóa ngoại
        public int? CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public Category Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public double Rating { get; set; } = 5.0;
        public int ReviewCount { get; set; } = 0;

        public bool IsNew { get; set; } = false;
    }
}