using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace ECommerceWeb.Models
{
    public class MembershipTier
    {
        [Key]
        public int TierID { get; set; }
        public string TierName { get; set; }
        public int MinPoints { get; set; }
        public string ColorHex { get; set; }
        public string IconClass { get; set; }

        public ICollection<User> Users { get; set; }
        public ICollection<TierBenefit> Benefits { get; set; }
    }
}