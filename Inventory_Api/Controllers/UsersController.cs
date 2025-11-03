using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Services;
using System.Security.Claims;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "UserManagement")] // Authorization در سطح کنترلر
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUser = await _userService.GetUserByIdAsync(currentUserId);

                if (currentUser == null)
                    return Unauthorized();

                var users = await _userService.GetAccessibleUsersAsync(currentUserId, currentUser.Role);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در دریافت لیست کاربران: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var user = await _userService.CreateUserAsync(createUserDto, currentUserId);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var user = await _userService.UpdateUserAsync(id, updateUserDto, currentUserId);

                if (user == null)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")] // فقط ادمین می‌تواند کاربران را حذف کند
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _userService.DeleteUserAsync(id, currentUserId);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}