using Inventory_Api.Models.Entities;
using Inventory_Api.Models.Enums;

namespace Inventory_Api.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByPersonnelCodeAsync(string personnelCode);
        Task<User?> GetByMobileNumberAsync(string mobileNumber);
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<bool> IsPersonnelCodeUniqueAsync(string personnelCode, int? excludeUserId = null);
        Task<bool> IsMobileNumberUniqueAsync(string mobileNumber, int? excludeUserId = null);
    }
}