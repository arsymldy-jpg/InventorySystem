using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;

namespace Inventory_Api.Repositories
{
    public class CostCenterRepository : Repository<CostCenter>, ICostCenterRepository
    {
        // اضافه کردن یک فیلد برای دسترسی به DbContext
        private readonly ApplicationDbContext _context;
        public CostCenterRepository(ApplicationDbContext context) : base(context)
        {
            _context = context; // ذخیره مرجع DbContext
        }

        public async Task<IEnumerable<CostCenter>> GetActiveCostCentersAsync()
        {
            return await _context.CostCenters
                .Where(cc => cc.IsActive)
                .ToListAsync();
        }

        // اضافه کردن متد Update و SaveChangesAsync
        public void Update(CostCenter entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}