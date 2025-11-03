using Inventory_Web.Models;
using System.Text;
using Newtonsoft.Json;

namespace Inventory_Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string TokenCookieName = "JwtToken";

        // تغییر: وابسته گرفتن IHttpClientFactory
        public AuthService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            // ایجاد HttpClient با نام ثبت شده
            _httpClient = httpClientFactory.CreateClient("InventoryApi"); // همان نام از Program.cs
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // تغییر: استفاده از LoginDto جدید (PersonnelCode به جای Username)
                var json = JsonConvert.SerializeObject(loginDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("Auth/Login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // فرض بر این است که API یک DTO با پراپرتی Token برمی‌گرداند
                    // شما باید ساختار AuthResponseDto را مطابق با خروجی API خود تعریف کنید
                    var authResponse = JsonConvert.DeserializeObject<AuthResponseDto>(responseContent);

                    if (authResponse != null && !string.IsNullOrEmpty(authResponse.Token))
                    {
                        // ذخیره Token در کوکی
                        SetTokenCookie(authResponse.Token);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                // لاگ خطا
                Console.WriteLine($"Login Error: {ex.Message}");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            // حذف Token از کوکی
            RemoveTokenCookie();
        }

        public bool IsLoggedIn()
        {
            var token = GetTokenFromCookie();
            // در اینجا می‌توانید یک چک ساده مانند اعتبارسنجی فرمت یا فرستادن یک درخواست "Validate" به API انجام دهید
            // برای سادگی، فقط چک می‌کنیم که توکن وجود داشته باشد
            return !string.IsNullOrEmpty(token);
        }

        private void SetTokenCookie(string token)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // امنیت: جلوگیری از دسترسی JavaScript به کوکی
                    Secure = true,   // امنیت: فقط روی HTTPS ارسال شود (اگر HTTPS فعال باشد)
                    SameSite = SameSiteMode.Strict, // امنیت: جلوگیری از حملات CSRF قوی‌تر
                    Expires = DateTime.UtcNow.AddHours(1) // مثلاً 1 ساعت
                };
                _httpContextAccessor.HttpContext.Response.Cookies.Append(TokenCookieName, token, cookieOptions);
            }
        }

        private void RemoveTokenCookie()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Delete(TokenCookieName);
            }
        }

        private string? GetTokenFromCookie()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                return _httpContextAccessor.HttpContext.Request.Cookies[TokenCookieName];
            }
            return null;
        }
    }

    // کلاس کمکی برای دیسریالایز کردن پاسخ API
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        // ممکن است API فیلدهای دیگری مانند User Info نیز برگرداند
        // مثلاً:
        // public string UserName { get; set; } = string.Empty;
        // public string Role { get; set; } = string.Empty;
    }
}