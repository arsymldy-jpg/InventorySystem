using Inventory_Web.Models;
using System.Text;
using Newtonsoft.Json; // نیاز به نصب پکیج Newtonsoft.Json در پروژه Inventory_Web دارید

namespace Inventory_Web.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;

        // تغییر: HttpClient نام‌گذاری شده را از طریق IHttpClientFactory ایجاد می‌کنیم
        public ProductService(IHttpClientFactory httpClientFactory)
        {
            // نام HttpClient ثبت شده در Program.cs
            _httpClient = httpClientFactory.CreateClient("InventoryApi");
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var response = await _httpClient.GetAsync("Products");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(jsonString) ?? new List<ProductDto>();
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"Products/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProductDto>(jsonString);
            }
            return null;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
        {
            var json = JsonConvert.SerializeObject(productDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Products", content);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ProductDto>(responseString) ?? throw new InvalidOperationException("Failed to deserialize created product.");
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto productDto)
        {
            // توجه: ممکن است نیاز باشد DTO ویرایش فقط شامل فیلدهای مورد نیاز باشد یا Id در بدنه نباشد
            // این بستگی به پیاده‌سازی API دارد
            // در اینجا فرض می‌کنیم UpdateProductDto شامل Id است و API آن را نادیده می‌گیرد و از مسیر استفاده می‌کند
            var json = JsonConvert.SerializeObject(productDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"Products/{id}", content);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProductDto>(responseString);
            }
            return null; // یا ایجاد یک استثنا
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Products/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}