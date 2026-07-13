using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        // Khai báo liên kết 1-Nhiều với Product
        public ICollection<Product> Products { get; set; }
    }
}