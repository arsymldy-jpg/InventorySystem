using Inventory_Api.Models.Enums;

namespace Inventory_Api.Models.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonnelCode { get; set; }
        public string MobileNumber { get; set; }
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonnelCode { get; set; }
        public string MobileNumber { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class UpdateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}