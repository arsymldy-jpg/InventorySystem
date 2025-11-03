using System.Security.Claims;
using Inventory_Api.Services;

namespace Inventory_Api.Middlewares
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuditService auditService)
        {
            // Skip audit for certain paths
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/api/auth"))
            {
                await _next(context);
                return;
            }

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
            }
            finally
            {
                await LogAuditAsync(context, auditService);

                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task LogAuditAsync(HttpContext context, IAuditService auditService)
        {
            try
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
                    return;

                var action = context.Request.Method switch
                {
                    "POST" => "CREATE",
                    "PUT" => "UPDATE",
                    "DELETE" => "DELETE",
                    "PATCH" => "UPDATE",
                    _ => "VIEW"
                };

                // ثبت اطلاعات کامل‌تر
                var description = $"{action} action performed on {context.Request.Path} - Status: {context.Response.StatusCode}";

                await auditService.LogActionAsync(
                    tableName: GetTableNameFromPath(context.Request.Path),
                    action: action,
                    recordId: GetRecordIdFromPath(context.Request.Path),
                    description: description,
                    userId: userIdInt,
                    oldValues: null,
                    newValues: null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while logging audit");
            }
        }

        private static string GetTableNameFromPath(PathString path)
        {
            var segments = path.Value?.Split('/') ?? Array.Empty<string>();
            return segments.Length >= 3 ? segments[2] : "Unknown";
        }

        private static string? GetRecordIdFromPath(PathString path)
        {
            var segments = path.Value?.Split('/') ?? Array.Empty<string>();
            return segments.Length >= 4 ? segments[3] : null;
        }
    }
}