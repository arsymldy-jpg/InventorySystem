using System.ComponentModel.DataAnnotations;

namespace Inventory_Api.Models.Entities
{
    public class Warehouse : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(10)]
        public string? Code { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        public virtual ICollection<UserWarehouseAccess> UserAccesses { get; set; } = new List<UserWarehouseAccess>();
    }
}