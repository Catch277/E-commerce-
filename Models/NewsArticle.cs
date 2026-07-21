using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.Models
{
    public class NewsArticle
    {
        [Key]
        public int NewsArticleID { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } // "Kinh nghiệm", "Tin tức công nghệ"...

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Excerpt { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }

        public DateTime PublishedAt { get; set; } = DateTime.Now;

        public bool IsPublished { get; set; } = true;
    }
}