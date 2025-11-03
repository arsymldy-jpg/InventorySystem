using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Services
{
    public interface ICostCenterService
    {
        Task<IEnumerable<CostCenterDto>> GetAllCostCentersAsync();
        Task<IEnumerable<CostCenterDto>> GetActiveCostCentersAsync();
        Task<CostCenterDto?> GetCostCenterByIdAsync(int id);
        Task<CostCenterDto> CreateCostCenterAsync(CreateCostCenterDto createDto);
        Task<CostCenterDto> UpdateCostCenterAsync(UpdateCostCenterDto updateDto);
        Task<bool> DeleteCostCenterAsync(int id); // غیرفعال کردن
    }
}