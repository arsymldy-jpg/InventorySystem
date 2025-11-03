using Inventory_Web.Models.Enums; // اگر Enums در یک پوشه جدا در Inventory_Web نیز تعریف شود، یا از Inventory_Api ارجاع داده شود

namespace Inventory_Web.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PersonnelCode { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        // توجه: PasswordHash نباید در DTO خروجی (برای امنیت) وجود داشته باشد
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiryDate { get; set; }
        // دیگر فیلدها بر اساس نیاز
    }

    public class CreateUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PersonnelCode { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Password { get; set; } = string.Empty; // در درخواست ایجاد، رمز لازم است
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiryDate { get; set; }
    }

    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiryDate { get; set; }
        // توجه: معمولاً رمز در ویرایش کاربر ارسال نمی‌شود، مگر برای تغییر خود کاربر یا توسط مدیر خاص
    }
}