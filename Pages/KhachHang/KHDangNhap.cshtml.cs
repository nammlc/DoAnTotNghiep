using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DoAnTotNghiep.Models;
using DoAnTotNghiep.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

public class DangNhapClientModel : PageModel
{
    private readonly string _connectionString;
    [BindProperty]
    public string ten_dang_nhap { get; set; }
    [BindProperty]
    public string mat_khau { get; set; }

    public string ErrorMessage { get; set; }
    private readonly ILogger<DangNhapClientModel> _logger;
    private readonly MyDbContext _context;
    public KhachHangs khachHang { get; set; }
    public DangNhapClientModel(MyDbContext context, IConfiguration configuration, ILogger<DangNhapClientModel> logger)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _logger = logger;
    }
    public void OnGet()
    {

    }
    public IActionResult OnPostLogout()
    {
        
        HttpContext.Session.Clear();

        
        HttpContext.Response.Cookies.Delete(".AspNetCore.Cookies"); 

        
        return RedirectToPage("/KhachHang/KHDangNhap");
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrEmpty(ten_dang_nhap) || string.IsNullOrEmpty(mat_khau))
        {
            ErrorMessage = "Tên đăng nhập và mật khẩu không được để trống.";
            return Page();
        }

        using (var connection = new MySqlConnection(_connectionString))
        {

            string query = "SELECT * FROM khach_hang WHERE ten_dang_nhap = @ten_dang_nhap AND mat_khau = @mat_khau";
            var user = connection.QueryFirstOrDefault<KhachHangs>(query, new { ten_dang_nhap, mat_khau });

            if (user != null)
            {

                HttpContext.Session.SetString("User", user.ho_ten);
                HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(
                        new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.ho_ten) },
                        "login"
                    )
                );
                TempData["UserName"] = user.ho_ten;
                return RedirectToPage("/KhachHang/TrangChu");
            }
            else
            {
                ErrorMessage = "Sai tên đăng nhập hoặc mật khẩu.";

                return Page();
            }
        }
    }

    [BindProperty]
    public KhachHangs khachHangs { get; set; }
    public IActionResult OnPostRegister(){
        _logger.LogInformation($"Lưu khách hàng có tên đăng nhập: {khachHangs.ten_dang_nhap}");
        if (ModelState.IsValid)
        {

            _logger.LogInformation($"Lưu khách hàng có tên đăng nhập: {khachHangs.ten_dang_nhap}");
            using (var db = new MySqlConnection(_connectionString))
            {
                string query = @"INSERT INTO khach_hang (ho_ten, dia_chi, so_dien_thoai, ten_dang_nhap, mat_khau, gioi_tinh)
                             VALUES (@ho_ten, @dia_chi, @so_dien_thoai, @ten_dang_nhap, @mat_khau, @gioi_tinh)";
                db.ExecuteAsync(query, khachHangs);
            }

            TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
            return RedirectToPage("/KhachHang/KHDangNhap");
        }
        else{
             
             return RedirectToPage("/KhachHang/KHDangKy");
        
        
    }
    }
}

