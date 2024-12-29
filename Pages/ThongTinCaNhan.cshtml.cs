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
    public class ThongTinCaNhanModel : PageModel
    {
        public string SearchQuery { get; set; }
        public int TotalPages { get; set; }
        private readonly ILogger<ThongTinCaNhanModel> _logger;
        private readonly MyDbContext _context;
        private readonly string _connectionString;

        public ThongTinCaNhanModel(ILogger<ThongTinCaNhanModel> logger, MyDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public NhanVien nhanVien { get; set; }
        public async Task<IActionResult> OnGet()
        {

            var userMaNhanVien = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userMaNhanVien))
            {
                return RedirectToPage("/Login");
            }

            nhanVien = await _context.NhanVien
            .Where(h => h.ma_nhan_vien.Equals(userMaNhanVien))
            .FirstAsync();

            return Page();
        }



    }
}


