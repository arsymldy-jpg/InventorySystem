using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Models.Enums;

namespace Inventory_Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordService _passwordService;

        public AuthService(ApplicationDbContext context, IConfiguration configuration, IPasswordService passwordService)
        {
            _context = context;
            _configuration = configuration;
            _passwordService = passwordService;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PersonnelCode == loginDto.PersonnelCode && u.IsActive);

            if (user == null || !_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            // اعتبارسنجی تاریخ انقضا برای کاربران غیر-ادمین
            if (user.Role != UserRole.Admin && user.Role != UserRole.SeniorUser &&
                user.ExpiryDate.HasValue && user.ExpiryDate.Value < DateTime.UtcNow)
            {
                return null;
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PersonnelCode = user.PersonnelCode,
                MobileNumber = user.MobileNumber,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                ExpiryDate = user.ExpiryDate,
                CreatedAt = user.CreatedAt
            };

            var token = GenerateJwtToken(userDto);

            return new AuthResponseDto
            {
                Token = token,
                User = userDto,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public string GenerateJwtToken(UserDto user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.PersonnelCode),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("PersonnelCode", user.PersonnelCode)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "YourSuperSecretKeyHereWithAtLeast32Characters!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "Inventory_Api",
                audience: _configuration["Jwt:Audience"] ?? "Inventory_Api_Users",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // بررسی رمز عبور فعلی
            if (!_passwordService.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                return false;

            // بررسی تطابق رمز عبور جدید و تأیید آن
            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
                return false;

            // تغییر رمز عبور
            user.PasswordHash = _passwordService.HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}