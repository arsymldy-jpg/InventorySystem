using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Services
{
    public interface IUserWarehouseAccessService
    {
        Task<UserWarehouseAccessDto> GrantAccessAsync(GrantWarehouseAccessDto grantDto, int currentUserId);
        Task<bool> RevokeAccessAsync(int userId, int warehouseId, int currentUserId); // تغییر از void به Task<bool>
        Task<IEnumerable<UserWarehouseAccessDto>> GetUserAccessesAsync(int userId);
        Task<IEnumerable<UserWarehouseAccessDto>> GetWarehouseAccessesAsync(int warehouseId);
        Task<bool> CanUserAccessWarehouseAsync(int userId, int warehouseId); // معادل HasViewAccess
        Task<bool> CanUserModifyWarehouseAsync(int userId, int warehouseId); // معادل HasEditAccess
        // متد جدید برای گرفتن لیست انبارهای قابل دسترسی
        Task<IEnumerable<int>> GetUserAccessibleWarehouseIdsAsync(int userId, bool includeViewOnly = false);
    }
}