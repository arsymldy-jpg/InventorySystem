using AutoMapper;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Models.Entities;
using Inventory_Api.Repositories;

namespace Inventory_Api.Services
{
    public class CostCenterService : ICostCenterService
    {
        private readonly ICostCenterRepository _costCenterRepository;
        private readonly IMapper _mapper;

        public CostCenterService(ICostCenterRepository costCenterRepository, IMapper mapper)
        {
            _costCenterRepository = costCenterRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CostCenterDto>> GetAllCostCentersAsync()
        {
            var costCenters = await _costCenterRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CostCenterDto>>(costCenters);
        }

        public async Task<IEnumerable<CostCenterDto>> GetActiveCostCentersAsync()
        {
            var costCenters = await _costCenterRepository.GetActiveCostCentersAsync();
            return _mapper.Map<IEnumerable<CostCenterDto>>(costCenters);
        }

        public async Task<CostCenterDto?> GetCostCenterByIdAsync(int id)
        {
            var costCenter = await _costCenterRepository.GetByIdAsync(id);
            return costCenter == null ? null : _mapper.Map<CostCenterDto>(costCenter);
        }

        public async Task<CostCenterDto> CreateCostCenterAsync(CreateCostCenterDto createDto)
        {
            var costCenter = _mapper.Map<CostCenter>(createDto);
            costCenter.IsActive = true;
            await _costCenterRepository.AddAsync(costCenter);
            await _costCenterRepository.SaveChangesAsync();
            return _mapper.Map<CostCenterDto>(costCenter);
        }

        public async Task<CostCenterDto> UpdateCostCenterAsync(UpdateCostCenterDto updateDto)
        {
            var costCenter = await _costCenterRepository.GetByIdAsync(updateDto.Id);
            if (costCenter == null) return null; // یا ایجاد استثنا

            _mapper.Map(updateDto, costCenter);
            _costCenterRepository.Update(costCenter);
            await _costCenterRepository.SaveChangesAsync();
            return _mapper.Map<CostCenterDto>(costCenter);
        }

        public async Task<bool> DeleteCostCenterAsync(int id)
        {
            var costCenter = await _costCenterRepository.GetByIdAsync(id);
            if (costCenter == null) return false;

            costCenter.IsActive = false; // غیرفعال کردن
            _costCenterRepository.Update(costCenter);
            await _costCenterRepository.SaveChangesAsync();
            return true;
        }
    }
}