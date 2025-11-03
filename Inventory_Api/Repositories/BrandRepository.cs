    using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;

namespace Inventory_Api.Repositories
{
    public class BrandRepository : Repository<Brand>, IBrandRepository
    {
        public BrandRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Brand?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.Name == name);
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            return !await _dbSet.AnyAsync(b =>
                b.Name == name &&
                (!excludeId.HasValue || b.Id != excludeId.Value));
        }
    }
}