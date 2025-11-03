namespace Inventory_Api.Models.DTOs
{
    public class InventorySummaryDto
    {
        public int TotalProducts { get; set; }
        public int TotalBrands { get; set; }
        public int TotalWarehouses { get; set; }
        public int TotalStockValue { get; set; } // می‌تواند بعداً به ارزش مالی تبدیل شود
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class StockMovementReportDto
    {
        public DateTime Date { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string WarehouseName { get; set; }
        public string Action { get; set; }
        public int Quantity { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
    }

    public class WarehouseStockSummaryDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseCode { get; set; }
        public int TotalProducts { get; set; }
        public int TotalStock { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
    }

    public class ProductStockHistoryDto
    {
        public DateTime Date { get; set; }
        public string WarehouseName { get; set; }
        public string Action { get; set; }
        public int Quantity { get; set; }
        public int NewBalance { get; set; }
        public string UserName { get; set; }
        public string Notes { get; set; }
    }

    public class BrandComparisonReportDto
    {
        public List<BrandStockInfo> BrandStocks { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class BrandStockInfo
    {
        public string BrandName { get; set; }
        public int ProductCount { get; set; }
        public int TotalStock { get; set; }
        public int AverageStock { get; set; }
        public int LowStockProducts { get; set; }
    }

    public class BrandStockSummaryDto
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int TotalProducts { get; set; }
        public int TotalStock { get; set; }
        public int LowStockProducts { get; set; }
    }
}