using Inventory_Api.Models.Entities;

namespace Inventory_Api.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(string tableName, string action, string recordId, string description, int userId, string? oldValues = null, string? newValues = null);
        Task<List<AuditLog>> GetAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? tableName = null, string? action = null, int? userId = null);
    }
}