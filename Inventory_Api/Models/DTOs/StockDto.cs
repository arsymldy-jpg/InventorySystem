namespace Inventory_Api.Models.DTOs
{
    public class StockDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductPrimaryCode { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AdjustStockDto
    {
        public int ProductId { get; set; }
        public int BrandId { get; set; }
        public int WarehouseId { get; set; }
        public int Quantity { get; set; }
        public string Action { get; set; } // "INCREASE", "DECREASE", "SET"
        public string? Reason { get; set; }
    }

    public class TransferStockDto
    {
        public int ProductId { get; set; }
        public int BrandId { get; set; }
        public int SourceWarehouseId { get; set; }
        public int DestinationWarehouseId { get; set; }
        public int Quantity { get; set; }
        public string? Reason { get; set; }
    }
}