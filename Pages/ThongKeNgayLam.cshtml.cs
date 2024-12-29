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
using Microsoft.EntityFrameworkCore;


namespace DoAnTotNghiep.Pages
{
    public class ThongKeNgayLamModel : PageModel
    {
        public string SearchQuery { get; set; }
        public int TotalPages { get; set; }
        private readonly ILogger<ThongKeNgayLamModel> _logger;
        private readonly MyDbContext _context;
        private readonly string _connectionString;

        public ThongKeNgayLamModel(ILogger<ThongKeNgayLamModel> logger, MyDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IList<HoaDon> ListHoaDons { get; set; } = new List<HoaDon>();
        public async Task<IActionResult> OnGet()
        {
            // Lấy danh sách hóa đơn từ database
            var userMaNhanVien = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userMaNhanVien))
            {
                return RedirectToPage("/Login"); // Nếu chưa đăng nhập thì chuyển về trang login
            }

            ListHoaDons = await _context.HoaDon
                .Where(m => m.ten_nhan_vien.Equals(userMaNhanVien)) // Lọc hóa đơn theo mã nhân viên
                .ToListAsync();

            return Page();
        }



    }
}


