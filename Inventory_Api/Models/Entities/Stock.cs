using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Api.Models.Entities
{
    public class Stock : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int BrandId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }

        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }
    }
}