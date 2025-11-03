using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Repositories;
using Inventory_Api.Helpers;
using Inventory_Api.Models.Enums;
using Newtonsoft.Json; // اضافه شود

namespace Inventory_Api.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IAuditService _auditService;

        public ProductService(
           IProductRepository productRepository,
           IBrandRepository brandRepository,
           IUserRepository userRepository, // اضافه شده
           IAuditService auditService)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _userRepository = userRepository; // اضافه شده
            _auditService = auditService;
        }

        // اضافه کردن متدهای جدید برای بررسی دسترسی کاربر
        public async Task<bool> CanUserManageProductsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // فقط ادمین و کاربر ارشد می‌توانند محصولات را مدیریت کنند
            return user.Role == UserRole.Admin || user.Role == UserRole.SeniorUser || user.Role == UserRole.SeniorWarehouseManager;
        }

        public async Task<bool> CanUserViewProductsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // همه کاربران لاگین شده می‌توانند محصولات را مشاهده کنند
            return user.IsActive;
        }


        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product == null ? null : MappingHelper.MapToDto(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Where(p => p.IsActive).Select(MappingHelper.MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByBrandAsync(int brandId)
        {
            var products = await _productRepository.GetProductsByBrandAsync(brandId);
            return products.Select(MappingHelper.MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _productRepository.SearchProductsAsync(searchTerm);
            return products.Select(MappingHelper.MapToDto);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, int currentUserId)
        {
            // اعتبارسنجی دسترسی کاربر
            if (!await CanUserManageProductsAsync(currentUserId))
            {
                throw new UnauthorizedAccessException("شما دسترسی ایجاد محصول را ندارید");
            }

            // Validate brand exists
            var brand = await _brandRepository.GetByIdAsync(createProductDto.BrandId);
            if (brand == null)
                throw new InvalidOperationException("برند مورد نظر وجود ندارد");

            // Validate unique codes
            if (!await _productRepository.IsPrimaryCodeUniqueAsync(createProductDto.PrimaryCode))
                throw new InvalidOperationException("کد اصلی تکراری است");

            if (!await _productRepository.IsCode2UniqueAsync(createProductDto.Code2))
                throw new InvalidOperationException("کد 2 تکراری است");

            if (!await _productRepository.IsCode3UniqueAsync(createProductDto.Code3))
                throw new InvalidOperationException("کد 3 تکراری است");

            var product = new Product
            {
                Name = createProductDto.Name,
                Name2 = createProductDto.Name2,
                PrimaryCode = createProductDto.PrimaryCode,
                Code2 = createProductDto.Code2,
                Code3 = createProductDto.Code3,
                ReorderPoint = createProductDto.ReorderPoint,
                SafetyStock = createProductDto.SafetyStock,
                BrandId = createProductDto.BrandId,
                TotalStock = 0,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdProduct = await _productRepository.AddAsync(product);

            await _auditService.LogActionAsync(
                "Products",
                "CREATE",
                createdProduct.Id.ToString(),
                $"Product created: {createdProduct.Name} ({createdProduct.PrimaryCode})",
                currentUserId
            );

            return MappingHelper.MapToDto(createdProduct);
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto, int currentUserId)
        {
            // اعتبارسنجی دسترسی کاربر
            if (!await CanUserManageProductsAsync(currentUserId))
            {
                throw new UnauthorizedAccessException("شما دسترسی ویرایش محصول را ندارید");
            }

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;

            // Validate brand exists
            var brand = await _brandRepository.GetByIdAsync(updateProductDto.BrandId);
            if (brand == null)
                throw new InvalidOperationException("برند مورد نظر وجود ندارد");

            // Validate unique codes
            if (!await _productRepository.IsPrimaryCodeUniqueAsync(updateProductDto.PrimaryCode, id))
                throw new InvalidOperationException("کد اصلی تکراری است");

            if (!await _productRepository.IsCode2UniqueAsync(updateProductDto.Code2, id))
                throw new InvalidOperationException("کد 2 تکراری است");

            if (!await _productRepository.IsCode3UniqueAsync(updateProductDto.Code3, id))
                throw new InvalidOperationException("کد 3 تکراری است");

            // ذخیره مقدار قبلی به صورت JSON قبل از تغییر
            var oldValuesJson = JsonConvert.SerializeObject(new
            {
                Name = product.Name,
                Name2 = product.Name2,
                PrimaryCode = product.PrimaryCode,
                Code2 = product.Code2,
                Code3 = product.Code3,
                ReorderPoint = product.ReorderPoint,
                SafetyStock = product.SafetyStock,
                BrandId = product.BrandId,
                IsActive = product.IsActive
                // CreatedAt و UpdatedAt معمولاً لازم نیست گزارش داده شوند
            });

            // اعمال تغییرات
            product.Name = updateProductDto.Name;
            product.Name2 = updateProductDto.Name2;
            product.PrimaryCode = updateProductDto.PrimaryCode;
            product.Code2 = updateProductDto.Code2;
            product.Code3 = updateProductDto.Code3;
            product.ReorderPoint = updateProductDto.ReorderPoint;
            product.SafetyStock = updateProductDto.SafetyStock;
            product.BrandId = updateProductDto.BrandId;
            // product.IsActive = updateProductDto.IsActive; // این خط را حذف کنید (اگر این فیلد در DTO وجود داشت)
            product.UpdatedAt = DateTime.UtcNow;

            var updatedProduct = await _productRepository.UpdateAsync(product);

            // ذخیره مقدار جدید به صورت JSON بعد از تغییر
            var newValuesJson = JsonConvert.SerializeObject(new
            {
                Name = product.Name,
                Name2 = product.Name2,
                PrimaryCode = product.PrimaryCode,
                Code2 = product.Code2,
                Code3 = product.Code3,
                ReorderPoint = product.ReorderPoint,
                SafetyStock = product.SafetyStock,
                BrandId = product.BrandId,
                IsActive = product.IsActive
            });

            await _auditService.LogActionAsync(
                "Products",
                "UPDATE",
                product.Id.ToString(),
                $"Product updated: {product.Name} ({product.PrimaryCode})",
                currentUserId,
                oldValuesJson, // ارسال JSON مقدار قبلی
                newValuesJson  // ارسال JSON مقدار جدید
            );

            return MappingHelper.MapToDto(updatedProduct);
        }

        public async Task<bool> DeleteProductAsync(int id, int currentUserId)
        {
            // اعتبارسنجی دسترسی کاربر
            if (!await CanUserManageProductsAsync(currentUserId))
            {
                throw new UnauthorizedAccessException("شما دسترسی حذف محصول را ندارید");
            }

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            // Soft delete
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);

            await _auditService.LogActionAsync(
                "Products",
                "DELETE",
                product.Id.ToString(),
                $"Product deleted: {product.Name} ({product.PrimaryCode})",
                currentUserId
            );

            return true;
        }
    }
}