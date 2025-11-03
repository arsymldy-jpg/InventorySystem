using System.ComponentModel.DataAnnotations;

namespace Inventory_Api.Models.Entities
{
    public class Brand : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation Properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>(); // اضافه شده
    }
}