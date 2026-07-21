using System;
using System.Collections.Generic;

namespace ECommerceWeb.ViewModels
{
    public class OrderHistoryItemViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public int TotalProducts { get; set; }

        // Lấy 1 ảnh sản phẩm đại diện để hiển thị thumbnail trong danh sách đơn hàng
        public string ThumbnailUrl { get; set; }

        // Tên sản phẩm đầu tiên
        public string FirstProductName { get; set; }
    }

    public class OrderHistoryViewModel
    {
        public List<OrderHistoryItemViewModel> Orders { get; set; } = new();

        // Trạng thái đang được lọc (null = tất cả)
        public string CurrentFilter { get; set; }

        // Khoảng thời gian lọc đơn hàng
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Phân trang
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;

        // Thông tin sidebar (tái sử dụng để hiển thị avatar/tên trong layout profile)
        public string FullName { get; set; }
    }
}