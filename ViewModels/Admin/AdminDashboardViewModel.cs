namespace ECommerceWeb.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }

        public int TotalCategories { get; set; }

        public int TotalUsers { get; set; }

        public int TotalOrders { get; set; }

        public int LowStockProducts { get; set; }
        public List<ECommerceWeb.Models.Product> LowStockProductList { get; set; } = new();

    }
}