namespace ECommerceWeb.ViewModels
{
    public class ProductCardViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public double Rating { get; set; } = 5.0;
        public int ReviewCount { get; set; }
        public bool IsNew { get; set; }
        public int? DiscountPercent { get; set; }
        public string DetailUrl { get; set; } = "#";
    }
}
