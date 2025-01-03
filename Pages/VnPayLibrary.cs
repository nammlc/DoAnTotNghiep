using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Net;
namespace DoAnTotNghiep.Pages
{
    public class VnPayLibrary
    {
        private SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayComparer());
        private SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayComparer());

        // Add request data
        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                _requestData[key] = value;
            }
        }

        // Get full request data
        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();

            foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            var querystring = data.ToString();

            baseUrl += "?" + querystring;
            var signData = querystring;
            if (signData.Length > 0)
            {
                signData = signData.Remove(data.Length - 1, 1);
            }

            var vnpSecureHash = HmacSha512(vnpHashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnpSecureHash;

            return baseUrl;
        }


        // Add response data
        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                _responseData[key] = value;
            }
        }

        // Get response data
        public PaymentResponseModel GetFullResponseData(IQueryCollection collections, string vnpHashSecret)
        {
            foreach (var item in collections)
            {
                AddResponseData(item.Key, item.Value);
            }

            if (!_responseData.TryGetValue("vnp_SecureHash", out var vnpSecureHash))
                return new PaymentResponseModel { Success = false, VnPayResponseCode = "No hash provided" };

            _responseData.Remove("vnp_SecureHash");
            _responseData.Remove("vnp_SecureHashType");

            var rawHash = BuildData();
            var checkSignature = HmacSHA512(vnpHashSecret, rawHash);

            if (checkSignature == vnpSecureHash)
            {
                return new PaymentResponseModel
                {
                    Success = true,
                    TransactionId = _responseData.GetValueOrDefault("vnp_TransactionNo"),
                    OrderId = _responseData.GetValueOrDefault("vnp_TxnRef"),
                    PaymentMethod = _responseData.GetValueOrDefault("vnp_CardType"),
                    VnPayResponseCode = _responseData.GetValueOrDefault("vnp_ResponseCode"),
                    OrderDescription = _responseData.GetValueOrDefault("vnp_OrderInfo")
                };
            }

            return new PaymentResponseModel { Success = false, VnPayResponseCode = "Invalid hash" };
        }

        // Get client IP address
        public string GetIpAddress(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        }
        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        // Build query string
        private string BuildData()
        {
            var data = new StringBuilder();
            foreach (var kv in _responseData)
            {
                data.Append(kv.Key + "=" + kv.Value + "&");
            }
            return data.ToString().TrimEnd('&');
        }

        // HMAC SHA512 encryption
        private static string HmacSHA512(string key, string inputData)
        {
            var keyByte = Encoding.UTF8.GetBytes(key);
            using var hmacsha512 = new HMACSHA512(keyByte);
            var data = hmacsha512.ComputeHash(Encoding.UTF8.GetBytes(inputData));
            return BitConverter.ToString(data).Replace("-", "").ToLower();
        }
    }

    public class VnPayComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null || y == null) return 0;
            return string.CompareOrdinal(x, y);
        }
    }
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
