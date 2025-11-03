using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;

namespace Inventory_Api.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Product?> GetByPrimaryCodeAsync(string primaryCode)
        {
            return await _dbSet
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.PrimaryCode == primaryCode);
        }

        public async Task<Product?> GetByCode2Async(string code2)
        {
            return await _dbSet
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Code2 == code2);
        }

        public async Task<Product?> GetByCode3Async(string code3)
        {
            return await _dbSet
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Code3 == code3);
        }

        public async Task<IEnumerable<Product>> GetProductsByBrandAsync(int brandId)
        {
            return await _dbSet
                .Include(p => p.Brand)
                .Where(p => p.BrandId == brandId && p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _dbSet
                .Include(p => p.Brand)
                .Where(p => p.IsActive && (
                    p.Name.Contains(searchTerm) ||
                    p.Name2.Contains(searchTerm) ||
                    p.PrimaryCode.Contains(searchTerm) ||
                    p.Code2.Contains(searchTerm) ||
                    p.Code3.Contains(searchTerm) ||
                    p.Brand.Name.Contains(searchTerm)
                ))
                .ToListAsync();
        }

        public async Task<bool> IsPrimaryCodeUniqueAsync(string primaryCode, int? excludeId = null)
        {
            return !await _dbSet.AnyAsync(p =>
                p.PrimaryCode == primaryCode &&
                (!excludeId.HasValue || p.Id != excludeId.Value));
        }

        public async Task<bool> IsCode2UniqueAsync(string code2, int? excludeId = null)
        {
            if (string.IsNullOrEmpty(code2)) return true;
            return !await _dbSet.AnyAsync(p =>
                p.Code2 == code2 &&
                (!excludeId.HasValue || p.Id != excludeId.Value));
        }

        public async Task<bool> IsCode3UniqueAsync(string code3, int? excludeId = null)
        {
            if (string.IsNullOrEmpty(code3)) return true;
            return !await _dbSet.AnyAsync(p =>
                p.Code3 == code3 &&
                (!excludeId.HasValue || p.Id != excludeId.Value));
        }

        public async Task UpdateTotalStockAsync(int productId)
        {
            var product = await _dbSet
                .Include(p => p.Stocks)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product != null)
            {
                product.TotalStock = product.Stocks.Where(s => s.IsActive).Sum(s => s.Quantity);
                await _context.SaveChangesAsync();
            }
        }
    }
}