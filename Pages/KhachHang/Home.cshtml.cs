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


namespace DoAnTotNghiep.Pages
{
    public class HomeModel : PageModel
    {
        public string SearchQuery { get; set; }
        public int TotalPages { get; set; }
        private readonly ILogger<HomeModel> _logger;
        private readonly MyDbContext _context;
        private readonly string _connectionString;
        public int CurrentPage { get; set; } = 1;


        public HomeModel(ILogger<HomeModel> logger, MyDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IList<MonAn> MonAn { get; set; } = new List<MonAn>(); // Danh sách món ăn


        // Số món ăn trên mỗi trang
        private const int ItemsPerPage = 12;

        public void OnGet()
        {
            MonAn = _context.MonAn.ToList();
        }

        //Tạo đơn hàng 
        public HoaDon HoaDons { get; set; }
        [HttpPost]
        public async Task<IActionResult> OnPostAsync()
        {
            if (HoaDons == null)
            {
                HoaDons = new HoaDon();
            }

            // Chuyển đổi và gán giá trị cho tong_tien
            if (long.TryParse(Request.Form["tong_tien"], out long tongTien))
            {
                HoaDons.tong_tien = tongTien;
            }
            else
            {
                ModelState.AddModelError("tong_tien", "Giá trị 'tong_tien' không hợp lệ.");
                return Page();
            }

            HoaDons.gio_vao = DateTime.Now;


            if (!string.IsNullOrEmpty(Request.Form["gio_ra"]))
            {
                if (DateTime.TryParse(Request.Form["gio_ra"], out DateTime gioRa))
                {
                    HoaDons.gio_ra = gioRa;
                }
                else
                {
                    ModelState.AddModelError("gio_ra", "Giá trị 'gio_ra' không hợp lệ.");
                    return Page();
                }
            }
            else
            {

                HoaDons.gio_ra = null;
            }

            // Gán giá trị cho các thuộc tính còn lại
            HoaDons.list_mon_an = Request.Form["list_mon_an"];
            HoaDons.ten_ban = Request.Form["ten_ban"];

            using (var db = new MySqlConnection(_connectionString))
            {
                string query = @"INSERT INTO hoa_don (tong_tien, gio_vao, gio_ra, list_mon_an, ten_ban, ten_nhan_vien, trang_thai,ten_kh)
                         VALUES (@tong_tien, @gio_vao, @gio_ra, @list_mon_an, @ten_ban,@ten_nhan_vien, @trang_thai, @ten_kh)";

                await db.ExecuteAsync(query, new HoaDon
                {
                    tong_tien = HoaDons.tong_tien,
                    gio_vao = HoaDons.gio_vao,
                    gio_ra = HoaDons.gio_ra,
                    list_mon_an = HoaDons.list_mon_an,
                    ten_ban = HoaDons.ten_ban,
                    ten_nhan_vien = "Role Client",
                    trang_thai = "Chưa hoàn thành",
                    ten_kh = "Client",
                });
            }

            TempData["SuccessMessage"] = "Thêm hóa đơn thành công!";
            return RedirectToPage("/Home");
        }



    }

}


