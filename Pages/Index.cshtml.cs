using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DoAnTotNghiep.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IList<Bank> Banks { get; set; } = new List<Bank>(); // Danh sách ngân hàng
        public QRCodeViewModel QRCodeModel { get; set; } = new QRCodeViewModel();

        public async Task<IActionResult> OnGet()
        {
            await GenerateQRCode(); // Tạo mã QR khi truy cập trang
            await GetAllBanks(); // Lấy danh sách ngân hàng khi truy cập trang
            return Page();
        }
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

        public async Task<IActionResult> GetAllBanks() // Phương thức này sẽ được gọi khi nhấn nút
        {
            Banks = await GetBanksAsync(); // Gọi phương thức để lấy danh sách ngân hàng
            return Page(); // Trả về trang để cập nhật danh sách ngân hàng
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
                    _logger.LogInformation("Kết quả từ API: {@Result}", result);
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
