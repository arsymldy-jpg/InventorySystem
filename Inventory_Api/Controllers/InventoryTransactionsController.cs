using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Services;
using Inventory_Api.Models.Enums; // برای UserRole
using System.Security.Claims; // برای گرفتن اطلاعات کاربر

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryTransactionsController : ControllerBase
    {
        private readonly IInventoryTransactionService _transactionService;

        public InventoryTransactionsController(IInventoryTransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // POST: api/InventoryTransactions
        [HttpPost]
        [Authorize(Policy = "StockManagement")] // Admin, SeniorUser, SeniorWarehouseManager, WarehouseManager
        public async Task<ActionResult<InventoryTransactionDto>> CreateTransaction([FromBody] CreateInventoryTransactionDto createDto)
        {
            // گرفتن شناسه و نقش کاربر فعلی
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out int userId) || !Enum.TryParse<UserRole>(roleClaim, out UserRole userRole))
            {
                return Unauthorized("User authentication information is invalid.");
            }

            try
            {
                var transaction = await _transactionService.CreateTransactionAsync(createDto, userId, userRole);
                return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // یا 403
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // مثلاً موجودی کافی نیست
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // مثلاً نوع تراکنش اشتباه
            }
        }

        // GET: api/InventoryTransactions?productId=1
        [HttpGet]
        [Authorize(Policy = "ViewOnly")] // Admin, SeniorUser, SeniorWarehouseManager, WarehouseManager, Supervisor
        public async Task<ActionResult<IEnumerable<InventoryTransactionDto>>> GetTransactions(
            [FromQuery] int? productId = null,
            [FromQuery] int? warehouseId = null)
        {
            // گرفتن شناسه و نقش کاربر فعلی
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out int userId) || !Enum.TryParse<UserRole>(roleClaim, out UserRole userRole))
            {
                return Unauthorized("User authentication information is invalid.");
            }

            IEnumerable<InventoryTransactionDto> transactions;

            if (productId.HasValue)
            {
                transactions = await _transactionService.GetTransactionsByProductAsync(productId.Value, userId, userRole);
            }
            else if (warehouseId.HasValue)
            {
                // چک کردن دسترسی در سرویس
                try
                {
                    transactions = await _transactionService.GetTransactionsByWarehouseAsync(warehouseId.Value, userId, userRole);
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Forbid(ex.Message); // یا 403
                }
            }
            else
            {
                // گرفتن تمام تراکنش‌های قابل دسترسی توسط کاربر
                transactions = await _transactionService.GetUserAccessibleTransactionsAsync(userId, userRole);
            }

            return Ok(transactions);
        }

        // GET: api/InventoryTransactions/5
        [HttpGet("{id}")]
        [Authorize(Policy = "ViewOnly")] // Admin, SeniorUser, SeniorWarehouseManager, WarehouseManager, Supervisor
        public async Task<ActionResult<InventoryTransactionDto>> GetTransactionById(int id)
        {
            // گرفتن شناسه و نقش کاربر فعلی
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out int userId) || !Enum.TryParse<UserRole>(roleClaim, out UserRole userRole))
            {
                return Unauthorized("User authentication information is invalid.");
            }

            InventoryTransactionDto? transaction;
            try
            {
                transaction = await _transactionService.GetTransactionByIdAsync(id, userId, userRole);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // یا 403
            }

            if (transaction == null) return NotFound();
            return Ok(transaction);
        }
    }
}