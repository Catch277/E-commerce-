using ECommerceWeb.Models;

namespace ECommerceWeb.ViewModels.Admin
{
    public class AdminOrderDetailViewModel
    {
        public Order Order { get; set; } = null!;

        public Payment? Payment { get; set; }

        public Shipping? Shipping { get; set; }

        public List<string> AllowedNextStatuses { get; set; } = new();
    }
}