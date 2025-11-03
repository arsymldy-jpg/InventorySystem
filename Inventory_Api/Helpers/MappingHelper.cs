using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;

namespace Inventory_Api.Helpers
{
    public static class MappingHelper
    {
        public static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Name2 = product.Name2,
                PrimaryCode = product.PrimaryCode,
                Code2 = product.Code2,
                Code3 = product.Code3,
                TotalStock = product.TotalStock,
                ReorderPoint = product.ReorderPoint,
                SafetyStock = product.SafetyStock,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name ?? "Unknown",
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt
            };
        }

        public static StockDto MapToDto(Stock stock)
        {
            return new StockDto
            {
                Id = stock.Id,
                Quantity = stock.Quantity,
                ProductId = stock.ProductId,
                ProductName = stock.Product?.Name ?? "Unknown",
                ProductPrimaryCode = stock.Product?.PrimaryCode ?? "Unknown",
                WarehouseId = stock.WarehouseId,
                WarehouseName = stock.Warehouse?.Name ?? "Unknown",
                WarehouseCode = stock.Warehouse?.Code ?? "Unknown",
                CreatedAt = stock.CreatedAt,
                UpdatedAt = stock.UpdatedAt
            };
        }

        public static BrandDto MapToDto(Brand brand)
        {
            return new BrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                IsActive = brand.IsActive,
                CreatedAt = brand.CreatedAt,
                ProductsCount = brand.Products?.Count(p => p.IsActive) ?? 0
            };
        }

        public static WarehouseDto MapToDto(Warehouse warehouse)
        {
            return new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Address = warehouse.Address,
                Phone = warehouse.Phone,
                Code = warehouse.Code,
                IsActive = warehouse.IsActive,
                CreatedAt = warehouse.CreatedAt,
                StockItemsCount = warehouse.Stocks?.Count(s => s.IsActive) ?? 0
            };
        }

        public static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PersonnelCode = user.PersonnelCode,
                MobileNumber = user.MobileNumber,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                ExpiryDate = user.ExpiryDate,
                CreatedAt = user.CreatedAt
            };
        }
    }
}