using Inventory_Api.Models.Entities;

namespace Inventory_Api.Models.Entities
{
    public class InventoryTransaction : BaseEntity // اطمینان از ارث بری از BaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!; // ناوبری
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!; // ناوبری
        public int Quantity { get; set; }
        public string TransactionType { get; set; } = string.Empty; // مثلاً "In", "Out"
        public int? CostCenterId { get; set; } // Nullable، فقط برای خروج استفاده می‌شود
        public CostCenter? CostCenter { get; set; } // ناوبری اختیاری
        public int UserId { get; set; } // کاربری که تراکنش را انجام داده
        public User User { get; set; } = null!; // ناوبری
        public string? Note { get; set; } // توضیحات
        // توجه: تاریخ ثبت توسط BaseEntity فراهم می‌شود (اگر شامل CreatedAt باشد)
    }
}