using System.ComponentModel.DataAnnotations;

namespace Inventory_Api.Models.Entities
{
    public class Product : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string? Name2 { get; set; }

        [Required]
        [StringLength(50)]
        public string PrimaryCode { get; set; }

        [StringLength(50)]
        public string? Code2 { get; set; }

        [StringLength(50)]
        public string? Code3 { get; set; }

        public int TotalStock { get; set; }

        public int ReorderPoint { get; set; }

        public int SafetyStock { get; set; }

        // Foreign Keys
        public int BrandId { get; set; }

        // Navigation Properties
        public virtual Brand Brand { get; set; }
        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    }
}