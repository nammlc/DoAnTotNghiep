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

        // public void OnGet()
        // {
        //     MonAn = _context.MonAn.ToList();
        // }

        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            _logger.LogInformation($"Đang tìm món ăn với ID: {id}");

            using (var db = new MySqlConnection(_connectionString))
            {
                // Truy vấn món ăn được chọn
                string queryMainDish = "SELECT * FROM mon_an WHERE id = @Id";
                var monAn = await db.QueryFirstOrDefaultAsync<MonAn>(queryMainDish, new { Id = id });

                if (monAn == null)
                {
                    return NotFound();
                }

                // Truy vấn các món ăn liên quan cùng loại (ngoại trừ món ăn hiện tại)
                string queryRelatedDishes = "SELECT * FROM mon_an WHERE phan_loai_thuc_an = @Category AND id != @Id";
                var relatedDishes = await db.QueryAsync<MonAn>(queryRelatedDishes, new { Category = monAn.phan_loai_thuc_an, Id = id });

                // Truyền cả món ăn chính và danh sách món ăn liên quan vào model
                var model = new ChiTietMonAnViewModel
                {
                    MonAnChinh = monAn,
                    MonAnLienQuan = relatedDishes.ToList()
                };

                return Partial("_ChiTietMonAn", model);
            }
        }

        //tạo hóa đơn
        public HoaDon hoaDons { get; set; }
        [HttpPost]
        public async Task<IActionResult> OnPostCreateBill()
        {
            if (hoaDons == null)
            {
                hoaDons = new HoaDon();
            }

            // Chuyển đổi và gán giá trị cho tong_tien
            if (long.TryParse(Request.Form["tong_tien"], out long tongTien))
            {
                hoaDons.tong_tien = tongTien;
            }

            hoaDons.gio_vao = DateTime.Now;
            _logger.LogInformation("Dữ liệu nhận được: tong_tien={tong_tien}, list_mon_an={list_mon_an}",
                Request.Form["tong_tien"], Request.Form["list_mon_an"]);



            if (DateTime.TryParse(Request.Form["gio_ra"], out DateTime gioRa))
            {
                hoaDons.gio_ra = gioRa;
            }

            // Gán giá trị cho các thuộc tính còn lại
            hoaDons.list_mon_an = Request.Form["list_mon_an"];
            hoaDons.ten_ban = Request.Form["ten_ban"];

            using (var db = new MySqlConnection(_connectionString))
            {
                string query = @"INSERT INTO hoa_don (tong_tien, gio_vao, gio_ra, list_mon_an, ten_ban, ten_nhan_vien, trang_thai,ten_kh)
                         VALUES (@tong_tien, @gio_vao, @gio_ra, @list_mon_an, @ten_ban,@ten_nhan_vien, @trang_thai, @ten_kh)";

                await db.ExecuteAsync(query, new HoaDon
                {
                    tong_tien = 10000,
                    gio_vao = hoaDons.gio_vao,
                    gio_ra = hoaDons.gio_ra,
                    list_mon_an = hoaDons.list_mon_an,
                    ten_ban = hoaDons.ten_ban,
                    ten_nhan_vien = "Client",
                    trang_thai = "Chưa hoàn thành",
                    ten_kh = "Client",
                });
            }

            TempData["SuccessMessage"] = "Thêm hóa đơn thành công!";
            return RedirectToPage("/KhachHang/TrangChu");
        }

        //Danh sách hóa đơn
        public IList<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

        public async Task<IActionResult> OnGet(int? page, string searchQuery)
        {
            SearchQuery = searchQuery;

            var allBill = _context.HoaDon
                .Select(hd => new HoaDon
                {
                    id = hd.id,
                    tong_tien = hd.tong_tien,
                    gio_vao = hd.gio_vao ?? DateTime.Now,
                    gio_ra = hd.gio_ra ?? DateTime.Now,
                    list_mon_an = hd.list_mon_an ?? "",
                    ten_ban = hd.ten_ban ?? "",
                    ten_nhan_vien = hd.ten_nhan_vien ?? "",
                    trang_thai = hd.trang_thai ?? "",
                    ten_kh = hd.ten_kh
                })

                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                allBill = allBill.Where(e => e.ten_ban.Contains(searchQuery));
            }

            allBill = allBill.OrderByDescending(m => m.gio_vao);
            int countt = 0 ;
            
           
            HoaDons = allBill.ToList();
            MonAn = _context.MonAn.ToList();
            long tongTienTrongCa = 0;
            foreach (var hoadon in HoaDons)
            {
                if (hoadon.ten_nhan_vien == "Client")
                {
                    tongTienTrongCa = tongTienTrongCa + hoadon.tong_tien;
                    countt ++ ;
                }
            }
            ViewData["tongTienTrongCa"] = tongTienTrongCa.ToString("#,##0");
            ViewData["TotalBill"] = countt;
            return Page();
        }





    }
    public class ChiTietMonAnViewModel
    {
        public MonAn MonAnChinh { get; set; }
        public List<MonAn> MonAnLienQuan { get; set; }
    }

}


