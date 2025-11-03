using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        string GenerateJwtToken(UserDto user);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto); // اضافه شده
    }
}