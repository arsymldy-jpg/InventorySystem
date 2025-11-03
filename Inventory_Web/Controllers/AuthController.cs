using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Inventory_Web.Models;
using Inventory_Web.Services;
using System.Security.Claims; // برای ایجاد Claims

namespace Inventory_Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (_authService.IsLoggedIn())
            {
                // اگر کاربر قبلاً وارد شده بود، مستقیماً به داشبورد یا صفحه اصلی هدایت شود
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.LoginAsync(loginDto);
                if (result)
                {
                    // --- بخش جدید: ایجاد نشست محلی ASP.NET Core ---
                    // اینجا می‌توانیم اطلاعات کاربر را از Token استخراج کنیم یا یکباره از API درخواست کنیم
                    // برای سادگی، فرض می‌کنیم اطلاعات اولیه از loginDto و نتیجه ورود قابل استخراج است
                    // در عمل، ممکن است بخواهید از Token خود کاربر را شناسایی کنید

                    // مثلاً فرض کنید بعد از ورود، می‌توانید نام کاربری یا نقش را از جایی بگیرید
                    // یا اینکه یک درخواست اضافی به API بزنید تا جزئیات کاربر را بگیرید و Claims ایجاد کنید
                    // برای مثال ساده، فقط یک Claim برای PersonnelCode ایجاد می‌کنیم
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, loginDto.PersonnelCode), // یا یک شناسه واقعی از API
                        new Claim(ClaimTypes.Name, loginDto.PersonnelCode), // معمولاً Username یا PersonnelCode
                        // شما می‌توانید Claims دیگری مانند Role را نیز اضافه کنید، اگر از API در دسترس باشد
                        // new Claim(ClaimTypes.Role, userFromApi.Role.ToString()),
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // ایجاد نشست با Claims ایجاد شده
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                    // --- پایان بخش جدید ---

                    // موفقیت در ورود
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "کد پرسنلی یا رمز عبور اشتباه است.");
                }
            }
            return View(loginDto);
        }

        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();

            // باطل کردن نشست محلی (کوکی احراز هویت ASP.NET Core)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        //// GET: Auth/Register
        //[HttpGet]
        //public IActionResult Register()
        //{
        //    // فقط صفحه ثبت‌نام را نمایش می‌دهد
        //    // پیاده‌سازی کامل ثبت‌نام در گام‌های بعدی
        //    return View();
        //}

        //// POST: Auth/Register
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Register(/* مدل ثبت‌نام را بعداً اضافه می‌کنیم */)
        //{
        //    // پیاده‌سازی ثبت‌نام (فرستادن اطلاعات به API)
        //    // در گام‌های بعدی انجام می‌شود
        //    throw new NotImplementedException("Register functionality not yet implemented.");
        //}
    }
}