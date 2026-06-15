using HungryHub.Data;
using HungryHub.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(
        builder.Configuration
            .GetConnectionString(
                "DefaultConnection")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout =
        TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<ExcelService>();
builder.Services.AddScoped<EmailReportService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<WeatherService>();
builder.Services.AddScoped<PaymentService>();
builder.Services
    .AddHostedService<WeeklyReportHostedService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// ── Named routes for short URLs ──────────────
app.MapControllerRoute(
    name: "cart",
    pattern: "Cart",
    defaults: new
    {
        controller = "Cart",
        action = "Index"
    });

app.MapControllerRoute(
    name: "dashboard",
    pattern: "Dashboard",
    defaults: new
    {
        controller = "Dashboard",
        action = "Index"
    });

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Account}" +
        "/{action=Login}/{id?}");

app.Run();