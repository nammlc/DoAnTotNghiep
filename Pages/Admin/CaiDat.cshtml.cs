using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DoAnTotNghiep.Models;
using DoAnTotNghiep.Data;
using System.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;


namespace DoAnTotNghiep.Pages
{
    public class TaiKhoanThanhToanModel : PageModel
    {
        private readonly MyDbContext _context;
        private readonly ILogger<TaiKhoanThanhToanModel> _logger;
        private readonly string _connectionString;


        public TaiKhoanThanhToanModel(MyDbContext context, ILogger<TaiKhoanThanhToanModel> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }
        public TaiKhoanThanhToan taiKhoanThanhToan { get; set; }

        public async Task<IActionResult> OnGet()
        {
            taiKhoanThanhToan = await _context.TaiKhoanThanhToan.FirstAsync();
            return Page();
        }

        public async Task<IActionResult> OnPost(TaiKhoanThanhToan account)
        {
            // Kiểm tra thông tin đầu vào
            if (account == null || string.IsNullOrEmpty(account.ten_ngan_hang) || string.IsNullOrEmpty(account.so_tai_khoan) || string.IsNullOrEmpty(account.ten_tai_khoan))
            {
                return new JsonResult(new { success = false, message = "Thông tin không đầy đủ!" });
            }

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    
                    string query = @"UPDATE tai_khoan_thanh_toan 
                             SET ten_ngan_hang = @ten_ngan_hang, 
                                 so_tai_khoan = @so_tai_khoan, 
                                 ten_tai_khoan = @ten_tai_khoan, 
                                 chi_nhanh_ngan_hang = @chi_nhanh_ngan_hang 
                             WHERE id = 1";  

                    var affectedRows = await connection.ExecuteAsync(query, new
                    {
                        ten_ngan_hang = account.ten_ngan_hang,
                        so_tai_khoan = account.so_tai_khoan,
                        ten_tai_khoan = account.ten_tai_khoan,
                        chi_nhanh_ngan_hang = account.chi_nhanh_ngan_hang
                    });

                    if (affectedRows == 0)
                    {
                        return NotFound();  
                    }

                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                    return RedirectToPage("/Admin/CaiDat");
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

    }
}