namespace Inventory_Api.Models.Entities
{
    public class CostCenter:BaseEntity
    {
        public string Name { get; set; } // مثلاً: تعمیرات، پروژه A، تولید خط 1 و ...
        public string? Description { get; set; }
    }
}
