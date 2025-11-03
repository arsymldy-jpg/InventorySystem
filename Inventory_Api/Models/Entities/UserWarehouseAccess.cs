namespace Inventory_Api.Models.Entities
{
    public class UserWarehouseAccess : BaseEntity
    {
        public bool CanModify { get; set; }
        public bool CanView { get; set; }

        // Foreign Keys
        public int UserId { get; set; }
        public int WarehouseId { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Warehouse Warehouse { get; set; }
    }
}