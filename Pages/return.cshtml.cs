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
using System.Security.Cryptography;
using System;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace DoAnTotNghiep.Pages
{
    public class VnpayReturnModel : PageModel
    {
        private string vnp_HashSecret = "AOPUQQQ3BE1TTXQ9UA8DV6JSHY23VG19"; // Thay thế bằng Secret Key của bạn

        // Thông tin giao dịch sẽ được hiển thị trong view
        public bool IsSuccess { get; set; }
        public string TransactionNo { get; set; }
        public string Amount { get; set; }
        public string OrderInfo { get; set; }
        public string ErrorMessage { get; set; }
        public bool Success { get; set; }

        // Phương thức xử lý phản hồi từ VNPay
        public IActionResult OnGet()
        {
            // Lấy dữ liệu từ query string
            var query = Request.Query;
            var vnpAmount = query["vnp_Amount"];
            var vnpBankCode = query["vnp_BankCode"];
            var vnpBankTranNo = query["vnp_BankTranNo"];
            var vnpCardType = query["vnp_CardType"];
            var vnpOrderInfo = query["vnp_OrderInfo"];
            var vnpPayDate = query["vnp_PayDate"];
            var vnpResponseCode = query["vnp_ResponseCode"];
            var vnpTmnCode = query["vnp_TmnCode"];
            var vnpTransactionNo = query["vnp_TransactionNo"];
            var vnpTransactionStatus = query["vnp_TransactionStatus"];
            var vnpTxnRef = query["vnp_TxnRef"];
            var vnpSecureHash = query["vnp_SecureHash"];
        
            ViewData["vnpBankTranNo"] = vnpBankTranNo;
            if (vnpResponseCode == "00" && vnpTransactionStatus == "00")
            {
                Success = true;
            }
            else
            {
                Success = false;
            }

            return Page();
        }

        
    }
}