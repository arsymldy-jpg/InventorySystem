using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Models.Enums;
using Inventory_Api.Repositories;
using Inventory_Api.Helpers;

namespace Inventory_Api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IAuditService _auditService;

        public UserService(IUserRepository userRepository, IPasswordService passwordService, IAuditService auditService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _auditService = auditService;
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : MappingHelper.MapToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetAccessibleUsersAsync(int currentUserId, UserRole currentUserRole)
        {
            var allUsers = await GetAllUsersAsync();

            return currentUserRole switch
            {
                UserRole.Admin => allUsers, // ادمین همه کاربران را می‌بیند

                UserRole.SeniorUser => allUsers.Where(u =>
                    u.Role != UserRole.Admin && u.Role != UserRole.SeniorUser), // کاربر ارشد فقط کاربران سطح پایین‌تر

                UserRole.SeniorWarehouseManager => allUsers.Where(u =>
                    u.Role == UserRole.WarehouseManager), // انباردار ارشد فقط انبارداران

                _ => allUsers.Where(u => u.Id == currentUserId) // سایر کاربران فقط خودشان را می‌بینند
            };
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetActiveUsersAsync();
            return users.Select(MappingHelper.MapToDto);
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(UserRole role)
        {
            var users = await _userRepository.GetUsersByRoleAsync(role);
            return users.Select(MappingHelper.MapToDto);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto, int currentUserId)
        {
            // Validate unique constraints
            if (!await _userRepository.IsPersonnelCodeUniqueAsync(createUserDto.PersonnelCode))
                throw new InvalidOperationException("کد پرسنلی تکراری است");

            if (!await _userRepository.IsMobileNumberUniqueAsync(createUserDto.MobileNumber))
                throw new InvalidOperationException("شماره موبایل تکراری است");

            var user = new User
            {
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                PersonnelCode = createUserDto.PersonnelCode,
                MobileNumber = createUserDto.MobileNumber,
                Email = createUserDto.Email,
                PasswordHash = _passwordService.HashPassword(createUserDto.Password),
                Role = createUserDto.Role,
                ExpiryDate = createUserDto.ExpiryDate,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdUser = await _userRepository.AddAsync(user);

            // Audit log
            await _auditService.LogActionAsync(
                "Users",
                "CREATE",
                createdUser.Id.ToString(),
                $"User created: {createdUser.FirstName} {createdUser.LastName}",
                currentUserId
            );

            return MappingHelper.MapToDto(createdUser);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto, int currentUserId)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            // Validate unique constraints
            if (!await _userRepository.IsMobileNumberUniqueAsync(updateUserDto.MobileNumber, id))
                throw new InvalidOperationException("شماره موبایل تکراری است");

            var oldValues = $"FirstName: {user.FirstName}, LastName: {user.LastName}, Mobile: {user.MobileNumber}, Email: {user.Email}, IsActive: {user.IsActive}";

            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            user.MobileNumber = updateUserDto.MobileNumber;
            user.Email = updateUserDto.Email;
            user.IsActive = updateUserDto.IsActive;
            user.ExpiryDate = updateUserDto.ExpiryDate;
            user.UpdatedAt = DateTime.UtcNow;

            var updatedUser = await _userRepository.UpdateAsync(user);

            var newValues = $"FirstName: {user.FirstName}, LastName: {user.LastName}, Mobile: {user.MobileNumber}, Email: {user.Email}, IsActive: {user.IsActive}";

            // Audit log
            await _auditService.LogActionAsync(
                "Users",
                "UPDATE",
                user.Id.ToString(),
                $"User updated: {user.FirstName} {user.LastName}",
                currentUserId,
                oldValues,
                newValues
            );

            return MappingHelper.MapToDto(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(int id, int currentUserId)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;

            // Soft delete
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // Audit log
            await _auditService.LogActionAsync(
                "Users",
                "DELETE",
                user.Id.ToString(),
                $"User deleted: {user.FirstName} {user.LastName}",
                currentUserId
            );

            return true;
        }

        public async Task<bool> ChangeUserPasswordAsync(int userId, string newPassword, int currentUserId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.PasswordHash = _passwordService.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // Audit log
            await _auditService.LogActionAsync(
                "Users",
                "UPDATE",
                user.Id.ToString(),
                "User password changed",
                currentUserId
            );

            return true;
        }
    }
}