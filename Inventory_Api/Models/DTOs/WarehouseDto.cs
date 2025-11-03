namespace Inventory_Api.Models.DTOs
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int StockItemsCount { get; set; }
    }

    public class CreateWarehouseDto
    {
        public string Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Code { get; set; }
    }

    public class UpdateWarehouseDto
    {
        public string Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; }
    }
}