namespace ECommerceWeb.Models
{
    public enum WarrantyStatus
    {
        Received = 1,       // Đã tiếp nhận
        Coordinating = 2,   // Đang điều phối
        Repairing = 3,      // Đang sửa
        Repaired = 4,       // Đã sửa xong
        Returned = 5        // Đã trả máy
    }

    public class WarrantyTicket
    {
        public int Id { get; set; }

        // Liên kết với user đang đăng nhập (nếu Identity)
        public string UserId { get; set; }

        public string WarrantyCode { get; set; }
        public string PhoneNumber { get; set; }

        public string ProductName { get; set; }
        public string IssueDescription { get; set; }

        public WarrantyStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
    }
}
