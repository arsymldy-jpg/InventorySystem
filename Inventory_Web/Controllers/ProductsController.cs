using Microsoft.AspNetCore.Mvc;
using Inventory_Web.Models;
using Inventory_Web.Services;

namespace Inventory_Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            // ممکن است نیاز به لیست برندها برای نمایش در یک DropdownList داشته باشیم
            // اینجا فقط یک View خالی برمی‌گردانیم، بعداً این بخش را کامل‌تر می‌کنیم
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDto productDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _productService.CreateProductAsync(productDto);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // می‌توانید خطا را لاگ کنید یا به کاربر نمایش دهید
                    ModelState.AddModelError("", $"خطا در ایجاد محصول: {ex.Message}");
                }
            }
            return View(productDto);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // مپ کردن از ProductDto به UpdateProductDto (یا استفاده مستقیم اگر ساختار یکسان است)
            var updateDto = new UpdateProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Name2 = product.Name2,
                PrimaryCode = product.PrimaryCode,
                Code2 = product.Code2,
                Code3 = product.Code3,
                ReorderPoint = product.ReorderPoint,
                SafetyStock = product.SafetyStock,
                BrandId = product.BrandId,
                IsActive = product.IsActive
            };

            return View(updateDto);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateProductDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var updatedProduct = await _productService.UpdateProductAsync(id, updateDto);
                    if (updatedProduct == null)
                    {
                        return NotFound(); // یا یک خطا نمایش دهید
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // می‌توانید خطا را لاگ کنید یا به کاربر نمایش دهید
                    ModelState.AddModelError("", $"خطا در ویرایش محصول: {ex.Message}");
                }
            }
            return View(updateDto);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _productService.DeleteProductAsync(id);
            if (!success)
            {
                return NotFound(); // یا یک خطا نمایش دهید
            }
            return RedirectToAction(nameof(Index));
        }
    }
}