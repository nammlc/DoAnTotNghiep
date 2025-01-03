using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

using System.Globalization;
using DoAnTotNghiep.Models;
using DoAnTotNghiep.Data;

namespace DoAnTotNghiep.Pages
{
    public class NhanVienModel : PageModel
    {
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int ToTalStaff { get; set; }
        private readonly string _connectionString;
        private readonly MyDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public NhanVienModel(MyDbContext context, IConfiguration configuration, ILogger<IndexModel> logger)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public IList<NhanVien> NhanViens { get; set; }
        public string SearchQuery { get; set; }
        public int Page { get; set; }


        // Hiển thị danh sách nhân viên với phân trang
        [HttpGet]
        public IActionResult OnGet(int? page, string searchQuery)
        {
            SearchQuery = searchQuery;

            var allEmployees = _context.NhanVien.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                allEmployees = allEmployees.Where(e => e.ten_nhan_vien.Contains(searchQuery) || e.ma_nhan_vien.Contains(SearchQuery));
            }
            allEmployees = allEmployees.OrderBy(m => m.ten_nhan_vien);
            int pageSize = 10;
            int pageNumber = page ?? 1;

            TotalPages = (int)Math.Ceiling(allEmployees.Count() / (double)pageSize);
            ViewData["TotalPages"] = TotalPages;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["ToTalStaff"] = allEmployees.Count();

            NhanViens = allEmployees.ToList();

            return Page();
        }


        // Hiển thị form tạo mới nhân viên
        public IActionResult OnGetCreate()
        {
            return Page();
        }

        [BindProperty]
        public NhanVien nhanVien { get; set; }

        //Thêm mới nhân viên
        [HttpPost]
        public async Task<IActionResult> OnPostNhanVienAsync()
        {
            if (ModelState.IsValid)
            {
                nhanVien.ten_dang_nhap = await TaoTenDangNhapDuyNhat(nhanVien.ten_nhan_vien);
                nhanVien.ten_dang_nhap = nhanVien.ten_dang_nhap.ToUpper();
                nhanVien.ma_nhan_vien = GenerateUniqueEmployeeCode();
                _logger.LogInformation($"Lưu nhân viên có tên đăng nhập: {nhanVien.ten_dang_nhap}");
                using (var db = new MySqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO nhanvien (ten_nhan_vien, ma_nhan_vien, ngay_sinh, gioi_tinh, vi_tri, so_cmnd, ngay_cap_cmnd, noi_cap_cmnd, dia_chi, so_dien_thoai, so_dien_thoai_co_dinh, email, so_tai_khoan_ngan_hang, ten_ngan_hang, chi_nhanh_ngan_hang,mat_khau, ten_dang_nhap)
                             VALUES (@ten_nhan_vien, @ma_nhan_vien, @ngay_sinh, @gioi_tinh, @vi_tri, @so_cmnd, @ngay_cap_cmnd, @noi_cap_cmnd, @dia_chi, @so_dien_thoai, @so_dien_thoai_co_dinh, @email, @so_tai_khoan_ngan_hang, @ten_ngan_hang, @chi_nhanh_ngan_hang , 123456, @ten_dang_nhap)";
                    await db.ExecuteAsync(query, nhanVien);
                }

                TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                return RedirectToPage("/Admin/NhanVien");
            }
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm nhân viên.";
            return RedirectToPage("/Admin/NhanVien");
        }

        //Tạo mã nhân viên
        private string GenerateUniqueEmployeeCode()
        {
            using (var db = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ma_nhan_vien FROM nhanvien ORDER BY ma_nhan_vien DESC LIMIT 1";
                var lastEmployeeCode = db.QueryFirstOrDefault<string>(query);

                if (lastEmployeeCode == null)
                {
                    return "NV001";
                }

                int newCodeNumber = int.Parse(lastEmployeeCode.Substring(2)) + 1;
                return "NV" + newCodeNumber.ToString("D3");
            }
        }
        public static string TaoVietTat(string tenNhanVien)
        {
            if (string.IsNullOrEmpty(tenNhanVien))
                throw new ArgumentNullException("Tên nhân viên không được để trống.");

            // Loại bỏ dấu tiếng Việt
            string tenKhongDau = tenNhanVien.Trim();

            // Tách tên thành các phần
            string[] parts = tenKhongDau.Split(' ');

            // Kiểm tra nếu tên không hợp lệ
            if (parts.Length < 2)
            {
                throw new ArgumentException("Tên nhân viên phải có ít nhất họ và tên.");
            }

            // Lấy chữ cái đầu tiên của mỗi phần (họ và tên đệm)
            string vietTat = string.Join("", parts.Take(parts.Length - 1).Select(p => p[0].ToString().ToUpper()));

            // Lấy chữ cái đầu tiên của tên cuối cùng
            vietTat += parts[parts.Length - 1][0].ToString().ToUpper();

            return vietTat;
        }

        private async Task<string> TaoTenDangNhapDuyNhat(string tenNhanVien)
        {
            string tenDangNhap = TaoVietTat(tenNhanVien);
            using (var db = new MySqlConnection(_connectionString))
            {
                // Kiểm tra tên đăng nhập đã tồn tại chưa
                string query = "SELECT COUNT(*) FROM nhanvien WHERE ten_dang_nhap = @tenDangNhap";

                // Thêm từ khóa await để đợi kết quả trả về từ ExecuteScalarAsync
                int count = await db.ExecuteScalarAsync<int>(query, new { tenDangNhap });

                if (count > 0)
                {
                    int i = 1;
                    string newTenDangNhap;
                    do
                    {
                        newTenDangNhap = tenDangNhap + i.ToString(); // Thêm số vào tên đăng nhập
                        i++;
                        // Thêm từ khóa await để đợi kết quả trả về từ ExecuteScalarAsync
                        count = await db.ExecuteScalarAsync<int>(query, new { tenDangNhap = newTenDangNhap });
                    }
                    while (count > 0);

                    return newTenDangNhap; // Trả về tên đăng nhập duy nhất
                }

                return tenDangNhap;
            }
        }

        //Xóa nhân viên
        [HttpDelete]
        public async Task<IActionResult> OnDeleteAsync(int id)
        {
            using (var db = new MySqlConnection(_connectionString))
            {
                string query = "DELETE FROM nhanvien WHERE id = @Id";
                var affectedRows = await db.ExecuteAsync(query, new { Id = id });

                if (affectedRows == 0)
                {
                    return NotFound("Nhân viên không tồn tại.");
                }

                return new JsonResult(new { message = "Xóa thành công." }) { StatusCode = 200 };
            }
        }

        //tìm nhân viên để chỉnh sửa
        [HttpGet]
        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            _logger.LogInformation($"Đang tìm nhân viên với ID: {id}");

            using (var db = new MySqlConnection(_connectionString))
            {
                string query = "SELECT * FROM NhanVien WHERE id = @Id";
                var nhanVien = await db.QueryFirstOrDefaultAsync<NhanVien>(query, new { Id = id });

                if (nhanVien == null)
                {
                    return NotFound();
                }
                return Partial("_EditNhanVien", nhanVien);
            }
        }

        // Lưu thông tin nhân viên
        [HttpPost]
        [Route("NhanVien/EditNhanVien")]
        public async Task<IActionResult> OnPostEditNhanVienAsync(int id, NhanVien nhanVien)

        {

            if (id != nhanVien.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _logger.LogInformation($"Lưu nhân viên có ID: {id}");
                using (var db = new MySqlConnection(_connectionString))
                {
                    string query = "UPDATE nhanvien SET ten_nhan_vien = @ten_nhan_vien, ma_nhan_vien = @ma_nhan_vien, ngay_sinh = @ngay_sinh, gioi_tinh = @gioi_tinh, vi_tri = @vi_tri, ngay_cap_cmnd = @ngay_cap_cmnd, noi_cap_cmnd = @noi_cap_cmnd, dia_chi = @dia_chi, so_dien_thoai = @so_dien_thoai, so_dien_thoai_co_dinh = @so_dien_thoai_co_dinh,  so_tai_khoan_ngan_hang = @so_tai_khoan_ngan_hang, ten_ngan_hang = @ten_ngan_hang, chi_nhanh_ngan_hang = @chi_nhanh_ngan_hang WHERE id = @id";
                    var affectedRows = await db.ExecuteAsync(query, nhanVien);

                    if (affectedRows == 0)
                    {
                        return NotFound();
                    }

                    TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
                    return RedirectToPage("/Admin/NhanVien");
                }
            }
            return Page();
        }

    }
}