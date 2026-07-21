using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWeb.Models
{
    public class Favorite
    {
        public int UserID { get; set; }
        public int ProductID { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }

        [ForeignKey("ProductID")]
        public Product Product { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}