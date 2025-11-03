using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;

namespace Inventory_Api.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(string tableName, string action, string recordId, string description, int userId, string? oldValues = null, string? newValues = null)
        {
            var auditLog = new AuditLog
            {
                TableName = tableName,
                Action = action,
                RecordId = recordId,
                Description = description,
                OldValues = oldValues,
                NewValues = newValues,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? tableName = null, string? action = null, int? userId = null)
        {
            var query = _context.AuditLogs
                .Include(al => al.User)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(al => al.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(al => al.CreatedAt <= toDate.Value);

            if (!string.IsNullOrEmpty(tableName))
                query = query.Where(al => al.TableName == tableName);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(al => al.Action == action);

            if (userId.HasValue)
                query = query.Where(al => al.UserId == userId.Value);

            return await query
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync();
        }
    }
}