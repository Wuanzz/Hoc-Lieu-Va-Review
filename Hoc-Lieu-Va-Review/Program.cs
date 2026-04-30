using Hoc_Lieu_Va_Review.Hubs;
using Hoc_Lieu_Va_Review.Models;
using Hoc_Lieu_Va_Review.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Đăng ký GeminiService và cấp cho nó một cái HttpClient để lướt web gọi API
builder.Services.AddHttpClient<GeminiService>();

// Thêm cấu hình Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn sẽ bị đẩy tới nếu chưa đăng nhập
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Thời gian sống của phiên đăng nhập
    });

// Thêm dịch vụ SignalR vào hệ thống
builder.Services.AddSignalR();

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

app.UseAuthentication(); // Thêm middleware xác thực
app.UseAuthorization();

// Đăng ký đường dẫn cho Trạm phát sóng
app.MapHub<NotificationHub>("/notificationHub");

// Định tuyến cho khu vực admin
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Định tuyến cho khu vực sinh viên
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();