using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Inventory_Api.Services;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Models.Enums;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/v2/[controller]")]
    [Authorize]
    public class UsersV2Controller : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersV2Controller(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Policy = "UserManagement")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var currentUserRole = GetCurrentUserRole();
            var users = await _userService.GetAllUsersAsync();

            // Filter users based on current user's role
            var filteredUsers = currentUserRole switch
            {
                UserRole.Admin => users,
                UserRole.SeniorUser => users.Where(u => u.Role != UserRole.Admin && u.Role != UserRole.SeniorUser),
                UserRole.SeniorWarehouseManager => users.Where(u => u.Role == UserRole.WarehouseManager || u.Role == UserRole.Supervisor),
                _ => Enumerable.Empty<UserDto>()
            };

            return Ok(filteredUsers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // Users can view their own profile, managers can view their subordinates
            if (id != currentUserId && currentUserRole > UserRole.SeniorWarehouseManager)
            {
                return Forbid();
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost]
        [Authorize(Policy = "UserManagement")]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Role-based validation
                if (!CanCreateUserWithRole(currentUserRole, createUserDto.Role))
                {
                    return Forbid("You don't have permission to create users with this role");
                }

                var user = await _userService.CreateUserAsync(createUserDto, currentUserId);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "UserManagement")]
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
        [Authorize(Policy = "AdminOnly")]
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

        [HttpPost("{id}/change-password")]
        [Authorize(Policy = "UserManagement")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] string newPassword)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _userService.ChangeUserPasswordAsync(id, newPassword, currentUserId);

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

        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.Parse<UserRole>(roleClaim ?? "Supervisor");
        }

        private bool CanCreateUserWithRole(UserRole currentUserRole, UserRole targetRole)
        {
            return currentUserRole switch
            {
                UserRole.Admin => true, // Admin can create any role
                UserRole.SeniorUser => targetRole != UserRole.Admin && targetRole != UserRole.SeniorUser,
                UserRole.SeniorWarehouseManager => targetRole == UserRole.WarehouseManager || targetRole == UserRole.Supervisor,
                _ => false
            };
        }
    }
}