using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Inventory_Api.Services;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Models.Enums;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserWarehouseAccessController : ControllerBase
    {
        private readonly IUserWarehouseAccessService _accessService;

        public UserWarehouseAccessController(IUserWarehouseAccessService accessService)
        {
            _accessService = accessService;
        }

        [HttpPost("grant")]
        [Authorize(Policy = "SeniorUsers")]
        public async Task<ActionResult<UserWarehouseAccessDto>> GrantAccess(GrantWarehouseAccessDto grantDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var access = await _accessService.GrantAccessAsync(grantDto, currentUserId);
                return Ok(access);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("user/{userId}/warehouse/{warehouseId}")]
        [Authorize(Policy = "SeniorUsers")]
        public async Task<IActionResult> RevokeAccess(int userId, int warehouseId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _accessService.RevokeAccessAsync(userId, warehouseId, currentUserId);

                if (!result)
                {
                    return NotFound("دسترسی مورد نظر یافت نشد");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا برای دیباگ
                Console.WriteLine($"Error in RevokeAccess: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, "خطای سرور در حذف دسترسی");
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize(Policy = "UserManagement")]
        public async Task<ActionResult<IEnumerable<UserWarehouseAccessDto>>> GetUserAccesses(int userId)
        {
            var accesses = await _accessService.GetUserAccessesAsync(userId);
            return Ok(accesses);
        }

        [HttpGet("warehouse/{warehouseId}")]
        [Authorize] // تغییر: حذف Policy سخت‌گیرانه - همه کاربران لاگین شده می‌توانند ببینند
        public async Task<ActionResult<IEnumerable<UserWarehouseAccessDto>>> GetWarehouseAccesses(int warehouseId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // بررسی دسترسی کاربر به این انبار
                var canAccess = await _accessService.CanUserAccessWarehouseAsync(currentUserId, warehouseId);
                if (!canAccess)
                {
                    return Forbid("شما دسترسی به این انبار را ندارید");
                }

                var accesses = await _accessService.GetWarehouseAccessesAsync(warehouseId);
                return Ok(accesses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در دریافت دسترسی‌های انبار: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}/warehouse/{warehouseId}/can-access")]
        public async Task<ActionResult<bool>> CanUserAccessWarehouse(int userId, int warehouseId)
        {
            var canAccess = await _accessService.CanUserAccessWarehouseAsync(userId, warehouseId);
            return Ok(canAccess);
        }

        [HttpGet("user/{userId}/warehouse/{warehouseId}/can-modify")]
        public async Task<ActionResult<bool>> CanUserModifyWarehouse(int userId, int warehouseId)
        {
            var canModify = await _accessService.CanUserModifyWarehouseAsync(userId, warehouseId);
            return Ok(canModify);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}