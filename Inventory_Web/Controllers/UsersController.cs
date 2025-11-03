using Microsoft.AspNetCore.Mvc;
using Inventory_Web.Models;
using Inventory_Web.Services;
using Microsoft.AspNetCore.Authorization;

namespace Inventory_Web.Controllers
{
    // اعمال احراز هویت: کاربر باید وارد شده باشد تا بتواند به این کنترلر دسترسی پیدا کند
    [Authorize]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            // ممکن است نیاز به لیست نقش‌ها برای نمایش در یک DropdownList داشته باشیم
            // اینجا فقط یک View خالی برمی‌گردانیم، بعداً این بخش را کامل‌تر می‌کنیم
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserDto userDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.CreateUserAsync(userDto);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // می‌توانید خطا را لاگ کنید یا به کاربر نمایش دهید
                    ModelState.AddModelError("", $"خطا در ایجاد کاربر: {ex.Message}");
                }
            }
            return View(userDto);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // مپ کردن از UserDto به UpdateUserDto (یا استفاده مستقیم اگر ساختار یکسان است)
            var updateDto = new UpdateUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MobileNumber = user.MobileNumber,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                ExpiryDate = user.ExpiryDate
            };

            return View(updateDto);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateUserDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var updatedUser = await _userService.UpdateUserAsync(id, updateDto);
                    if (updatedUser == null)
                    {
                        return NotFound(); // یا یک خطا نمایش دهید
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // می‌توانید خطا را لاگ کنید یا به کاربر نمایش دهید
                    ModelState.AddModelError("", $"خطا در ویرایش کاربر: {ex.Message}");
                }
            }
            return View(updateDto);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // توجه: API باید منطق حذف (یا غیرفعال‌سازی) را بر اساس نقش کاربر فراخوانی کننده مدیریت کند
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound(); // یا یک خطا نمایش دهید
            }
            return RedirectToAction(nameof(Index));
        }
    }
}