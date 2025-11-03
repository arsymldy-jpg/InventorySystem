namespace Inventory_Api.Models.DTOs
{
    public class BrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductsCount { get; set; }
    }

    public class CreateBrandDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateBrandDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        //public bool IsActive { get; set; }
    }
}