using System.Collections.Generic;

namespace ECommerceWeb.ViewModels
{
    public class StoreItemViewModel
    {
        public int StoreID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string OpenHours { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class StoreLocatorViewModel
    {
        public string Keyword { get; set; }
        public List<StoreItemViewModel> Stores { get; set; } = new();

        // Cửa hàng đang được chọn để hiển thị trên bản đồ (mặc định = cửa hàng đầu tiên)
        public StoreItemViewModel SelectedStore { get; set; }
    }
}