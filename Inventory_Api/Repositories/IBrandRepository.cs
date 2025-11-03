using Inventory_Api.Models.Entities;

namespace Inventory_Api.Repositories
{
    public interface IBrandRepository : IRepository<Brand>
    {
        Task<Brand?> GetByNameAsync(string name);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    }
}