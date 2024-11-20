using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using DoAnTotNghiep.Data;

var builder = WebApplication.CreateBuilder(args);

// Thêm dịch vụ vào container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 25)))); // Điều chỉnh phiên bản MySQL nếu cần
builder.Services.AddRazorPages(); // Phải thêm dịch vụ trước khi gọi builder.Build()
builder.Services.AddHttpClient(); // Thêm các dịch vụ trước khi build ứng dụng
builder.Services.AddAntiforgery(options =>
{
    // Đặt tên cho header chứa token chống CSRF (có thể là XSRF-TOKEN hoặc tên bạn chọn)
    options.HeaderName = "XSRF-TOKEN";
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
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
