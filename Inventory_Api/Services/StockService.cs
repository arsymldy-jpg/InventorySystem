using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Repositories;
using Inventory_Api.Models.Enums;

namespace Inventory_Api.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IUserWarehouseAccessService _userWarehouseAccessService;
        private readonly IAuditService _auditService;

        public StockService(
            IStockRepository stockRepository,
            IProductRepository productRepository,
            IWarehouseRepository warehouseRepository,
            IUserWarehouseAccessService userWarehouseAccessService,
            IAuditService auditService)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
            _userWarehouseAccessService = userWarehouseAccessService;
            _auditService = auditService;
        }

        public async Task<IEnumerable<StockDto>> GetAllStocksAsync()
        {
            var stocks = await _stockRepository.GetAllAsync();
            return await MapStocksToDtos(stocks);
        }

        public async Task<IEnumerable<StockDto>> GetStocksByProductAsync(int productId)
        {
            var stocks = await _stockRepository.GetByProductIdAsync(productId);
            return await MapStocksToDtos(stocks);
        }

        public async Task<IEnumerable<StockDto>> GetStocksByWarehouseAsync(int warehouseId)
        {
            var stocks = await _stockRepository.GetStocksByWarehouseAsync(warehouseId);
            return await MapStocksToDtos(stocks);
        }

        public async Task<StockDto?> GetStockByProductAndWarehouseAsync(int productId, int warehouseId)
        {
            var stock = await _stockRepository.GetByProductAndWarehouseAsync(productId, warehouseId);
            if (stock == null) return null;

            return await MapStockToDto(stock);
        }

        public async Task<StockDto> AdjustStockAsync(AdjustStockDto adjustStockDto, int currentUserId)
        {
            var product = await _productRepository.GetByIdAsync(adjustStockDto.ProductId);
            if (product == null)
                throw new InvalidOperationException("محصول مورد نظر وجود ندارد");

            var warehouse = await _warehouseRepository.GetByIdAsync(adjustStockDto.WarehouseId);
            if (warehouse == null)
                throw new InvalidOperationException("انبار مورد نظر وجود ندارد");

            var existingStock = await _stockRepository.GetByProductAndWarehouseAsync(adjustStockDto.ProductId, adjustStockDto.WarehouseId);

            Stock stock;
            int oldQuantity = 0;
            int newQuantity = 0;

            if (existingStock != null)
            {
                oldQuantity = existingStock.Quantity;
                stock = existingStock;

                switch (adjustStockDto.Action.ToUpper())
                {
                    case "INCREASE":
                        stock.Quantity += adjustStockDto.Quantity;
                        break;
                    case "DECREASE":
                        if (stock.Quantity < adjustStockDto.Quantity)
                            throw new InvalidOperationException("موجودی کافی نیست");
                        stock.Quantity -= adjustStockDto.Quantity;
                        break;
                    case "SET":
                        stock.Quantity = adjustStockDto.Quantity;
                        break;
                    default:
                        throw new InvalidOperationException("عملیات نامعتبر");
                }

                stock.UpdatedAt = DateTime.UtcNow;
                await _stockRepository.UpdateAsync(stock);
            }
            else
            {
                if (adjustStockDto.Action.ToUpper() == "DECREASE")
                    throw new InvalidOperationException("امکان کاهش موجودی از صفر وجود ندارد");

                stock = new Stock
                {
                    ProductId = adjustStockDto.ProductId,
                    WarehouseId = adjustStockDto.WarehouseId,
                    Quantity = adjustStockDto.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _stockRepository.AddAsync(stock);
            }

            newQuantity = stock.Quantity;

            await _auditService.LogActionAsync(
                "Stock",
                "ADJUST",
                $"{adjustStockDto.ProductId}-{adjustStockDto.WarehouseId}",
                $"موجودی تنظیم شد: {product.Name} در {warehouse.Name} - از {oldQuantity} به {newQuantity} ({adjustStockDto.Action}) - دلیل: {adjustStockDto.Reason}",
                currentUserId
            );

            return await MapStockToDto(stock);
        }

        public async Task TransferStockAsync(TransferStockDto transferStockDto, int currentUserId)
        {
            var sourceStock = await _stockRepository.GetByProductAndWarehouseAsync(transferStockDto.ProductId, transferStockDto.SourceWarehouseId);
            if (sourceStock == null || sourceStock.Quantity < transferStockDto.Quantity)
                throw new InvalidOperationException("موجودی مبدا کافی نیست");

            var destinationStock = await _stockRepository.GetByProductAndWarehouseAsync(transferStockDto.ProductId, transferStockDto.DestinationWarehouseId);

            // کاهش از مبدا
            sourceStock.Quantity -= transferStockDto.Quantity;
            sourceStock.UpdatedAt = DateTime.UtcNow;
            await _stockRepository.UpdateAsync(sourceStock);

            // افزایش به مقصد
            if (destinationStock != null)
            {
                destinationStock.Quantity += transferStockDto.Quantity;
                destinationStock.UpdatedAt = DateTime.UtcNow;
                await _stockRepository.UpdateAsync(destinationStock);
            }
            else
            {
                destinationStock = new Stock
                {
                    ProductId = transferStockDto.ProductId,
                    WarehouseId = transferStockDto.DestinationWarehouseId,
                    Quantity = transferStockDto.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await _stockRepository.AddAsync(destinationStock);
            }

            var product = await _productRepository.GetByIdAsync(transferStockDto.ProductId);
            var sourceWarehouse = await _warehouseRepository.GetByIdAsync(transferStockDto.SourceWarehouseId);
            var destinationWarehouse = await _warehouseRepository.GetByIdAsync(transferStockDto.DestinationWarehouseId);

            await _auditService.LogActionAsync(
                "Stock",
                "TRANSFER",
                $"{transferStockDto.ProductId}",
                $"انتقال موجودی: {product?.Name} - از {sourceWarehouse?.Name} به {destinationWarehouse?.Name} - تعداد: {transferStockDto.Quantity} - دلیل: {transferStockDto.Reason}",
                currentUserId
            );
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
        {
            var lowStockProducts = await _stockRepository.GetLowStockProductsAsync();
            return lowStockProducts.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Name2 = p.Name2,
                PrimaryCode = p.PrimaryCode,
                Code2 = p.Code2,
                Code3 = p.Code3,
                TotalStock = p.Stocks?.Where(s => s.IsActive).Sum(s => s.Quantity) ?? 0,
                SafetyStock = p.SafetyStock,
                ReorderPoint = p.ReorderPoint
            });
        }

        public async Task<IEnumerable<BrandStockSummaryDto>> GetBrandStockSummaryAsync()
        {
            return await _stockRepository.GetBrandStockSummaryAsync();
        }

        public async Task<bool> CanUserAccessWarehouseAsync(int userId, int warehouseId)
        {
            return await _userWarehouseAccessService.CanUserAccessWarehouseAsync(userId, warehouseId);
        }

        public async Task<bool> CanUserModifyWarehouseAsync(int userId, int warehouseId)
        {
            return await _userWarehouseAccessService.CanUserModifyWarehouseAsync(userId, warehouseId);
        }

        private async Task<IEnumerable<StockDto>> MapStocksToDtos(IEnumerable<Stock> stocks)
        {
            var result = new List<StockDto>();

            foreach (var stock in stocks.Where(s => s.IsActive))
            {
                result.Add(await MapStockToDto(stock));
            }

            return result;
        }

        private async Task<StockDto> MapStockToDto(Stock stock)
        {
            var product = stock.Product ?? await _productRepository.GetByIdAsync(stock.ProductId);
            var warehouse = stock.Warehouse ?? await _warehouseRepository.GetByIdAsync(stock.WarehouseId);

            return new StockDto
            {
                Id = stock.Id,
                Quantity = stock.Quantity,
                ProductId = stock.ProductId,
                ProductName = product?.Name ?? "نامشخص",
                ProductPrimaryCode = product?.PrimaryCode ?? "نامشخص",
                WarehouseId = stock.WarehouseId,
                WarehouseName = warehouse?.Name ?? "نامشخص",
                WarehouseCode = warehouse?.Code ?? "نامشخص",
                CreatedAt = stock.CreatedAt,
                UpdatedAt = stock.UpdatedAt
            };
        }
    }
}