using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Services
{
    public interface IProductService
    {
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetProductsByBrandAsync(int brandId);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, int currentUserId);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto, int currentUserId);
        Task<bool> DeleteProductAsync(int id, int currentUserId);
        Task<bool> CanUserManageProductsAsync(int userId);
        Task<bool> CanUserViewProductsAsync(int userId);
    }
}