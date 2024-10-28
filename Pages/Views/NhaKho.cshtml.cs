using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DoAnTotNghiep.Models;
using DoAnTotNghiep.Data;

namespace DoAnTotNghiep.Pages
{
    public class NhaKhoModel : PageModel
    {
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public bool HasPreviousPage => CurrentPage > 1; 
        public bool HasNextPage => CurrentPage < TotalPages;
        public int PageSize { get; set; } 
        public int TotalCount { get; set; }
        public int ToTalStaff {get; set ;}
        private readonly string _connectionString;
        private readonly MyDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public NhaKhoModel(MyDbContext context, IConfiguration configuration, ILogger<IndexModel> logger)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public IList<NhaKho> NhaKhos { get; set; }
        public string SearchQuery { get; set; }
        public int Page { get; set; }


        // Hiển thị danh sách nguyên liệu với phân trang
        [HttpGet]
        public IActionResult OnGet(int? page, string searchQuery)
        {
            SearchQuery = searchQuery;

            var allStocks = _context.NhaKho.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                allStocks = allStocks.Where(e => e.ten_nguyen_lieu.Contains(searchQuery) || e.nha_cung_cap.Contains(SearchQuery));
            }
            allStocks = allStocks.OrderBy(m => m.ten_nguyen_lieu); 
            int pageSize = 10;
            int pageNumber = page ?? 1;

            TotalPages = (int)Math.Ceiling(allStocks.Count() / (double)pageSize);
            ViewData["TotalPages"] = TotalPages;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["ToTalStaff"] = allStocks.Count();

            NhaKhos = allStocks.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Page();
        }


        // Hiển thị form thêm nguyên liệu
        public IActionResult OnGetCreate()
        {
            return Page();
        }

        [BindProperty]
        public NhaKho nhaKho { get; set; }

        //Thêm mới nguyên liệu
        [HttpPost]
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // nhaKho.ma_nguyen_lieu = GenerateUniqueIngredientCode();

                using (var db = new MySqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO nha_kho (ten_nguyen_lieu, gia_nguyen_lieu, don_vi, chu_thich, nha_cung_cap, so_luong_trong_kho)
                             VALUES (@ten_nguyen_lieu, @gia_nguyen_lieu, @don_vi, @chu_thich, @nha_cung_cap, @so_luong_trong_kho)";
                    await db.ExecuteAsync(query, nhaKho);
                }

                TempData["SuccessMessage"] = "Thêm nguyên liệu thành công!";
                return RedirectToPage("/Views/NhaKho");
            }
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm nguyên liệu.";
            return RedirectToPage("/Views/NhaKho");
        }

        //Tạo mã nhân viên
        private string GenerateUniqueIngredientCode()
        {
            using (var db = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ma_nguyen_lieu FROM nha_kho ORDER BY ma_nguyen_lieu DESC LIMIT 1";
                var lastEmployeeCode = db.QueryFirstOrDefault<string>(query);

                if (lastEmployeeCode == null)
                {
                    return "NV001";
                }

                int newCodeNumber = int.Parse(lastEmployeeCode.Substring(2)) + 1;
                return "NV" + newCodeNumber.ToString("D3");
            }
        }


        //Xóa nguyên liệu
        [HttpDelete]
        public async Task<IActionResult> OnDeleteAsync(int id)
        {
            using (var db = new MySqlConnection(_connectionString))
            {
                string query = "DELETE FROM nha_kho WHERE id = @Id";
                var affectedRows = await db.ExecuteAsync(query, new { Id = id });

                if (affectedRows == 0)
                {
                    return NotFound("Nguyên liệu không tồn tại.");
                }

                return new JsonResult(new { message = "Xóa thành công." }) { StatusCode = 200 };
            }
        }

        //tìm nguyên liệu để chỉnh sửa
        [HttpGet]
        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            _logger.LogInformation($"Đang tìm nguyên liệu với ID: {id}");

            using (var db = new MySqlConnection(_connectionString))
            {
                string query = "SELECT * FROM nha_kho WHERE id = @Id";
                var nhaKho = await db.QueryFirstOrDefaultAsync<NhaKho>(query, new { Id = id });

                if (nhaKho == null)
                {
                    return NotFound();
                }
                return Partial("_EditNguyenLieu", nhaKho);
            }
        }

        // Lưu thông tin nguyên liệu
        [HttpPost]
        public async Task<IActionResult> OnPostEditAsync(int id, NhaKho nhaKho)
        {
            if (id != nhaKho.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using (var db = new MySqlConnection(_connectionString))
                {
                    string query = "UPDATE nha_kho SET ten_nguyen_lieu = @ten_nguyen_lieu, gia_nguyen_lieu = @gia_nguyen_lieu, don_vi = @don_vi, chu_thich = @chu_thich, nha_cung_cap = @nha_cung_cap, so_luong_trong_kho = @so_luong_trong_kho WHERE id = @id";
                    var affectedRows = await db.ExecuteAsync(query, nhaKho);

                    if (affectedRows == 0)
                    {
                        return NotFound();
                    }

                    TempData["SuccessMessage"] = "Cập nhật nguyên liệu thành công!";
                    return RedirectToPage("/Views/NhaKho");
                }
            }
            return Page();
        }

    }
}
