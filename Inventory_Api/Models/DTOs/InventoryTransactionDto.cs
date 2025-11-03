namespace Inventory_Api.Models.DTOs
{
    public class InventoryTransactionDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; // برای نمایش
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty; // برای نمایش
        public int Quantity { get; set; }
        public string TransactionType { get; set; } = string.Empty; // "In" یا "Out"
        public int? CostCenterId { get; set; }
        public string? CostCenterName { get; set; } // برای نمایش
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty; // برای نمایش
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } // از BaseEntity
    }

    public class CreateInventoryTransactionDto
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int Quantity { get; set; }
        public string TransactionType { get; set; } = string.Empty; // "In" یا "Out"
        public int? CostCenterId { get; set; } // فقط برای "Out"
        public string? Note { get; set; }
    }
}