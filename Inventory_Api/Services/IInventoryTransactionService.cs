using Inventory_Api.Models.DTOs;
using Inventory_Api.Models.Enums; // برای UserRole

namespace Inventory_Api.Services
{
    public interface IInventoryTransactionService
    {
        // تغییر: اضافه کردن userId و userRole
        Task<InventoryTransactionDto> CreateTransactionAsync(CreateInventoryTransactionDto createDto, int userId, UserRole userRole);
        // تغییر: اضافه کردن userId و userRole
        Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByProductAsync(int productId, int userId, UserRole userRole);
        // تغییر: اضافه کردن userId و userRole
        Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByWarehouseAsync(int warehouseId, int userId, UserRole userRole);
        // تغییر: اضافه کردن userId و userRole
        Task<IEnumerable<InventoryTransactionDto>> GetUserAccessibleTransactionsAsync(int userId, UserRole userRole);
        // تغییر: اضافه کردن userId و userRole
        Task<InventoryTransactionDto?> GetTransactionByIdAsync(int id, int userId, UserRole userRole);
        // می‌توانید متد های دیگری نیز اضافه کنید
    }
}