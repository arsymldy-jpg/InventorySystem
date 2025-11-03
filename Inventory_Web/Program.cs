using Microsoft.AspNetCore.Mvc;
using Inventory_Web.Services; // برای ثبت سرویس‌ها
using Newtonsoft.Json; // برای HttpClient (اگر لازم باشد)
using Microsoft.AspNetCore.Authentication.Cookies; // برای احراز هویت کوکی

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// اضافه کردن IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// خواندن BaseUrl از appsettings.json
var apiBaseUrl = builder.Configuration.GetSection("ApiSettings:BaseUrl").Get<string>();
if (string.IsNullOrEmpty(apiBaseUrl))
{
    throw new InvalidOperationException("ApiSettings:BaseUrl is not configured in appsettings.json");
}

// --- روش صحیح: ثبت HttpClient نام‌گذاری شده ---
// یک HttpClient با نام خاص (مثلاً "InventoryApi") ثبت می‌کنیم
builder.Services.AddHttpClient("InventoryApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl); // استفاده از مقدار از appsettings
    // می‌توانید Headers پیش‌فرض نیز اضافه کنید
    // client.DefaultRequestHeaders.Add("Accept", "application/json");
    // client.DefaultRequestHeaders.Add("User-Agent", "Inventory_Web_Client/1.0");
});
// --- پایان تغییر ---

// ثبت سرویس‌ها (HttpClient نام‌گذاری شده را از طریق IHttpClientFactory دریافت می‌کنند)
// مهم: تغییر دادن سرویس‌هایی که HttpClient وابسته دارند
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>(); // اطمینان از ثبت این سرویس

// پیکربندی Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // مسیر صفحه ورود
        options.LogoutPath = "/Auth/Logout"; // مسیر خروج
        options.AccessDeniedPath = "/Home/AccessDenied"; // مسیر دسترسی غیرمجاز (اختیاری)
        // می‌توانید تنظیمات بیشتری مانند مدت زمان نشست را نیز اضافه کنید
    });

// ثبت سرویس‌های دیگر در صورت نیاز
// builder.Services.AddScoped<IOtherService, OtherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// اضافه کردن احراز هویت
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();