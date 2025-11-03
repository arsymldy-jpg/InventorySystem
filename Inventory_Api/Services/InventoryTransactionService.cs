using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Models.Entities;
using Inventory_Api.Repositories;
using Inventory_Api.Services; // برای IUserWarehouseAccessService
using Inventory_Api.Models.Enums; // برای UserRole

namespace Inventory_Api.Services
{
    public class InventoryTransactionService : IInventoryTransactionService
    {
        private readonly IInventoryTransactionRepository _transactionRepository;
        private readonly IStockRepository _stockRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        // اضافه کردن سرویس جدید
        private readonly IUserWarehouseAccessService _userWarehouseAccessService;

        public InventoryTransactionService(
            IInventoryTransactionRepository transactionRepository,
            IStockRepository stockRepository,
            ApplicationDbContext context,
            IMapper mapper,
            // اضافه کردن وابستگی جدید
            IUserWarehouseAccessService userWarehouseAccessService)
        {
            _transactionRepository = transactionRepository;
            _stockRepository = stockRepository;
            _context = context;
            _mapper = mapper;
            // اختصاص دادن
            _userWarehouseAccessService = userWarehouseAccessService;
        }

        public async Task<InventoryTransactionDto> CreateTransactionAsync(CreateInventoryTransactionDto createDto, int userId, UserRole userRole)
        {
            // 1. اگر نقش کاربر WarehouseManager باشد، چک کردن دسترسی انبار (CanModify)
            if (userRole == UserRole.WarehouseManager)
            {
                // تغییر: استفاده از متد موجود CanUserModifyWarehouseAsync
                var hasAccess = await _userWarehouseAccessService.CanUserModifyWarehouseAsync(userId, createDto.WarehouseId);
                if (!hasAccess)
                {
                    throw new UnauthorizedAccessException("User does not have access to modify this warehouse.");
                }
            }
            // می‌توانید برای سایر نقش‌ها نیز منطق مشابه یا متفاوتی اضافه کنید.

            // 2. اعتبارسنجی ورودی
            if (createDto.TransactionType != "In" && createDto.TransactionType != "Out")
            {
                throw new ArgumentException("TransactionType must be 'In' or 'Out'.");
            }
            if (createDto.TransactionType == "Out" && createDto.CostCenterId == null)
            {
                // اگر منطق کسب و کار الزامی بودن CostCenter برای خروج را داشت، اینجا چک کنید
                // throw new ArgumentException("CostCenterId is required for 'Out' transactions.");
            }

            // 3. بررسی موجودی برای تراکنش خروج
            if (createDto.TransactionType == "Out")
            {
                var stock = await _stockRepository.GetByProductAndWarehouseAsync(createDto.ProductId, createDto.WarehouseId);
                if (stock == null || stock.Quantity < createDto.Quantity)
                {
                    throw new InvalidOperationException("Insufficient stock for this transaction.");
                }
            }

            // 4. ایجاد تراکنش
            var transaction = _mapper.Map<InventoryTransaction>(createDto);
            transaction.UserId = userId;

            await _transactionRepository.AddAsync(transaction);

            // 5. به‌روزرسانی موجودی (Stock)
            var stockToUpdate = await _stockRepository.GetByProductAndWarehouseAsync(createDto.ProductId, createDto.WarehouseId);
            if (stockToUpdate == null)
            {
                // اگر موجودی وجود نداشت، ممکن است باید ایجاد شود (برای ورود)
                if (createDto.TransactionType == "In")
                {
                    stockToUpdate = new Stock
                    {
                        ProductId = createDto.ProductId,
                        WarehouseId = createDto.WarehouseId,
                        Quantity = createDto.Quantity
                    };
                    await _stockRepository.AddAsync(stockToUpdate);
                }
                else
                {
                    // اگر برای خروج موجودی نبود، خطا
                    throw new InvalidOperationException("Stock record not found for 'Out' transaction.");
                }
            }
            else
            {
                // به‌روزرسانی موجودی
                if (createDto.TransactionType == "In")
                {
                    stockToUpdate.Quantity += createDto.Quantity;
                }
                else if (createDto.TransactionType == "Out")
                {
                    stockToUpdate.Quantity -= createDto.Quantity;
                }
                _stockRepository.Update(stockToUpdate);
            }

            // 6. ذخیره تغییرات
            await _stockRepository.SaveChangesAsync();
            await _transactionRepository.SaveChangesAsync();

            // 7. مپ کردن و بازگرداندن DTO
            var transactionDto = _mapper.Map<InventoryTransactionDto>(transaction);
            // پر کردن نام‌ها
            transactionDto.ProductName = (await _context.Products.FindAsync(createDto.ProductId))?.Name ?? "Unknown";
            transactionDto.WarehouseName = (await _context.Warehouses.FindAsync(createDto.WarehouseId))?.Name ?? "Unknown";
            if (transaction.CostCenterId.HasValue)
            {
                transactionDto.CostCenterName = (await _context.CostCenters.FindAsync(transaction.CostCenterId.Value))?.Name ?? "Unknown";
            }
            var user = await _context.Users.FindAsync(userId);
            transactionDto.UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown";

            return transactionDto;
        }

        // تغییر در متدهای بازیابی نیز لازم است
        public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByProductAsync(int productId, int userId, UserRole userRole)
        {
            // فرض بر این است که کاربر می‌تواند تراکنش‌های محصولی را ببیند که در انبارهایی است که دسترسی دارد.
            // ابتدا انبارهای قابل دسترسی کاربر را گرفته و سپس تراکنش‌ها را فیلتر می‌کنیم.
            IQueryable<InventoryTransaction> query = _context.InventoryTransactions;

            if (userRole == UserRole.WarehouseManager)
            {
                // استفاده از متد جدید
                var accessibleWarehouseIds = await _userWarehouseAccessService.GetUserAccessibleWarehouseIdsAsync(userId, includeViewOnly: true); // شامل انبارهایی که فقط می‌تواند ببیند
                query = query.Where(t => accessibleWarehouseIds.Contains(t.WarehouseId));
            }
            // می‌توانید برای سایر نقش‌ها نیز منطق فیلتر متفاوتی اضافه کنید.

            var transactions = await query.Where(t => t.ProductId == productId).ToListAsync();

            var dtos = new List<InventoryTransactionDto>();
            foreach (var t in transactions)
            {
                var dto = _mapper.Map<InventoryTransactionDto>(t);
                // پر کردن نام‌ها
                dto.ProductName = (await _context.Products.FindAsync(t.ProductId))?.Name ?? "Unknown";
                dto.WarehouseName = (await _context.Warehouses.FindAsync(t.WarehouseId))?.Name ?? "Unknown";
                if (t.CostCenterId.HasValue)
                {
                    dto.CostCenterName = (await _context.CostCenters.FindAsync(t.CostCenterId.Value))?.Name ?? "Unknown";
                }
                var user = await _context.Users.FindAsync(t.UserId);
                dto.UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown";
                dto.CreatedAt = t.CreatedAt; // فرض بر این است که BaseEntity دارای CreatedAt است
                dtos.Add(dto);
            }
            return dtos;
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByWarehouseAsync(int warehouseId, int userId, UserRole userRole)
        {
            // چک کردن دسترسی قبل از بازیابی
            if (userRole == UserRole.WarehouseManager)
            {
                // استفاده از متد موجود CanUserAccessWarehouseAsync
                var hasAccess = await _userWarehouseAccessService.CanUserAccessWarehouseAsync(userId, warehouseId);
                if (!hasAccess)
                {
                    throw new UnauthorizedAccessException("User does not have access to view this warehouse's transactions.");
                }
            }
            // می‌توانید برای سایر نقش‌ها نیز منطق مشابه یا متفاوتی اضافه کنید.

            var transactions = await _transactionRepository.GetTransactionsByWarehouseAsync(warehouseId);
            var dtos = new List<InventoryTransactionDto>();
            foreach (var t in transactions)
            {
                var dto = _mapper.Map<InventoryTransactionDto>(t);
                // پر کردن نام‌ها
                dto.ProductName = (await _context.Products.FindAsync(t.ProductId))?.Name ?? "Unknown";
                dto.WarehouseName = (await _context.Warehouses.FindAsync(t.WarehouseId))?.Name ?? "Unknown";
                if (t.CostCenterId.HasValue)
                {
                    dto.CostCenterName = (await _context.CostCenters.FindAsync(t.CostCenterId.Value))?.Name ?? "Unknown";
                }
                var user = await _context.Users.FindAsync(t.UserId);
                dto.UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown";
                dto.CreatedAt = t.CreatedAt; // فرض بر این است که BaseEntity دارای CreatedAt است
                dtos.Add(dto);
            }
            return dtos;
        }

        // متد جدید برای گرفتن همه تراکنش‌های یک کاربر در انبارهای قابل دسترس
        public async Task<IEnumerable<InventoryTransactionDto>> GetUserAccessibleTransactionsAsync(int userId, UserRole userRole)
        {
            IQueryable<InventoryTransaction> query = _context.InventoryTransactions;

            if (userRole == UserRole.WarehouseManager)
            {
                // استفاده از متد جدید
                var accessibleWarehouseIds = await _userWarehouseAccessService.GetUserAccessibleWarehouseIdsAsync(userId, includeViewOnly: true);
                query = query.Where(t => accessibleWarehouseIds.Contains(t.WarehouseId));
            }
            // می‌توانید برای سایر نقش‌ها نیز منطق فیلتر متفاوتی اضافه کنید.

            var transactions = await query.ToListAsync();

            var dtos = new List<InventoryTransactionDto>();
            foreach (var t in transactions)
            {
                var dto = _mapper.Map<InventoryTransactionDto>(t);
                // پر کردن نام‌ها
                dto.ProductName = (await _context.Products.FindAsync(t.ProductId))?.Name ?? "Unknown";
                dto.WarehouseName = (await _context.Warehouses.FindAsync(t.WarehouseId))?.Name ?? "Unknown";
                if (t.CostCenterId.HasValue)
                {
                    dto.CostCenterName = (await _context.CostCenters.FindAsync(t.CostCenterId.Value))?.Name ?? "Unknown";
                }
                var user = await _context.Users.FindAsync(t.UserId);
                dto.UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown";
                dto.CreatedAt = t.CreatedAt; // فرض بر این است که BaseEntity دارای CreatedAt است
                dtos.Add(dto);
            }
            return dtos;
        }

        // اصلاح متد GetTransactionByIdAsync
        public async Task<InventoryTransactionDto?> GetTransactionByIdAsync(int id, int userId, UserRole userRole)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null) return null;

            // چک کردن دسترسی به انبار تراکنش
            if (userRole == UserRole.WarehouseManager)
            {
                // استفاده از متد موجود CanUserAccessWarehouseAsync
                var hasAccess = await _userWarehouseAccessService.CanUserAccessWarehouseAsync(userId, transaction.WarehouseId);
                if (!hasAccess)
                {
                    throw new UnauthorizedAccessException("User does not have access to view this warehouse's transaction.");
                }
            }
            // می‌توانید برای سایر نقش‌ها نیز منطق مشابه یا متفاوتی اضافه کنید.

            var dto = _mapper.Map<InventoryTransactionDto>(transaction);
            // پر کردن نام‌ها
            dto.ProductName = (await _context.Products.FindAsync(transaction.ProductId))?.Name ?? "Unknown";
            dto.WarehouseName = (await _context.Warehouses.FindAsync(transaction.WarehouseId))?.Name ?? "Unknown";
            if (transaction.CostCenterId.HasValue)
            {
                dto.CostCenterName = (await _context.CostCenters.FindAsync(transaction.CostCenterId.Value))?.Name ?? "Unknown";
            }
            var user = await _context.Users.FindAsync(transaction.UserId);
            dto.UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown";
            dto.CreatedAt = transaction.CreatedAt; // فرض بر این است که BaseEntity دارای CreatedAt است
            return dto;
        }
    }
}