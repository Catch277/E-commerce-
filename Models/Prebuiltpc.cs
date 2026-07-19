using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWeb.Models
{
    public class PrebuiltPc
    {
        [Key]
        public int PrebuiltPcID { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsBestSeller { get; set; } = false;

        public int DisplayOrder { get; set; }

        public ICollection<PrebuiltPcSpec> Specs { get; set; }
    }

    public class PrebuiltPcSpec
    {
        [Key]
        public int PrebuiltPcSpecID { get; set; }

        public int PrebuiltPcID { get; set; }
        [ForeignKey("PrebuiltPcID")]
        public PrebuiltPc PrebuiltPc { get; set; }

        [Required]
        [StringLength(150)]
        public string SpecText { get; set; } // "Intel Core i5-12400F"

        public int DisplayOrder { get; set; }
    }
}