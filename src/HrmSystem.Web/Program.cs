using System;
using HrmSystem.Infrastructure;
using HrmSystem.Infrastructure.Configuration;
using HrmSystem.Infrastructure.Seeder;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm dịch vụ MVC Controllers & Views
builder.Services.AddControllersWithViews();

// 2. Đăng ký Infrastructure theo Repository Pattern (chọn Provider từ appsettings.json)
builder.Services.AddHrmInfrastructure(builder.Configuration);

var app = builder.Build();

// 3. Tự động khởi tạo MongoDB Indexes và nạp dữ liệu mẫu khi ứng dụng khởi chạy
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var providerInfo = services.GetService<IDatabaseProviderInfo>();
        logger.LogInformation("Đang khởi động HRM System với Database Provider: {Provider}", providerInfo?.CurrentProvider);

        if (providerInfo?.CurrentProvider.Equals("MongoDB", StringComparison.OrdinalIgnoreCase) == true)
        {
            var indexInitializer = services.GetService<MongoIndexInitializer>();
            if (indexInitializer != null)
            {
                await indexInitializer.InitializeIndexesAsync();
            }

            var seeder = services.GetService<MongoDbDataSeeder>();
            if (seeder != null)
            {
                await seeder.SeedAsync(force: true);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Lỗi xảy ra trong quá trình khởi tạo database và nạp dữ liệu mẫu.");
    }
}

// 4. Cấu hình HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// 5. Cấu hình route mặc định trỏ vào Dashboard
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
