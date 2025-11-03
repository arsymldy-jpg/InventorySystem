using Microsoft.AspNetCore.Authorization;
using Inventory_Api.Models.Enums;

namespace Inventory_Api.Helpers
{
    public static class AuthorizationPolicies
    {
        public static void ConfigurePolicies(AuthorizationOptions options)
        {
            // Admin Policy - فقط ادمین
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole(UserRole.Admin.ToString()));

            // Senior Users Policy - ادمین و کاربر ارشد
            options.AddPolicy("SeniorUsers", policy =>
                policy.RequireRole(UserRole.Admin.ToString(), UserRole.SeniorUser.ToString()));

            // Warehouse Management Policy - مدیریت انبار
            options.AddPolicy("WarehouseManagement", policy =>
                policy.RequireRole(
                    UserRole.Admin.ToString(),
                    UserRole.SeniorUser.ToString(),
                    UserRole.SeniorWarehouseManager.ToString(),
                    UserRole.WarehouseManager.ToString()));

            // View Only Policy - فقط مشاهده
            options.AddPolicy("ViewOnly", policy =>
                policy.RequireRole(
                    UserRole.Admin.ToString(),
                    UserRole.SeniorUser.ToString(),
                    UserRole.SeniorWarehouseManager.ToString(),
                    UserRole.WarehouseManager.ToString(),
                    UserRole.Supervisor.ToString()));

            // User Management Policy - مدیریت کاربران
            options.AddPolicy("UserManagement", policy =>
                policy.RequireRole(
                    UserRole.Admin.ToString(),
                    UserRole.SeniorUser.ToString(),
                    UserRole.SeniorWarehouseManager.ToString()));

            // Product Management Policy - مدیریت کالاها
            options.AddPolicy("ProductManagement", policy =>
                policy.RequireRole(
                    UserRole.Admin.ToString(),
                    UserRole.SeniorUser.ToString()));

            // Stock Management Policy - مدیریت موجودی
            options.AddPolicy("StockManagement", policy =>
                policy.RequireRole(
                    UserRole.Admin.ToString(),
                    UserRole.SeniorUser.ToString(),
                    UserRole.SeniorWarehouseManager.ToString(),
                    UserRole.WarehouseManager.ToString()));

            // Warehouse View Policy - مشاهده انبارها
            options.AddPolicy("WarehouseView", policy =>
                policy.RequireRole(
                    UserRole.Admin.ToString(),
                    UserRole.SeniorUser.ToString(),
                    UserRole.SeniorWarehouseManager.ToString(),
                    UserRole.WarehouseManager.ToString(),
                    UserRole.Supervisor.ToString()));

            // Report View Policy - مشاهده گزارشات
            options.AddPolicy("ReportView", policy =>
                policy.RequireRole(
                    UserRole.Admin.ToString(),
                    UserRole.SeniorUser.ToString(),
                    UserRole.SeniorWarehouseManager.ToString(),
                    UserRole.Supervisor.ToString()));

            // Audit View Policy - مشاهده لاگ‌ها
            options.AddPolicy("AuditView", policy =>
                policy.RequireRole(
                    UserRole.Admin.ToString(),
                    UserRole.SeniorUser.ToString()));
        }
    }
}