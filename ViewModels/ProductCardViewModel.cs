namespace ECommerceWeb.ViewModels
{
    public class ProductCardViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string ImageUrl { get; set; }
        public string CategoryName { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsNew { get; set; }
        public bool IsFavorite { get; set; } // Thuộc tính mới này
        public string DetailUrl { get; set; }
        public int? DiscountPercent { get; set; } // Giả sử bạn tính toán ở mapper
    }
}