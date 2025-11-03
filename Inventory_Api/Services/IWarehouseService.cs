using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Services
{
    public interface IWarehouseService
    {
        // متدهای موجود
        Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync();
        Task<IEnumerable<WarehouseDto>> GetActiveWarehousesAsync();
        Task<WarehouseDto?> GetWarehouseByIdAsync(int id);
        Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseDto createWarehouseDto, int currentUserId);
        Task<WarehouseDto?> UpdateWarehouseAsync(int id, UpdateWarehouseDto updateWarehouseDto, int currentUserId);
        Task<bool> DeleteWarehouseAsync(int id, int currentUserId);

        // متدهای جدید برای دسترسی مبتنی بر کاربر
        Task<IEnumerable<WarehouseDto>> GetAccessibleWarehousesAsync(int userId);
        Task<IEnumerable<WarehouseDto>> GetAccessibleActiveWarehousesAsync(int userId);
        Task<WarehouseDto?> GetAccessibleWarehouseByIdAsync(int id, int userId);
    }
}