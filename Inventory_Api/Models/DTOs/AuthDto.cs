namespace Inventory_Api.Models.DTOs
{
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class LoginDto
    {
        public string PersonnelCode { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}