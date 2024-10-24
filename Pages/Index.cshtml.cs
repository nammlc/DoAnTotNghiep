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

namespace DoAnTotNghiep.Pages
{
    public class IndexModel : PageModel
    {
        public  string SearchQuery{get; set ;}
        public int TotalPages{get; set;}
        private readonly ILogger<IndexModel> _logger;
        private readonly MyDbContext _context ;
        private readonly string _connectionString;

        public IndexModel(ILogger<IndexModel> logger, MyDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IList<MonAn> MonAn {get;set;} = new List<MonAn>(); // Danh sách món ăn
        public IList<Bank> Banks { get; set; } = new List<Bank>(); // Danh sách ngân hàng
        public QRCodeViewModel QRCodeModel { get; set; } = new QRCodeViewModel();


        //Lấy danh sách món ăn, khởi tạo dự án

        public async Task<IActionResult> OnGet(int? page, string searchQuery)
        {
            SearchQuery = searchQuery;

            var allDish = _context.MonAn.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                allDish = allDish.Where(e => e.ten_mon_an.Contains(searchQuery) || e.phan_loai_thuc_an.Contains(SearchQuery) || e.loai_mon_an.Contains(searchQuery));
            }

            int pageSize = 12;
            int pageNumber = page ?? 1;

            TotalPages = (int)Math.Ceiling(allDish.Count() / (double)pageSize);
            ViewData["TotalPages"] = TotalPages;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TongMonAn"] = allDish.Count();

            MonAn = allDish.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            await GenerateQRCode(); 
            await GetAllBanks(); 
            return Page();
        }
       
        //Tạo mã qr
        private async Task<IActionResult> GenerateQRCode()
        {
            try
            {
                // Thông tin thanh toán
                string bankId = "TPBANK"; // Mã ngân hàng
                string accountNumber = "00003800981"; // Số tài khoản
                string template = "print"; // Mẫu
                string amount = "500000"; // Số tiền
                string description = "Tra tien cho ta"; // Nội dung chuyển khoản
                string accountName = "LE CONG NAM"; // Tên người nhận

                // Tạo URL QuickLink
                string qrCodeUrl = $"https://img.vietqr.io/image/{bankId}-{accountNumber}-{template}.png?amount={amount}&addInfo={description}&accountName={accountName}";

                // Log giá trị hiện tại của QRCodeModel.QRCodeUrl trước khi cập nhật
                if (!string.IsNullOrEmpty(QRCodeModel.QRCodeUrl))
                {
                    _logger.LogInformation("Giá trị QRCodeModel trước khi cập nhật: {QRCodeUrl}", QRCodeModel.QRCodeUrl);
                }
                else
                {
                    _logger.LogInformation("QRCodeModel.QRCodeUrl hiện tại là rỗng.");
                }

                // Cập nhật giá trị mới
                QRCodeModel.QRCodeUrl = qrCodeUrl;

                // Log giá trị mới đã được cập nhật
                _logger.LogInformation("Giá trị QRCodeModel đã được cập nhật: {QRCodeUrl}", QRCodeModel.QRCodeUrl);

                return Page(); // Trả về trang hiện tại
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                ModelState.AddModelError(string.Empty, "Không thể tạo mã QR: " + ex.Message);
                return Page();
            }
        }
        // Phương thức được gọi khi nhấn nút
        public async Task<IActionResult> GetAllBanks() 
        {
            await GetBanksAsync(); 
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
}
