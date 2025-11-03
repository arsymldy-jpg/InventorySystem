using Inventory_Api.Data;
using Inventory_Api.Models.Entities;
using Inventory_Api.Models.Enums;

namespace Inventory_Api.Services
{
    public class DatabaseInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if database already has data
            if (context.Users.Any())
            {
                return; // DB has been seeded
            }

            var passwordService = new PasswordService();

            // Create default admin user
            var adminUser = new User
            {
                FirstName = "Admin",
                LastName = "System",
                PersonnelCode = "ADMIN001",
                MobileNumber = "09123456789",
                Email = "admin@inventory.com",
                PasswordHash = passwordService.HashPassword("Admin123!"), // In production, use a stronger password
                Role = UserRole.Admin,
                ExpiryDate = null,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            context.SaveChanges();

            // Create default warehouse
            var mainWarehouse = new Warehouse
            {
                Name = "انبار اصلی",
                Code = "WH001",
                Address = "آدرس انبار اصلی",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Warehouses.Add(mainWarehouse);
            context.SaveChanges();

            // Create default brand
            var defaultBrand = new Brand
            {
                Name = "برند پیش‌فرض",
                Description = "برند پیش‌فرض سیستم",
                CreatedAt = DateTime.UtcNow
            };

            context.Brands.Add(defaultBrand);
            context.SaveChanges();
        }
    }
}