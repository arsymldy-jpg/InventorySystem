using Inventory_Web.Models;
using System.Text;
using Newtonsoft.Json;

namespace Inventory_Web.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;

        public UserService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("InventoryApi"); // همان نام از Program.cs
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            try
            {
                // تغییر: استفاده از مسیر صحیح بر اساس کنترلر Inventory_Api
                // اگر کنترلر Route داشته باشد مثلاً [Route("api/[controller]")], باید مسیر "api/Users" باشد
                var response = await _httpClient.GetAsync("api/Users"); // تغییر این خط
                response.EnsureSuccessStatusCode(); // این خط خطا می‌دهد اگر Status Code 2xx نباشد
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<UserDto>>(jsonString) ?? new List<UserDto>();
            }
            catch (HttpRequestException httpEx)
            {
                // چاپ خطا در کنسول
                Console.WriteLine($"HttpClient Error in GetAllUsersAsync: {httpEx.Message}");
                Console.WriteLine($"Request URI: {_httpClient.BaseAddress}api/Users"); // یا مسیر واقعی
                Console.WriteLine($"Status Code: {httpEx.StatusCode}");
                throw; // برای اینکه کنترلر بتواند آن را بگیرد و نمایش دهد
            }
            catch (TaskCanceledException tcEx) // ممکن است برای Timeout باشد
            {
                Console.WriteLine($"Request Timeout in GetAllUsersAsync: {tcEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // چاپ سایر خطاهای غیرمنتظره
                Console.WriteLine($"General Error in GetAllUsersAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw; // برای اینکه کنترلر بتواند آن را بگیرد و نمایش دهد
            }
        }


        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"Users/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserDto>(jsonString);
            }
            return null;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto userDto)
        {
            var json = JsonConvert.SerializeObject(userDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Users", content);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UserDto>(responseString) ?? throw new InvalidOperationException("Failed to deserialize created user.");
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto userDto)
        {
            // توجه: ممکن است نیاز باشد DTO ویرایش فقط شامل فیلدهای مورد نیاز باشد یا Id در بدنه نباشد
            // این بستگی به پیاده‌سازی API دارد
            // در اینجا فرض می‌کنیم UpdateUserDto شامل Id است و API آن را نادیده می‌گیرد و از مسیر استفاده می‌کند
            var json = JsonConvert.SerializeObject(userDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"Users/{id}", content);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserDto>(responseString);
            }
            return null; // یا ایجاد یک استثنا
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            // توجه: API باید بر اساس نقش کاربر فراخوانی کننده تصمیم بگیرد که آیا اجازه حذف وجود دارد یا خیر
            var response = await _httpClient.DeleteAsync($"Users/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ToggleUserActiveStatusAsync(int id, bool isActive)
        {
            // ممکن است یک اندپوینت خاص برای تغییر وضعیت فعال/غیرفعال وجود داشته باشد
            // مثلاً PUT یا POST به /Users/{id}/toggle-active
            // یا اینکه از همان اندپوینت ویرایش کاربر استفاده شود و فقط فیلد IsActive تغییر کند
            // برای الان، فرض می‌کنیم یک اندپوینت اختصاصی وجود دارد
            var toggleDto = new { IsActive = isActive };
            var json = JsonConvert.SerializeObject(toggleDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"Users/{id}/toggle-active", content);
            return response.IsSuccessStatusCode;
        }
    }
}