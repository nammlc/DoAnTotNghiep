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

        public void OnGet()
        {
            // Tùy chọn: Khởi tạo hoặc ghi lại điều gì đó nếu cần
        }

        public async Task<IActionResult> OnPostGenerateQR()
        {
            // Thông tin thanh toán
            string bankId = "MBBANK"; // Mã ngân hàng
            string accountNumber = "0560 1431 7146 6"; // Số tài khoản
            string template = "1"; // Mẫu
            string amount = "1000000"; // Số tiền
            string description = "Thanh Toán "; // Nội dung chuyển khoản
            string accountName = "NGUYEN HOANG LINH"; // Tên người nhận

            // Tạo URL QuickLink
            string qrUrl = $"https://img.vietqr.io/image/{bankId}-{accountNumber}-{template}.png?amount={amount}&addInfo={description}&accountName={System.Net.WebUtility.UrlEncode(accountName)}";

            // Tạo mã QR từ URL
            byte[] qrCodeBytes = GenerateQRCodeFromUrl(qrUrl);

            // Trả về mã QR dưới dạng FileResult (ảnh PNG)
            return File(qrCodeBytes, "image/png");
        }

        public async Task<IActionResult> OnPostGetAllBanks() // Phương thức này sẽ được gọi khi nhấn nút
        {
            Banks = await GetBanksAsync(); // Gọi phương thức để lấy danh sách ngân hàng

            // Ghi lại kết quả vào log để kiểm tra
            if (Banks != null && Banks.Count > 0)
            {
                _logger.LogInformation("Kết quả từ API: {@Banks}", Banks);
            }
            else
            {
                _logger.LogWarning("Không có ngân hàng nào được tìm thấy.");
            }

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

        private byte[] GenerateQRCodeFromUrl(string url)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsByteArrayAsync().Result;
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
}
