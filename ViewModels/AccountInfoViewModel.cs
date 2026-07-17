using System;
using System.Collections.Generic;

namespace ECommerceWeb.ViewModels
{
    public class AddressItemViewModel
    {
        public int AddressID { get; set; }
        public string Label { get; set; }        // "Home", "Công ty"...
        public string TypeTag { get; set; }       // "Nhà", "Văn phòng"
        public bool IsDefault { get; set; }
        public string ReceiverName { get; set; }
        public string Phone { get; set; }
        public string FullAddress { get; set; }
    }

    public class VoucherItemViewModel
    {
        public int VoucherID { get; set; }
        public string Title { get; set; }         // "Giảm 50.000đ"
        public string Description { get; set; }
        public string Code { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class LinkedAccountItemViewModel
    {
        public string ProviderName { get; set; }  // "Google", "Zalo"
        public string IconClass { get; set; }
        public string IconColorHex { get; set; }
        public bool IsLinked { get; set; }
    }

    public class AccountInfoViewModel
    {
        // ---- Thông tin cá nhân ----
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string PhoneMasked { get; set; }   // 091*****00
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string DefaultAddress { get; set; }

        // ---- Tổng quan / hạng thành viên ----
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public string CurrentTierCode { get; set; }
        public string CurrentTierName { get; set; }
        public bool IsStudentVerified { get; set; }
        public DateTime? TierRefreshDate { get; set; }
        public decimal AmountToNextTier { get; set; }
        public string NextTierCode { get; set; }

        // ---- Các block dữ liệu con ----
        public List<AddressItemViewModel> Addresses { get; set; } = new();
        public List<VoucherItemViewModel> Vouchers { get; set; } = new();
        public List<LinkedAccountItemViewModel> LinkedAccounts { get; set; } = new();

        public DateTime? PasswordLastUpdated { get; set; }
    }
}