namespace Inventory_Api.Models.DTOs
{
    public class UserWarehouseAccessDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserPersonnelCode { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseCode { get; set; }
        public bool CanModify { get; set; }
        public bool CanView { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GrantWarehouseAccessDto
    {
        public int UserId { get; set; }
        public int WarehouseId { get; set; }
        public bool CanModify { get; set; }
        public bool CanView { get; set; }
    }
}