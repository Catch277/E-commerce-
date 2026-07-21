using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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
        public string IconClass { get; set; } = "bi-cpu";

        // Khai báo liên kết 1-Nhiều với Product
        [ValidateNever]
        public ICollection<Product> Products { get; set; }
    }
}