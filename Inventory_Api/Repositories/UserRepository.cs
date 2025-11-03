using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;
using Inventory_Api.Models.Enums;

namespace Inventory_Api.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByPersonnelCodeAsync(string personnelCode)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.PersonnelCode == personnelCode);
        }

        public async Task<User?> GetByMobileNumberAsync(string mobileNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.MobileNumber == mobileNumber);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _dbSet.Where(u => u.Role == role && u.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbSet.Where(u => u.IsActive).ToListAsync();
        }

        public async Task<bool> IsPersonnelCodeUniqueAsync(string personnelCode, int? excludeUserId = null)
        {
            return !await _dbSet.AnyAsync(u =>
                u.PersonnelCode == personnelCode &&
                (!excludeUserId.HasValue || u.Id != excludeUserId.Value));
        }

        public async Task<bool> IsMobileNumberUniqueAsync(string mobileNumber, int? excludeUserId = null)
        {
            return !await _dbSet.AnyAsync(u =>
                u.MobileNumber == mobileNumber &&
                (!excludeUserId.HasValue || u.Id != excludeUserId.Value));
        }
    }
}