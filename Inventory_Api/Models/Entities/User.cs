// Inventory_Api/Models/Entities/User.cs (یا Inventory_Api_New/Models/Entities/User.cs)

using Inventory_Api.Models.Enums; // یا Inventory_Api_New.Models.Enums
// اگر از BaseEntity استفاده می‌کردی، اضافه کن
// using Inventory_Api.Models.Entities;

namespace Inventory_Api.Models.Entities // یا Inventory_Api_New.Models.Entities
{
    public class User :BaseEntity// تغییر: حذف : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PersonnelCode { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string? Email { get; set; } // اضافه شود اگر نبود
        public string PasswordHash { get; set; } = string.Empty; // اضافه شود اگر نبود
        public UserRole Role { get; set; }
        public DateTime? ExpiryDate { get; set; }

        // ناوبری‌ها
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<UserWarehouseAccess> WarehouseAccesses { get; set; } = new List<UserWarehouseAccess>();
    }
}