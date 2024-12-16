using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DoAnTotNghiep.Models;
using DoAnTotNghiep.Data;
using System.Data.SqlClient;
using System.Text.Json;
using MySql.Data.MySqlClient;
using Dapper;
using Mysqlx;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace DoAnTotNghiep.Pages
{
    public class TrangCaNhanModel : PageModel
    {
        public string SearchQuery { get; set; }
        public int TotalPages { get; set; }
        private readonly ILogger<TrangCaNhanModel> _logger;
        private readonly MyDbContext _context;
        private readonly string _connectionString;
        public int CurrentPage { get; set; } = 1;
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }


        public List<HoaDon> HoaDons { get; set; }

        public TrangCaNhanModel(ILogger<TrangCaNhanModel> logger, MyDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            var userName = HttpContext.Session.GetString("User");

            if (userName != null)
            {
                // Lấy thông tin cá nhân từ cơ sở dữ liệu (giả sử đã có bảng khách hàng)
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var query = "SELECT * FROM khach_hang WHERE ho_ten = @hoTen";
                    var user = connection.QueryFirstOrDefault<KhachHangs>(query, new { hoTen = userName });

                    if (user != null)
                    {
                        HoTen = user.ho_ten;
                        SoDienThoai = user.so_dien_thoai;
                        DiaChi = user.dia_chi;

                    }
                    string qr = "On Cart";
                    // Lấy đơn hàng của khách hàng
                    var ordersQuery = "SELECT * FROM hoa_don WHERE ten_kh = @userId && trang_thai != @qr";
                    HoaDons = connection.Query<HoaDon>(ordersQuery, new { userId = user.ho_ten, qr }).ToList();
                }
            }
        }
        public IActionResult OnPostUpdateInfo([FromBody] KhachHangs updateData)
        {
            var userName = HttpContext.Session.GetString("User");

            if (userName != null)
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var updateQuery = "UPDATE khach_hang SET so_dien_thoai = @SoDienThoai, dia_chi = @DiaChi WHERE ho_ten = @HoTen";
                    connection.Execute(updateQuery, new { SoDienThoai = updateData.so_dien_thoai, DiaChi = updateData.dia_chi, HoTen = userName });
                }
                return new JsonResult(new { success = true, message = "Cập nhật thông tin thành công!" });
            }
            return new JsonResult(new { success = false, message = "Cập nhật thất bại!" });
        }


    }
}







