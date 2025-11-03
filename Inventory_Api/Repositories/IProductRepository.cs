using Inventory_Api.Models.Entities;

namespace Inventory_Api.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetByPrimaryCodeAsync(string primaryCode);
        Task<Product?> GetByCode2Async(string code2);
        Task<Product?> GetByCode3Async(string code3);
        Task<IEnumerable<Product>> GetProductsByBrandAsync(int brandId);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<bool> IsPrimaryCodeUniqueAsync(string primaryCode, int? excludeId = null);
        Task<bool> IsCode2UniqueAsync(string code2, int? excludeId = null);
        Task<bool> IsCode3UniqueAsync(string code3, int? excludeId = null);
        Task UpdateTotalStockAsync(int productId);
    }
}