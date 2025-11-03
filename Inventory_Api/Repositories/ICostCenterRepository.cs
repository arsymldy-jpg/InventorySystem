using Inventory_Api.Models.Entities;

namespace Inventory_Api.Repositories
{
    public interface ICostCenterRepository : IRepository<CostCenter>
    {
        Task<IEnumerable<CostCenter>> GetActiveCostCentersAsync();
        // اضافه کردن متدهای مورد نیاز به Interface
        void Update(CostCenter entity);
        Task SaveChangesAsync();
    }
}