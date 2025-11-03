using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;

namespace Inventory_Api.Repositories
{
    public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
    {
        public WarehouseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Warehouse?> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(w => w.Code == code && w.IsActive);
        }

        public async Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync()
        {
            return await _dbSet
                .Where(w => w.IsActive)
                .ToListAsync();
        }

        public async Task<bool> HasActiveStocksAsync(int warehouseId)
        {
            return await _context.Stocks
                .AnyAsync(s => s.WarehouseId == warehouseId && s.IsActive && s.Quantity > 0);
        }
    }
}