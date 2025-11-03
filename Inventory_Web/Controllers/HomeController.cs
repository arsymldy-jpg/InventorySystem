using Microsoft.AspNetCore.Mvc;
using Inventory_Web.Services; // برای IAuthService

namespace Inventory_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;

        public HomeController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: Home
        [HttpGet]
        public IActionResult Index()
        {
            // چک می‌کنیم آیا کاربر وارد شده است یا خیر
            if (!_authService.IsLoggedIn())
            {
                // اگر وارد نشده، به صفحه ورود هدایت می‌شود (این در Program.cs پیکربندی شده)
                return Challenge(); // یا RedirectToAction("Login", "Auth");
            }

            // اگر وارد شده، صفحه داشبورد نمایش داده می‌شود
            return View();
        }

        // می‌توانید متدهای دیگری مانند Error برای صفحه خطا نیز اضافه کنید
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}