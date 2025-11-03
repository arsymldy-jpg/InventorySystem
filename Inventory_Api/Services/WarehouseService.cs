using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Repositories;
using Inventory_Api.Models.Enums;

namespace Inventory_Api.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IUserWarehouseAccessRepository _accessRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditService _auditService;

        public WarehouseService(
            IWarehouseRepository warehouseRepository,
            IUserWarehouseAccessRepository accessRepository,
            IUserRepository userRepository,
            IAuditService auditService)
        {
            _warehouseRepository = warehouseRepository;
            _accessRepository = accessRepository;
            _userRepository = userRepository;
            _auditService = auditService;
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync()
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            return warehouses.Select(MapToDto);
        }

        public async Task<IEnumerable<WarehouseDto>> GetActiveWarehousesAsync()
        {
            var warehouses = await _warehouseRepository.GetActiveWarehousesAsync();
            return warehouses.Select(MapToDto);
        }

        public async Task<WarehouseDto?> GetWarehouseByIdAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            return warehouse == null ? null : MapToDto(warehouse);
        }

        public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseDto createWarehouseDto, int currentUserId)
        {
            var existingWarehouse = await _warehouseRepository.GetByCodeAsync(createWarehouseDto.Code);
            if (existingWarehouse != null)
            {
                throw new InvalidOperationException("انباری با این کد از قبل وجود دارد");
            }

            var warehouse = new Warehouse
            {
                Name = createWarehouseDto.Name,
                Address = createWarehouseDto.Address,
                Phone = createWarehouseDto.Phone,
                Code = createWarehouseDto.Code,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _warehouseRepository.AddAsync(warehouse);

            await _auditService.LogActionAsync(
                "Warehouse",
                "CREATE",
                warehouse.Id.ToString(),
                $"انبار جدید ایجاد شد: {warehouse.Name} ({warehouse.Code})",
                currentUserId
            );

            return MapToDto(warehouse);
        }

        public async Task<WarehouseDto?> UpdateWarehouseAsync(int id, UpdateWarehouseDto updateWarehouseDto, int currentUserId)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
            {
                return null;
            }

            var oldName = warehouse.Name;
            var oldCode = warehouse.Code;

            warehouse.Name = updateWarehouseDto.Name;
            warehouse.Address = updateWarehouseDto.Address;
            warehouse.Phone = updateWarehouseDto.Phone;
            warehouse.Code = updateWarehouseDto.Code;
            warehouse.IsActive = updateWarehouseDto.IsActive;
            warehouse.UpdatedAt = DateTime.UtcNow;

            await _warehouseRepository.UpdateAsync(warehouse);

            await _auditService.LogActionAsync(
                "Warehouse",
                "UPDATE",
                warehouse.Id.ToString(),
                $"انبار ویرایش شد: {oldName} -> {warehouse.Name}, کد: {oldCode} -> {warehouse.Code}, وضعیت: {(warehouse.IsActive ? "فعال" : "غیرفعال")}",
                currentUserId
            );

            return MapToDto(warehouse);
        }

        public async Task<bool> DeleteWarehouseAsync(int id, int currentUserId)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
            {
                return false;
            }

            // Check if warehouse has any active stocks
            var hasStocks = await _warehouseRepository.HasActiveStocksAsync(id);
            if (hasStocks)
            {
                throw new InvalidOperationException("امکان حذف انبار دارای موجودی فعال وجود ندارد");
            }

            warehouse.IsActive = false;
            warehouse.UpdatedAt = DateTime.UtcNow;

            await _warehouseRepository.UpdateAsync(warehouse);

            await _auditService.LogActionAsync(
                "Warehouse",
                "DELETE",
                warehouse.Id.ToString(),
                $"انبار حذف شد: {warehouse.Name} ({warehouse.Code})",
                currentUserId
            );

            return true;
        }

        public async Task<IEnumerable<WarehouseDto>> GetAccessibleWarehousesAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("کاربر وجود ندارد");

            // اگر کاربر ادمین یا کاربر ارشد است، همه انبارها را برگردان
            if (user.Role == UserRole.Admin || user.Role == UserRole.SeniorUser)
            {
                return await GetAllWarehousesAsync();
            }

            // فقط انبارهایی که دسترسی فعال و قابل مشاهده دارند (CanView)
            var accesses = await _accessRepository.GetByUserIdAsync(userId);
            var accessibleVisibleWarehouses = accesses
                .Where(a => a.IsActive && a.CanView)
                .Select(a => a.Warehouse)
                .Where(w => w != null)
                .Select(MapToDto);

            return accessibleVisibleWarehouses;
        }

        public async Task<IEnumerable<WarehouseDto>> GetAccessibleActiveWarehousesAsync(int userId)
        {
            var warehouses = await GetAccessibleWarehousesAsync(userId);
            return warehouses.Where(w => w.IsActive);
        }

        public async Task<WarehouseDto?> GetAccessibleWarehouseByIdAsync(int id, int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("کاربر وجود ندارد");

            // اگر کاربر ادمین یا کاربر ارشد است، همه انبارها قابل دسترسی هستند
            if (user.Role == UserRole.Admin || user.Role == UserRole.SeniorUser)
            {
                return await GetWarehouseByIdAsync(id);
            }

            // بررسی کنید که کاربر به این انبار خاص دسترسی مشاهده (CanView) دارد
            var canAccess = await _accessRepository.UserCanAccessWarehouseAsync(userId, id);
            if (!canAccess)
            {
                throw new UnauthorizedAccessException("شما دسترسی به این انبار را ندارید");
            }

            return await GetWarehouseByIdAsync(id);
        }

        private static WarehouseDto MapToDto(Warehouse warehouse)
        {
            return new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Address = warehouse.Address,
                Phone = warehouse.Phone,
                Code = warehouse.Code,
                IsActive = warehouse.IsActive,
                CreatedAt = warehouse.CreatedAt,
                StockItemsCount = warehouse.Stocks?.Count(s => s.IsActive) ?? 0
            };
        }
    }
}