using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Repositories
{
    public class StockRepository : Repository<Stock>, IStockRepository
    {
        public StockRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Stock?> GetByProductAndWarehouseAsync(int productId, int warehouseId)
        {
            return await _dbSet
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId && s.IsActive);
        }

        public async Task<IEnumerable<Stock>> GetByProductIdAsync(int productId)
        {
            return await _dbSet
                .Include(s => s.Warehouse)
                .Where(s => s.ProductId == productId && s.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Stock>> GetStocksByWarehouseAsync(int warehouseId)
        {
            return await _dbSet
                .Include(s => s.Product)
                .Where(s => s.WarehouseId == warehouseId && s.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Stocks)
                .Include(p => p.Brand)
                .Where(p => p.IsActive &&
                           p.Stocks.Where(s => s.IsActive).Sum(s => s.Quantity) <= p.ReorderPoint)
                .ToListAsync();
        }

        public async Task<IEnumerable<BrandStockSummaryDto>> GetBrandStockSummaryAsync()
        {
            return await _context.Brands
                .Where(b => b.IsActive)
                .Select(b => new BrandStockSummaryDto
                {
                    BrandId = b.Id,
                    BrandName = b.Name,
                    TotalProducts = b.Products.Count(p => p.IsActive),
                    TotalStock = b.Products
                        .Where(p => p.IsActive)
                        .SelectMany(p => p.Stocks.Where(s => s.IsActive))
                        .Sum(s => s.Quantity),
                    LowStockProducts = b.Products
                        .Count(p => p.IsActive &&
                                   p.Stocks.Where(s => s.IsActive).Sum(s => s.Quantity) <= p.ReorderPoint)
                })
                .ToListAsync();
        }

        // اضافه کردن متد Update و SaveChangesAsync
        public void Update(Stock entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}