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
using Microsoft.EntityFrameworkCore;


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

        //tạo giỏ hàng
        public HoaDon hoaDons { get; set; }
        [HttpPost]
        public async Task<IActionResult> OnPostCreateCartItem()
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


            hoaDons.list_mon_an = Request.Form["list_mon_an"];
            hoaDons.ten_ban = Request.Form["ten_ban"];
            string soLuongString = Request.Form["so_luong"];


            if (long.TryParse(soLuongString, out long soLuong))
            {
                hoaDons.tong_tien = soLuong;
            }
            else
            {

                hoaDons.tong_tien = 0;
            }


            using (var db = new MySqlConnection(_connectionString))
            {
                string query = @"INSERT INTO hoa_don (tong_tien, gio_vao, gio_ra, list_mon_an, ten_ban, ten_nhan_vien, trang_thai,ten_kh)
                         VALUES (@tong_tien, @gio_vao, @gio_ra, @list_mon_an, @ten_ban,@ten_nhan_vien, @trang_thai, @ten_kh)";

                await db.ExecuteAsync(query, new HoaDon
                {
                    tong_tien = hoaDons.tong_tien,
                    gio_vao = hoaDons.gio_vao,
                    gio_ra = hoaDons.gio_ra,
                    list_mon_an = hoaDons.list_mon_an,
                    ten_ban = "Client",
                    ten_nhan_vien = "Client",
                    trang_thai = "On Cart",
                    ten_kh = HttpContext.Session.GetString("User"),
                });
            }

            TempData["SuccessMessage"] = "Thêm hóa đơn thành công!";
            return RedirectToPage("/KhachHang/TrangChu");
        }


        //Tạo hóa đơn 

        public async Task<IActionResult> OnPostCreateBill()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();

                // Cấu hình JsonSerializerOptions
                var jsonOptions = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNameCaseInsensitive = true
                };

                // Giải mã dữ liệu JSON
                var orderData = JsonSerializer.Deserialize<OrderData>(body, jsonOptions);
                string pt_thanh_toan;
                if (orderData.pt_thanh_toan == "cash")
                {
                    pt_thanh_toan = "Đang chờ xét duyệt - Chưa thanh toán";
                }
                else
                {
                    pt_thanh_toan = "Đang chờ xét duyệt - Đã thanh toán";

                }

                var hoaDon = new HoaDon
                {
                    tong_tien = orderData.tong_tien,
                    gio_vao = DateTime.Now,
                    gio_ra = DateTime.Now,
                    list_mon_an = JsonSerializer.Serialize(orderData.list_mon_an, jsonOptions),
                    ten_ban = orderData.dia_chi_giao_hang,
                    ten_nhan_vien = orderData.so_dien_thoai_giao_hang,
                    trang_thai = pt_thanh_toan,
                    ten_kh = HttpContext.Session.GetString("User")
                };

                using (var db = new MySqlConnection(_connectionString))
                {
                    await db.OpenAsync();
                    using (var transaction = await db.BeginTransactionAsync())
                    {
                        try
                        {
                            // Thêm hóa đơn
                            string insertBillQuery = @"INSERT INTO hoa_don (tong_tien, gio_vao, gio_ra, list_mon_an, ten_ban, ten_nhan_vien, trang_thai, ten_kh)
                                           VALUES (@tong_tien, @gio_vao, @gio_ra, @list_mon_an, @ten_ban, @ten_nhan_vien, @trang_thai, @ten_kh)";
                            await db.ExecuteAsync(insertBillQuery, hoaDon, transaction);

                            // Xóa hóa đơn cũ theo ID
                            if (orderData.idsToDelete != null && orderData.idsToDelete.Any())
                            {
                                string deleteQuery = "DELETE FROM hoa_don WHERE id IN @idsToDelete";
                                await db.ExecuteAsync(deleteQuery, new { idsToDelete = orderData.idsToDelete }, transaction);
                            }

                            await transaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            TempData["ErrorMessage"] = $"Lỗi khi thêm hóa đơn: {ex.Message}";
                            return RedirectToPage("/KhachHang/TrangChu");
                        }
                    }
                }

                TempData["SuccessMessage"] = "Thêm hóa đơn thành công!";
                return RedirectToPage("/KhachHang/TrangChu");
            }
        }

        //Tạo mã qr
        public QRCodeViewModel QRCodeModel { get; set; } = new QRCodeViewModel();
        public TaiKhoanThanhToan taiKhoanThanhToan { get; set; }

        public async Task<IActionResult> OnGetGenerateQRCode(decimal amount)
        {
            try
            {
                taiKhoanThanhToan = await _context.TaiKhoanThanhToan.FirstAsync();
                string bankId = taiKhoanThanhToan.ten_ngan_hang;
                string accountNumber = taiKhoanThanhToan.so_tai_khoan;
                string template = "1";
                string description = "Thanh Toan Hoa Don";
                string accountName = taiKhoanThanhToan.ten_tai_khoan;

                string qrCodeUrl = $"https://img.vietqr.io/image/{bankId}-{accountNumber}-{template}.png?amount={amount}&addInfo={description}&accountName={accountName}";

                QRCodeModel.QRCodeUrl = qrCodeUrl;
                return Content(QRCodeModel.QRCodeUrl);
            }
            catch (Exception ex)
            {

                return Content("Không thể tạo mã QR: " + ex.Message);
            }
        }
        //Danh sách hóa đơn
        public IList<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
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
            int countt = 0;


            HoaDons = allBill.ToList();
            MonAn = _context.MonAn.ToList();
            long tongTienTrongCa = 0;
            foreach (var hoadon in HoaDons)
            {
                if (hoadon.trang_thai == "On Cart" && hoadon.ten_kh == HttpContext.Session.GetString("User"))
                {
                    tongTienTrongCa = tongTienTrongCa + hoadon.tong_tien;
                    countt++;
                }
            }
            ViewData["tongTienTrongCa"] = tongTienTrongCa.ToString("#,##0");
            ViewData["TotalBill"] = countt;
            var userName = HttpContext.Session.GetString("User");

            if (userName != null)
            {
                // Lấy thông tin cá nhân 
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var query = "SELECT * FROM khach_hang WHERE ho_ten = @hoTen";
                    var user = connection.QueryFirstOrDefault<KhachHangs>(query, new { hoTen = userName });

                    if (user != null)
                    {
                        SoDienThoai = user.so_dien_thoai;
                        DiaChi = user.dia_chi;

                    }
                }
            }
            return Page();
        }


        //Xóa mặt hàng
        [HttpDelete]
        public async Task<IActionResult> OnDeleteAsync(int id)
        {
            using (var db = new MySqlConnection(_connectionString))
            {
                string query = "DELETE FROM hoa_don WHERE id = @Id";
                var affectedRows = await db.ExecuteAsync(query, new { Id = id });

                if (affectedRows == 0)
                {
                    return NotFound("Hóa đơn không tồn tại.");
                }

                return new JsonResult(new { message = "Xóa thành công." }) { StatusCode = 200 };
            }
        }
        public string DecodeUnicode(string input)
        {
            return JsonSerializer.Deserialize<string>($"\"{input}\"");
        }




    }
    public class ChiTietMonAnViewModel
    {
        public MonAn MonAnChinh { get; set; }
        public List<MonAn> MonAnLienQuan { get; set; }
    }

    public class OrderData
    {
        public long tong_tien { get; set; }
        public DateTime gio_vao { get; set; }
        public List<Dish> list_mon_an { get; set; } // Danh sách món ăn
        public List<int> idsToDelete { get; set; }  // Danh sách ID cần xóa
        public string so_dien_thoai_giao_hang { get; set; }
        public string dia_chi_giao_hang { get; set; }
        public string pt_thanh_toan { get; set; }


    }

    public class Dish
    {
        public string ten_mon { get; set; }
        public decimal gia_tien { get; set; }
        public string so_luong { get; set; } // Chấp nhận kiểu chuỗi
        public string anh_minh_hoa { get; set; }

        public int SoLuongAsInt()
        {
            return int.TryParse(so_luong, out var result) ? result : 0;
        }
    }
    public class QRCodeViewModel
    {
        public string QRCodeUrl { get; set; }
    }



}


