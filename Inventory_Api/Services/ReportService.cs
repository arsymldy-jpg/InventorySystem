using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Repositories;

namespace Inventory_Api.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IAuditService _auditService;

        public ReportService(
            ApplicationDbContext context,
            IProductRepository productRepository,
            IWarehouseRepository warehouseRepository,
            IStockRepository stockRepository,
            IAuditService auditService)
        {
            _context = context;
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
            _stockRepository = stockRepository;
            _auditService = auditService;
        }

        public async Task<InventorySummaryDto> GetInventorySummaryAsync()
        {
            var totalProducts = await _productRepository.CountAsync(p => p.IsActive);
            var totalBrands = await _context.Brands.CountAsync(b => b.IsActive);
            var totalWarehouses = await _warehouseRepository.CountAsync(w => w.IsActive);

            var products = await _productRepository.FindAsync(p => p.IsActive);
            var lowStockProducts = products.Count(p => p.TotalStock <= p.ReorderPoint && p.TotalStock > 0);
            var outOfStockProducts = products.Count(p => p.TotalStock == 0);

            return new InventorySummaryDto
            {
                TotalProducts = totalProducts,
                TotalBrands = totalBrands,
                TotalWarehouses = totalWarehouses,
                TotalStockValue = products.Sum(p => p.TotalStock), // می‌تواند با قیمت ضرب شود
                LowStockProducts = lowStockProducts,
                OutOfStockProducts = outOfStockProducts,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<StockMovementReportDto>> GetStockMovementReportAsync(DateTime fromDate, DateTime toDate)
        {
            var auditLogs = await _auditService.GetAuditLogsAsync(fromDate, toDate, "Stocks", null, null);

            var report = auditLogs.Select(log => new StockMovementReportDto
            {
                Date = log.CreatedAt,
                Action = log.Action,
                Description = log.Description,
                UserName = $"{log.User.FirstName} {log.User.LastName}",
                // این بخش نیاز به تجزیه description برای استخراج اطلاعات دارد
                // در پیاده‌سازی واقعی، این اطلاعات باید در جداول جداگانه ذخیره شوند
                ProductName = "N/A", // از description استخراج شود
                ProductCode = "N/A", // از description استخراج شود
                WarehouseName = "N/A", // از description استخراج شود
                Quantity = 0 // از description استخراج شود
            });

            return report;
        }

        public async Task<IEnumerable<WarehouseStockSummaryDto>> GetWarehouseStockSummaryAsync()
        {
            var warehouses = await _warehouseRepository.GetActiveWarehousesAsync();
            var summary = new List<WarehouseStockSummaryDto>();

            foreach (var warehouse in warehouses)
            {
                var stocks = await _stockRepository.GetStocksByWarehouseAsync(warehouse.Id);
                var totalStock = stocks.Sum(s => s.Quantity);
                var lowStockItems = stocks.Count(s => s.Quantity <= 10); // آستانه می‌تواند تنظیم شود
                var outOfStockItems = stocks.Count(s => s.Quantity == 0);

                summary.Add(new WarehouseStockSummaryDto
                {
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.Name,
                    WarehouseCode = warehouse.Code,
                    TotalProducts = stocks.Count(),
                    TotalStock = totalStock,
                    LowStockItems = lowStockItems,
                    OutOfStockItems = outOfStockItems
                });
            }

            return summary;
        }

        public async Task<IEnumerable<ProductStockHistoryDto>> GetProductStockHistoryAsync(int productId, DateTime fromDate, DateTime toDate)
        {
            // این متد نیاز به جدول history جداگانه دارد
            // فعلاً از audit logs استفاده می‌کنیم
            var auditLogs = await _auditService.GetAuditLogsAsync(fromDate, toDate, "Stocks", null, null);

            var productLogs = auditLogs
                .Where(log => log.Description.Contains($"ProductId: {productId}") || log.RecordId.Contains(productId.ToString()))
                .Select(log => new ProductStockHistoryDto
                {
                    Date = log.CreatedAt,
                    Action = log.Action,
                    UserName = $"{log.User.FirstName} {log.User.LastName}",
                    Notes = log.Description,
                    // سایر فیلدها نیاز به تجزیه description دارند
                    WarehouseName = "N/A",
                    Quantity = 0,
                    NewBalance = 0
                });

            return productLogs;
        }

        public async Task<BrandComparisonReportDto> GetBrandComparisonReportAsync()
        {
            var brands = await _context.Brands
                .Include(b => b.Products)
                    .ThenInclude(p => p.Stocks)
                .Where(b => b.IsActive)
                .ToListAsync();

            var brandStocks = brands.Select(b => new BrandStockInfo
            {
                BrandName = b.Name,
                ProductCount = b.Products.Count(p => p.IsActive),
                TotalStock = b.Products
                    .Where(p => p.IsActive)
                    .SelectMany(p => p.Stocks.Where(s => s.IsActive))
                    .Sum(s => s.Quantity),
                AverageStock = b.Products.Any(p => p.IsActive) ?
                    b.Products
                        .Where(p => p.IsActive)
                        .SelectMany(p => p.Stocks.Where(s => s.IsActive))
                        .Sum(s => s.Quantity) / b.Products.Count(p => p.IsActive) : 0,
                LowStockProducts = b.Products
                    .Count(p => p.IsActive &&
                               p.Stocks.Where(s => s.IsActive).Sum(s => s.Quantity) <= p.ReorderPoint)
            }).ToList();

            return new BrandComparisonReportDto
            {
                BrandStocks = brandStocks,
                GeneratedAt = DateTime.UtcNow
            };
        }

    }
}