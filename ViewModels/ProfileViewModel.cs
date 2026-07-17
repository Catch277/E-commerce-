using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.ViewModels
{
    public class ProfileViewModel
    {
        public int ProfileId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public string Role { get; set; }
        public string MemberTier { get; set; }
    }
}