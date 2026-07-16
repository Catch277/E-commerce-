using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.Models
{
    public class TierBenefit
    {
        [Key]
        public int BenefitID { get; set; }
        public int TierID { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }

        public MembershipTier Tier { get; set; }
    }
}