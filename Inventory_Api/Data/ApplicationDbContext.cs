using Microsoft.EntityFrameworkCore; // حذف Microsoft.AspNetCore.Identity.EntityFrameworkCore
using Inventory_Api.Models.Entities; // یا Inventory_Api_New.Models.Entities

namespace Inventory_Api.Data // یا Inventory_Api_New.Data
{
    // تغییر: ارث بری از DbContext به جای IdentityDbContext<User>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // اضافه کردن DbSet<User> به صورت دستی
        public DbSet<User> Users { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<UserWarehouseAccess> UserWarehouseAccesses { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        // اضافه کردن این دو خط جدید:
        public DbSet<CostCenter> CostCenters { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration (همان تنظیمات قبلی برای فیلدهای یکتا)
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.PersonnelCode).IsUnique();
                entity.HasIndex(u => u.MobileNumber).IsUnique();
                // اگر Email یک فیلد جدا داری، می‌توانی ایندکس بگذاری
                // entity.HasIndex(u => u.Email).IsUnique();
            });

            // ... (بقیه تنظیمات OnModelCreating همان قبلی)

            // Product configuration (همان تنظیمات قبلی)
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.PrimaryCode).IsUnique();
                entity.HasIndex(p => p.Code2).IsUnique();
                entity.HasIndex(p => p.Code3).IsUnique();

                entity.HasOne(p => p.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(p => p.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Stock configuration - Unique constraint for Product/Brand/Warehouse combination (همان تنظیمات قبلی)
            modelBuilder.Entity<Stock>(entity =>
            {
                entity.HasIndex(s => new { s.ProductId, s.BrandId, s.WarehouseId }).IsUnique();

                entity.HasOne(s => s.Product)
                      .WithMany(p => p.Stocks)
                      .HasForeignKey(s => s.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Brand)
                      .WithMany(b => b.Stocks)
                      .HasForeignKey(s => s.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Warehouse)
                      .WithMany(w => w.Stocks)
                      .HasForeignKey(s => s.WarehouseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UserWarehouseAccess configuration (همان تنظیمات قبلی)
            modelBuilder.Entity<UserWarehouseAccess>(entity =>
            {
                entity.HasIndex(ua => new { ua.UserId, ua.WarehouseId }).IsUnique();

                entity.HasOne(ua => ua.User)
                      .WithMany(u => u.WarehouseAccesses)
                      .HasForeignKey(ua => ua.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ua => ua.Warehouse)
                      .WithMany(w => w.UserAccesses)
                      .HasForeignKey(ua => ua.WarehouseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AuditLog configuration (همان تنظیمات قبلی)
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasOne(al => al.User)
                      .WithMany(u => u.AuditLogs)
                      .HasForeignKey(al => al.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Warehouse configuration (همان تنظیمات قبلی)
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasIndex(w => w.Code).IsUnique();
            });

            // اضافه کردن تنظیمات جدید برای CostCenter و InventoryTransaction
            // InventoryTransaction
            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                // رابطه با Product
                entity.HasOne(it => it.Product)
                      .WithMany() // یا ناوبری معکوس در Product اگر وجود داشت
                      .HasForeignKey(it => it.ProductId)
                      .OnDelete(DeleteBehavior.Cascade); // حذف تراکنش در صورت حذف کالا

                // رابطه با Warehouse
                entity.HasOne(it => it.Warehouse)
                      .WithMany() // یا ناوبری معکوس در Warehouse اگر وجود داشت
                      .HasForeignKey(it => it.WarehouseId)
                      .OnDelete(DeleteBehavior.Cascade); // حذف تراکنش در صورت حذف انبار

                // رابطه با CostCenter (اختیاری)
                entity.HasOne(it => it.CostCenter)
                      .WithMany() // یا ناوبری معکوس در CostCenter اگر وجود داشت
                      .HasForeignKey(it => it.CostCenterId)
                      .OnDelete(DeleteBehavior.SetNull); // یا Restrict

                // رابطه با User
                entity.HasOne(it => it.User)
                      .WithMany() // یا ناوبری معکوس در User اگر وجود داشت
                      .HasForeignKey(it => it.UserId)
                      .OnDelete(DeleteBehavior.NoAction); // جلوگیری از حذف کاربر در صورت وجود تراکنش
            });

            // CostCenter (اگر نیاز به تنظیمات خاصی داشت، اینجا اضافه می‌شود)
            // مثلاً ایندکس برای نام:
            modelBuilder.Entity<CostCenter>(entity =>
            {
                entity.HasIndex(cc => cc.Name).IsUnique(false); // یا true اگر منطق کسب و کار نیاز داشت
            });
        }
    }
}