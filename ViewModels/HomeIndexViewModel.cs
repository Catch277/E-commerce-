using System.Collections.Generic;

namespace ECommerceWeb.ViewModels
{
    public class PrebuiltPcViewModel
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Specs { get; set; } = new();
        public decimal Price { get; set; }
        public bool IsBestSeller { get; set; }
        public string DetailUrl { get; set; } = "#";
    }

    public class NewsArticleViewModel
    {
        public string Category { get; set; }
        public string Title { get; set; }
        public string Excerpt { get; set; }
        public string ImageUrl { get; set; }
        public string DetailUrl { get; set; } = "#";
    }

    public class HomeIndexViewModel
    {
        public List<ProductCardViewModel> DealProducts { get; set; } = new();
        public List<ProductCardViewModel> NewArrivals { get; set; } = new();
        public List<PrebuiltPcViewModel> PrebuiltPcs { get; set; } = new();
        public List<NewsArticleViewModel> NewsArticles { get; set; } = new();
    }
}
