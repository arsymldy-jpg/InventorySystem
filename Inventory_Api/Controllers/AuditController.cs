using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Inventory_Api.Services;
using Inventory_Api.Models.Entities;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "SeniorUsers")]

    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? tableName,
            [FromQuery] string? action,
            [FromQuery] int? userId)
        {
            var logs = await _auditService.GetAuditLogsAsync(fromDate, toDate, tableName, action, userId);
            return Ok(logs);
        }

        [HttpGet("users/{userId}")]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetUserAuditLogs(int userId)
        {
            var logs = await _auditService.GetAuditLogsAsync(userId: userId);
            return Ok(logs);
        }
    }
}