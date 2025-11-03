using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Services;
using System.Security.Claims;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // این کنترلر برای همه قابل دسترسی است - درست است
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
            {
                return Unauthorized("کد پرسنلی یا رمز عبور اشتباه است");
            }

            return Ok(result);
        }
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _authService.ChangePasswordAsync(currentUserId, changePasswordDto);

                if (!result)
                {
                    return BadRequest("تغییر رمز عبور انجام نشد. لطفاً اطلاعات را بررسی کنید.");
                }

                return Ok("رمز عبور با موفقیت تغییر یافت");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در تغییر رمز عبور: {ex.Message}");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpPost("refresh-token")]
        [Authorize] // این باید protected باشد
        public async Task<ActionResult<AuthResponseDto>> RefreshToken()
        {
            // این متد بعداً پیاده‌سازی می‌شود
            return Ok("Token refresh functionality will be implemented soon");
        }
    }
}