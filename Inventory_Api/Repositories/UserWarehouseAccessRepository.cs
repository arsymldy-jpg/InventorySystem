using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;

namespace Inventory_Api.Repositories
{
    public class UserWarehouseAccessRepository : Repository<UserWarehouseAccess>, IUserWarehouseAccessRepository
    {
        public UserWarehouseAccessRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<UserWarehouseAccess?> GetByUserAndWarehouseAsync(int userId, int warehouseId)
        {
            return await _dbSet
                .Include(ua => ua.User)
                .Include(ua => ua.Warehouse)
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.WarehouseId == warehouseId);
            // حذف شرط && ua.IsActive برای پیدا کردن همه دسترسی‌ها (فعال و غیرفعال)
        }

        public async Task<UserWarehouseAccess?> GetActiveByUserAndWarehouseAsync(int userId, int warehouseId)
{
    return await _dbSet
        .Include(ua => ua.User)
        .Include(ua => ua.Warehouse)
        .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.WarehouseId == warehouseId && ua.IsActive);
}

        public async Task<IEnumerable<UserWarehouseAccess>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(ua => ua.Warehouse)
                .Where(ua => ua.UserId == userId && ua.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserWarehouseAccess>> GetByWarehouseIdAsync(int warehouseId)
        {
            return await _dbSet
                .Include(ua => ua.User)
                .Where(ua => ua.WarehouseId == warehouseId && ua.IsActive)
                .ToListAsync();
        }

        public async Task<bool> UserCanAccessWarehouseAsync(int userId, int warehouseId)
        {
            return await _dbSet.AnyAsync(ua =>
                ua.UserId == userId &&
                ua.WarehouseId == warehouseId &&
                ua.IsActive &&
                ua.CanView);
        }

        public async Task<bool> UserCanModifyWarehouseAsync(int userId, int warehouseId)
        {
            return await _dbSet.AnyAsync(ua =>
                ua.UserId == userId &&
                ua.WarehouseId == warehouseId &&
                ua.IsActive &&
                ua.CanModify);
        }

        public async Task<bool> RemoveUserAccessAsync(int userId, int warehouseId)
        {
            var access = await GetByUserAndWarehouseAsync(userId, warehouseId);
            if (access != null)
            {
                access.IsActive = false;
                access.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(access);
                return true; // عملیات موفق
            }
            return false; // دسترسی وجود نداشت
        }
    }
}