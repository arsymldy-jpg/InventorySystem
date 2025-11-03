namespace Inventory_Web.Models
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Name2 { get; set; } = string.Empty;
        public string PrimaryCode { get; set; } = string.Empty;
        public string Code2 { get; set; } = string.Empty;
        public string Code3 { get; set; } = string.Empty;
        public int ReorderPoint { get; set; }
        public int SafetyStock { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty; // فرض کنیم API این را ارسال می‌کند
        public int TotalStock { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Name2 { get; set; } = string.Empty;
        public string PrimaryCode { get; set; } = string.Empty;
        public string Code2 { get; set; } = string.Empty;
        public string Code3 { get; set; } = string.Empty;
        public int ReorderPoint { get; set; }
        public int SafetyStock { get; set; }
        public int BrandId { get; set; }
    }

    public class UpdateProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Name2 { get; set; } = string.Empty;
        public string PrimaryCode { get; set; } = string.Empty;
        public string Code2 { get; set; } = string.Empty;
        public string Code3 { get; set; } = string.Empty;
        public int ReorderPoint { get; set; }
        public int SafetyStock { get; set; }
        public int BrandId { get; set; }
        public bool IsActive { get; set; }
    }
}