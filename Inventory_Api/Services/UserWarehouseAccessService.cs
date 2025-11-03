using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Repositories;
using Microsoft.EntityFrameworkCore; // برای LINQ ها

namespace Inventory_Api.Services
{
    public class UserWarehouseAccessService : IUserWarehouseAccessService
    {
        private readonly IUserWarehouseAccessRepository _accessRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IAuditService _auditService;

        public UserWarehouseAccessService(
            IUserWarehouseAccessRepository accessRepository,
            IUserRepository userRepository,
            IWarehouseRepository warehouseRepository,
            IAuditService auditService)
        {
            _accessRepository = accessRepository;
            _userRepository = userRepository;
            _warehouseRepository = warehouseRepository;
            _auditService = auditService;
        }

        public async Task<UserWarehouseAccessDto> GrantAccessAsync(GrantWarehouseAccessDto grantDto, int currentUserId)
        {
            try
            {
                // Validate user exists
                var user = await _userRepository.GetByIdAsync(grantDto.UserId);
                if (user == null)
                    throw new InvalidOperationException("کاربر مورد نظر وجود ندارد");

                // Validate warehouse exists
                var warehouse = await _warehouseRepository.GetByIdAsync(grantDto.WarehouseId);
                if (warehouse == null)
                    throw new InvalidOperationException("انبار مورد نظر وجود ندارد");

                // Check if access already exists (both active and inactive)
                var existingAccess = await _accessRepository.GetByUserAndWarehouseAsync(grantDto.UserId, grantDto.WarehouseId);

                UserWarehouseAccess access;

                if (existingAccess != null)
                {
                    // Update existing access (whether active or inactive)
                    existingAccess.CanModify = grantDto.CanModify;
                    existingAccess.CanView = grantDto.CanView;
                    existingAccess.IsActive = true; // Always set to active when granting access
                    existingAccess.UpdatedAt = DateTime.UtcNow;

                    await _accessRepository.UpdateAsync(existingAccess);
                    access = existingAccess;
                }
                else
                {
                    // Create new access
                    access = new UserWarehouseAccess
                    {
                        UserId = grantDto.UserId,
                        WarehouseId = grantDto.WarehouseId,
                        CanModify = grantDto.CanModify,
                        CanView = grantDto.CanView,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _accessRepository.AddAsync(access);
                }

                // Map to DTO
                var accessDto = MapToDto(access, user, warehouse);

                await _auditService.LogActionAsync(
                    "UserWarehouseAccess",
                    "GRANT",
                    $"{grantDto.UserId}-{grantDto.WarehouseId}",
                    $"Access granted: {user.FirstName} {user.LastName} to {warehouse.Name} (Modify: {grantDto.CanModify}, View: {grantDto.CanView})",
                    currentUserId
                );

                return accessDto;
            }
            catch (Exception ex)
            {
                // Log the detailed error
                Console.WriteLine($"Error in GrantAccessAsync: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw new InvalidOperationException($"خطا در اعطای دسترسی: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task<bool> RevokeAccessAsync(int userId, int warehouseId, int currentUserId)
        {
            var existingAccess = await _accessRepository.GetByUserAndWarehouseAsync(userId, warehouseId);
            if (existingAccess == null)
            {
                return false; // دسترسی وجود ندارد
            }

            var user = await _userRepository.GetByIdAsync(userId);
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);

            await _accessRepository.RemoveUserAccessAsync(userId, warehouseId);

            await _auditService.LogActionAsync(
                "UserWarehouseAccess",
                "REVOKE",
                $"{userId}-{warehouseId}",
                $"Access revoked: {user?.FirstName} {user?.LastName} from {warehouse?.Name}",
                currentUserId
            );

            return true;
        }

        public async Task<IEnumerable<UserWarehouseAccessDto>> GetUserAccessesAsync(int userId)
        {
            var accesses = await _accessRepository.GetByUserIdAsync(userId);
            var result = new List<UserWarehouseAccessDto>();

            foreach (var access in accesses)
            {
                // Load navigation properties if needed
                var user = access.User ?? await _userRepository.GetByIdAsync(access.UserId);
                var warehouse = access.Warehouse ?? await _warehouseRepository.GetByIdAsync(access.WarehouseId);

                result.Add(MapToDto(access, user, warehouse));
            }

            return result;
        }

        public async Task<IEnumerable<UserWarehouseAccessDto>> GetWarehouseAccessesAsync(int warehouseId)
        {
            var accesses = await _accessRepository.GetByWarehouseIdAsync(warehouseId);
            var result = new List<UserWarehouseAccessDto>();

            foreach (var access in accesses)
            {
                // Load navigation properties if needed
                var user = access.User ?? await _userRepository.GetByIdAsync(access.UserId);
                var warehouse = access.Warehouse ?? await _warehouseRepository.GetByIdAsync(access.WarehouseId);

                result.Add(MapToDto(access, user, warehouse));
            }

            return result;
        }

        public async Task<bool> CanUserAccessWarehouseAsync(int userId, int warehouseId)
        {
            return await _accessRepository.UserCanAccessWarehouseAsync(userId, warehouseId);
        }

        public async Task<bool> CanUserModifyWarehouseAsync(int userId, int warehouseId)
        {
            return await _accessRepository.UserCanModifyWarehouseAsync(userId, warehouseId);
        }

        // متد جدید برای گرفتن شناسه انبارهای قابل دسترس
        public async Task<IEnumerable<int>> GetUserAccessibleWarehouseIdsAsync(int userId, bool includeViewOnly = false)
        {
            // گرفتن تمام دسترسی‌های کاربر
            var userAccesses = await _accessRepository.GetByUserIdAsync(userId);

            // فیلتر کردن بر اساس دسترسی ویرایش یا مشاهده
            var accessibleWarehouseIds = userAccesses
                .Where(uwa => uwa.IsActive && (uwa.CanModify || (includeViewOnly && uwa.CanView)))
                .Select(uwa => uwa.WarehouseId);

            return accessibleWarehouseIds;
        }


        private static UserWarehouseAccessDto MapToDto(UserWarehouseAccess access, User user, Warehouse warehouse)
        {
            return new UserWarehouseAccessDto
            {
                Id = access.Id,
                UserId = access.UserId,
                UserFullName = $"{user.FirstName} {user.LastName}",
                UserPersonnelCode = user.PersonnelCode,
                WarehouseId = access.WarehouseId,
                WarehouseName = warehouse.Name,
                WarehouseCode = warehouse.Code,
                CanModify = access.CanModify,
                CanView = access.CanView,
                CreatedAt = access.CreatedAt
            };
        }
    }
}