using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Services
{
    public interface IBrandService
    {
        Task<IEnumerable<BrandDto>> GetAllBrandsAsync(); // فقط برندهای فعال
        Task<IEnumerable<BrandDto>> GetAllBrandsForManagementAsync(); // همه برندها
        Task<BrandDto> GetBrandByIdAsync(int id);
        Task<BrandDto> CreateBrandAsync(CreateBrandDto brand, int currentUserId);
        Task<BrandDto> UpdateBrandAsync(int id, UpdateBrandDto brand, int currentUserId);
        Task<bool> DeleteBrandAsync(int id, int currentUserId);
    }
}