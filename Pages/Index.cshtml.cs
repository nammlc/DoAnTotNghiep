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
    public class IndexModel : PageModel
    {
        public string SearchQuery { get; set; }
        public int TotalPages { get; set; }
        private readonly ILogger<IndexModel> _logger;
        private readonly MyDbContext _context;
        private readonly string _connectionString;

        public IndexModel(ILogger<IndexModel> logger, MyDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IList<MonAn> MonAn { get; set; } = new List<MonAn>(); // Danh sách món ăn
        public IList<Bank> Banks { get; set; } = new List<Bank>(); // Danh sách ngân hàng
        public QRCodeViewModel QRCodeModel { get; set; } = new QRCodeViewModel();

        public async Task<IActionResult> OnGet(int? page, string searchQuery)
        {
            SearchQuery = searchQuery;

            var allDish = _context.MonAn.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                allDish = allDish.Where(e => e.ten_mon_an.Contains(searchQuery) || e.phan_loai_thuc_an.Contains(SearchQuery) || e.loai_mon_an.Contains(searchQuery));
            }
            allDish = allDish.OrderBy(m => m.ten_mon_an);
            int pageSize = 12;
            int pageNumber = page ?? 1;

            TotalPages = (int)Math.Ceiling(allDish.Count() / (double)pageSize);
            ViewData["TotalPages"] = TotalPages;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TongMonAn"] = allDish.Count();

            MonAn = allDish.ToList();
            // await GetAllBanks(); 
            return Page();
        }


        //Tạo mã qr
        [BindProperty]
        public string QRCodeUrl { get; set; } // Lưu trữ URL QR code

        public async Task<IActionResult> OnGetGenerateQRCode(decimal amount)
        {
            try
            {
                // Thông tin thanh toán
                string bankId = "TPBANK"; // Mã ngân hàng
                string accountNumber = "00003800981"; // Số tài khoản
                string template = "print"; // Mẫu
                string description = "Thanh Toan Hoa Don"; // Nội dung chuyển khoản
                string accountName = "LE CONG NAM"; // Tên người nhận

                // Tạo URL QuickLink
                string qrCodeUrl = $"https://img.vietqr.io/image/{bankId}-{accountNumber}-{template}.png?amount={amount}&addInfo={description}&accountName={accountName}";

                // Cập nhật giá trị QRCodeModel
                QRCodeModel.QRCodeUrl = qrCodeUrl;

                // Trả về JSON chứa QRCodeUrl
                return new JsonResult(new { QRCodeUrl = QRCodeModel.QRCodeUrl });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return new JsonResult(new { error = "Không thể tạo mã QR: " + ex.Message });
            }
        }



        public async Task<IActionResult> GetAllBanks()
        {
            Banks = await GetBanksAsync();
            return Page();
        }

        private async Task<IList<Bank>> GetBanksAsync()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("https://api.vietqr.io/v2/banks");

                // Kiểm tra mã trạng thái
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<BankResponse>();

                    if (result.Code == "00")
                    {
                        return result.Data; // Trả về danh sách ngân hàng
                    }
                    else
                    {
                        _logger.LogWarning("Lỗi khi lấy danh sách ngân hàng: {Desc}", result.Desc);
                        return new List<Bank>(); // Trả về danh sách rỗng nếu có lỗi
                    }
                }
                else
                {
                    _logger.LogError($"Lỗi truy cập API: {response.StatusCode}");
                    return new List<Bank>(); // Trả về danh sách rỗng nếu có lỗi
                }
            }
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
                HoaDons.tong_tien =  tongTien;
            }
            else
            {
                ModelState.AddModelError("tong_tien", "Giá trị 'tong_tien' không hợp lệ.");
                return Page();
            }

            HoaDons.gio_vao = DateTime.Now;

            // Kiểm tra và gán giá trị cho gio_ra (nếu có)
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
                // Để gio_ra trống hoặc gán giá trị mặc định (null)
                HoaDons.gio_ra = null; // Giả sử gio_ra là nullable trong mô hình HoaDon
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
                    ten_nhan_vien = "Lê Công Namm",
                    trang_thai = "Đang trong ca làm",
                    ten_kh = "hiện lênnnn",
                });
            }

            TempData["SuccessMessage"] = "Thêm hóa đơn thành công!";
            return RedirectToPage("Index");
        }



    }

}

public class Bank
{
    public int id { get; set; }
    public string name { get; set; }
    public string code { get; set; }
    public string bin { get; set; }
    public string shortName { get; set; }
    public string logo { get; set; }
    public int transferSupported { get; set; }
    public int lookupSupported { get; set; }
    public int support { get; set; }
    public int isTransfer { get; set; }
    public string swiftCode { get; set; }
}

public class BankResponse
{
    public string Code { get; set; }
    public string Desc { get; set; }
    public IList<Bank> Data { get; set; }
}

public class QRCodeViewModel
{
    public string QRCodeUrl { get; set; }
}


