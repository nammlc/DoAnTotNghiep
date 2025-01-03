using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using DoAnTotNghiep.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using DoAnTotNghiep.Pages;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 25))));
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<VnPayService>();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "XSRF-TOKEN";
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/DangNhap";  // Đường dẫn đến trang đăng nhập
        options.LogoutPath = "/DangXuat";  // Đường dẫn đến trang đăng xuất
    });



var app = builder.Build();

// Khởi tạo và kiểm tra kết nối cơ sở dữ liệu
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    try
    {
        dbContext.Database.CanConnect();
        Console.WriteLine("Kết nối cơ sở dữ liệu thành công.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Lỗi kết nối cơ sở dữ liệu: {ex.Message}");
    }
}

// Cấu hình pipeline xử lý HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseAuthentication();
app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/DangNhap");
    return Task.CompletedTask;
});
app.MapRazorPages();

app.UseEndpoints(endpoints =>
    {
        endpoints.MapRazorPages();
    });



app.Run();
