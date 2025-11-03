namespace Inventory_Web.Models
{
    public class LoginDto
    {
        // تغییر: از Username به PersonnelCode
        public string PersonnelCode { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}