using Inventory_Api.Models.DTOs;
using Inventory_Api.Models.Enums;

namespace Inventory_Api.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(UserRole role);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto, int currentUserId);
        Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto, int currentUserId);
        Task<bool> DeleteUserAsync(int id, int currentUserId);
        Task<bool> ChangeUserPasswordAsync(int userId, string newPassword, int currentUserId);
        Task<IEnumerable<UserDto>> GetAccessibleUsersAsync(int currentUserId, UserRole currentUserRole);
    }
}