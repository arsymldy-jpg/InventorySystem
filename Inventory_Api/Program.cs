using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Inventory_Api.Data;
using Inventory_Api.Services;
using Inventory_Api.Helpers;
using Inventory_Api.Repositories;
using Inventory_Api.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Inventory_Api.Models.Entities;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inventory API",
        Version = "v1",
        Description = "A comprehensive inventory management system API",
        Contact = new OpenApiContact
        {
            Name = "Inventory System",
            Email = "support@inventory.com"
        }
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter your token in the text input below.
                      Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

//builder.Services.AddIdentity<User, IdentityRole<int>>()
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddDefaultTokenProviders();

// Add DbContext with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add Authorization with policies
builder.Services.AddAuthorization(options =>
{
    AuthorizationPolicies.ConfigurePolicies(options);
});

// Add Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IUserWarehouseAccessRepository, UserWarehouseAccessRepository>(); 
builder.Services.AddScoped<ICostCenterRepository, CostCenterRepository>();
builder.Services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();

// Add Services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IUserWarehouseAccessService, UserWarehouseAccessService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICostCenterService, CostCenterService>();
builder.Services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add HttpContextAccessor for accessing HttpContext in services
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly); // معرفی Assembly حاوی MappingProfile

var app = builder.Build();

// Add Middleware
app.UseMiddleware<AuditMiddleware>();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DatabaseInitializer.Initialize(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API V1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Inventory API Documentation";
        c.EnablePersistAuthorization();
        c.DefaultModelsExpandDepth(-1);
    });
}

//app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

Console.WriteLine("Inventory API is starting...");
Console.WriteLine("Swagger UI available at: /swagger");
Console.WriteLine("Default admin credentials: ADMIN001 / Admin123!");

app.Run();


// در فایل Program.cs، بخش Authorization Policies را به این صورت اصلاح کنید:
public static class AuthorizationPolicies
{
    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin")); // فقط ادمین

        options.AddPolicy("SeniorUsers", policy =>
            policy.RequireRole("Admin", "SeniorUser")); // ادمین و کاربر ارشد

        options.AddPolicy("UserManagement", policy =>
            policy.RequireRole("Admin", "SeniorUser", "SeniorWarehouseManager")); // ادمین، کاربر ارشد، انباردار ارشد

        options.AddPolicy("WarehouseView", policy =>
            policy.RequireAuthenticatedUser()); // همه کاربران لاگین شده

        options.AddPolicy("ViewOnly", policy =>
            policy.RequireAuthenticatedUser()); // همه کاربران لاگین شده

        options.AddPolicy("StockManagement", policy =>
            policy.RequireRole("Admin", "SeniorUser", "SeniorWarehouseManager", "WarehouseManager")); // همه به جز ناظر

        options.AddPolicy("ReportView", policy =>
            policy.RequireRole("Admin", "SeniorUser", "SeniorWarehouseManager", "Supervisor")); // ادمین، کاربر ارشد، انباردار ارشد، ناظر

        options.AddPolicy("ProductManagement", policy =>
            policy.RequireRole("Admin", "SeniorUser")); // فقط ادمین و کاربر ارشد
    }
}