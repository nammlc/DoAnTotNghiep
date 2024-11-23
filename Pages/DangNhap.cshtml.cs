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

public class DangNhapModel : PageModel
{
    private readonly string _connectionString;
    [BindProperty]
    public string ten_dang_nhap { get; set; }
    [BindProperty]
    public string mat_khau { get; set; }

    public string ErrorMessage { get; set; }
    private readonly ILogger<DangNhapModel> _logger;
    private readonly MyDbContext _context;
    public NhanVien nhanVien { get; set; }
    public DangNhapModel(MyDbContext context, IConfiguration configuration, ILogger<DangNhapModel> logger)
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

        
        return RedirectToPage("/DangNhap");
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

            string query = "SELECT * FROM nhanvien WHERE ten_dang_nhap = @ten_dang_nhap AND mat_khau = @mat_khau";
            var user = connection.QueryFirstOrDefault<NhanVien>(query, new { ten_dang_nhap, mat_khau });

            if (user != null)
            {

                HttpContext.Session.SetString("User", user.ten_nhan_vien);
                HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(
                        new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.ten_nhan_vien) },
                        "login"
                    )
                );
                return RedirectToPage("/Home");
            }
            else
            {
                ErrorMessage = "Sai tên đăng nhập hoặc mật khẩu.";

                return Page();
            }
        }
    }
}
