using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;

namespace Inventory_Api.Repositories
{
    public class InventoryTransactionRepository : Repository<InventoryTransaction>, IInventoryTransactionRepository
    {
        // اضافه کردن یک فیلد برای دسترسی به DbContext
        private readonly ApplicationDbContext _context;
        public InventoryTransactionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context; // ذخیره مرجع DbContext
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByProductAsync(int productId)
        {
            return await _context.InventoryTransactions
                .Where(t => t.ProductId == productId)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByWarehouseAsync(int warehouseId)
        {
            return await _context.InventoryTransactions
                .Where(t => t.WarehouseId == warehouseId)
                .ToListAsync();
        }

        // اضافه کردن متد Update و SaveChangesAsync
        public void Update(InventoryTransaction entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}