using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models;
using Inventory_Api.Repositories;
using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IAuditService _auditService;
        private readonly ApplicationDbContext _context;

        public BrandService(IBrandRepository brandRepository, IAuditService auditService, ApplicationDbContext context)
        {
            _brandRepository = brandRepository;
            _auditService = auditService;
            _context = context;
        }

        public async Task<IEnumerable<BrandDto>> GetAllBrandsAsync()
        {
            var brands = await _context.Brands
                .Where(b => b.IsActive) // فقط برندهای فعال برای نمایش در لیست
                .Include(b => b.Products)
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    ProductsCount = b.Products.Count(p => p.IsActive),
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return brands;
        }

        // اضافه کردن متد جدید برای گرفتن همه برندها (فعال و غیرفعال)
        public async Task<IEnumerable<BrandDto>> GetAllBrandsForManagementAsync()
        {
            var brands = await _context.Brands
                .Include(b => b.Products)
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    ProductsCount = b.Products.Count(p => p.IsActive),
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return brands;
        }

        public async Task<BrandDto> GetBrandByIdAsync(int id)
        {
            var brand = await _context.Brands
                .Where(b => b.Id == id)
                .Include(b => b.Products)
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    ProductsCount = b.Products.Count(p => p.IsActive),
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt
                })
                .FirstOrDefaultAsync();

            return brand;
        }

        public async Task<BrandDto> CreateBrandAsync(CreateBrandDto createBrandDto, int currentUserId)
        {
            var brand = new Brand
            {
                Name = createBrandDto.Name,
                Description = createBrandDto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdBrand = await _brandRepository.AddAsync(brand);

            await _auditService.LogActionAsync(
                "Brands",
                "CREATE",
                createdBrand.Id.ToString(),
                $"برند جدید ایجاد شد: {createdBrand.Name}",
                currentUserId
            );

            return new BrandDto
            {
                Id = createdBrand.Id,
                Name = createdBrand.Name,
                Description = createdBrand.Description,
                ProductsCount = 0,
                IsActive = createdBrand.IsActive,
                CreatedAt = createdBrand.CreatedAt
            };
        }

        public async Task<BrandDto> UpdateBrandAsync(int id, UpdateBrandDto updateBrandDto, int currentUserId)
        {
            var existingBrand = await _brandRepository.GetByIdAsync(id);
            if (existingBrand == null)
                return null;

            var oldValues = JsonSerializer.Serialize(existingBrand);

            // فقط فیلدهای مورد نظر را آپدیت کن، IsActive را تغییر نده
            existingBrand.Name = updateBrandDto.Name;
            existingBrand.Description = updateBrandDto.Description;
            // existingBrand.IsActive را تغییر نده
            existingBrand.UpdatedAt = DateTime.UtcNow;

            var updatedBrand = await _brandRepository.UpdateAsync(existingBrand);

            var newValues = JsonSerializer.Serialize(updatedBrand);

            await _auditService.LogActionAsync(
                "Brands",
                "UPDATE",
                updatedBrand.Id.ToString(),
                $"برند ویرایش شد: {updatedBrand.Name}",
                currentUserId,
                oldValues,
                newValues
            );

            return new BrandDto
            {
                Id = updatedBrand.Id,
                Name = updatedBrand.Name,
                Description = updatedBrand.Description,
                ProductsCount = await _context.Products.CountAsync(p => p.BrandId == id && p.IsActive),
                IsActive = updatedBrand.IsActive, // مقدار اصلی را برگردان
                CreatedAt = updatedBrand.CreatedAt
            };
        }

        public async Task<bool> DeleteBrandAsync(int id, int currentUserId)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null) return false;

            // بررسی آیا برند در محصولات استفاده شده است
            bool hasProducts = await _context.Products.AnyAsync(p => p.BrandId == id && p.IsActive);
            if (hasProducts)
            {
                // اگر برند در محصولات فعال استفاده شده، نمی‌توان حذف کرد
                return false;
            }

            // Soft delete
            brand.IsActive = false;
            brand.UpdatedAt = DateTime.UtcNow;

            await _brandRepository.UpdateAsync(brand);

            await _auditService.LogActionAsync(
                "Brands",
                "DELETE",
                brand.Id.ToString(),
                $"برند حذف شد: {brand.Name}",
                currentUserId
            );

            return true;
        }
    }
}