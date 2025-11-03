using Inventory_Web.Models;

namespace Inventory_Web.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginDto loginDto);
        Task LogoutAsync();
        bool IsLoggedIn();
    }
}