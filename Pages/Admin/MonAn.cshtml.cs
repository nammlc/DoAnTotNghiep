using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DoAnTotNghiep.Models;
using DoAnTotNghiep.Data;
using System.Data.SqlTypes;

namespace DoAnTotNghiep.Pages
{
    public class MonAnModel : PageModel
    {
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int ToTalDish { get; set; }
        private readonly string _connectionString;
        private readonly MyDbContext _context;
        private readonly ILogger<MonAnModel> _logger;

        public MonAnModel(MyDbContext context, IConfiguration configuration, ILogger<MonAnModel> logger)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public IList<MonAn> MonAns { get; set; }
        public string SearchQuery { get; set; }
        public int Page { get; set; }


        // Hiển thị danh sách món ăn với phân trang
        [HttpGet]
        public IActionResult OnGet(int? page, string searchQuery)
        {
            SearchQuery = searchQuery;

            var allDishes = _context.MonAn.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                allDishes = allDishes.Where(e => e.ten_mon_an.Contains(searchQuery) || e.ma_mon_an.Contains(SearchQuery));
            }
            allDishes = allDishes.OrderBy(m => m.ten_mon_an);
            int pageSize = 10;
            int pageNumber = page ?? 1;

            TotalPages = (int)Math.Ceiling(allDishes.Count() / (double)pageSize);
            ViewData["TotalPages"] = TotalPages;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["ToTalDish"] = allDishes.Count();

            MonAns = allDishes.ToList();

            return Page();
        }

        // Hiển thị form thêm món ăn
        public IActionResult OnGetCreate()
        {
            return Page();
        }

        [BindProperty]
        public MonAn monAn { get; set; }

        //Thêm mới món ăn
        [HttpPost]
        public async Task<IActionResult> OnPostMonAnAsync()
        {
            if (ModelState.IsValid)
            {
                

                using (var db = new MySqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO mon_an (ten_mon_an, gia_mon_an, mo_ta, loai_mon_an, phan_loai_thuc_an, anh_minh_hoa)
                             VALUES (@ten_mon_an, @gia_mon_an, @mo_ta, @loai_mon_an, @phan_loai_thuc_an, @anh_minh_hoa)";
                    await db.ExecuteAsync(query, monAn);
                }

                TempData["SuccessMessage"] = "Thêm món ăn thành công!";
                return RedirectToPage("/Admin/MonAn");
            }
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm món ăn.";
            return RedirectToPage("/Admin/MonAn");
        }

        //Tạo mã món ăn
        private string GenerateUniqueIngredientCode()
        {
            using (var db = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ma_nguyen_lieu FROM mon_an ORDER BY ma_mon_an DESC LIMIT 1";
                var lastDishCode = db.QueryFirstOrDefault<string>(query);

                if (lastDishCode == null)
                {
                    return "NV001";
                }

                int newCodeNumber = int.Parse(lastDishCode.Substring(2)) + 1;
                return "NV" + newCodeNumber.ToString("D3");
            }
        }


        //Xóa món ăn
        [HttpDelete]
        public async Task<IActionResult> OnDeleteAsync(int id)
        {
            using (var db = new MySqlConnection(_connectionString))
            {
                string query = "DELETE FROM mon_an WHERE id = @Id";
                var affectedRows = await db.ExecuteAsync(query, new { Id = id });

                if (affectedRows == 0)
                {
                    return NotFound("món ăn không tồn tại.");
                }

                return new JsonResult(new { message = "Xóa thành công." }) { StatusCode = 200 };
            }
        }

        //tìm món ăn để chỉnh sửa
        [HttpGet]
        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            _logger.LogInformation($"Đang tìm món ăn với ID: {id}");

            using (var db = new MySqlConnection(_connectionString))
            {
                string query = "SELECT * FROM mon_an WHERE id = @Id";
                var monAn = await db.QueryFirstOrDefaultAsync<MonAn>(query, new { Id = id });

                if (monAn == null)
                {
                    return NotFound();
                }
                return Partial("_EditMonAn", monAn);
            }
        }

        // Lưu thông tin món ăn
        [HttpPost]
        public async Task<IActionResult> OnPostEditMonAnAsync(int id, MonAn monAn)
        {
            if (id != monAn.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)

            {
                 _logger.LogInformation($"đang lưu món ăn với ID: {id}");
                using (var db = new MySqlConnection(_connectionString))
                {
                    string query = "UPDATE mon_an SET ma_mon_an = @ma_mon_an , ten_mon_an = @ten_mon_an, gia_mon_an = @gia_mon_an, mo_ta = @mo_ta, loai_mon_an = @loai_mon_an, phan_loai_thuc_an = @phan_loai_thuc_an, anh_minh_hoa = @anh_minh_hoa WHERE id = @id";
                    var affectedRows = await db.ExecuteAsync(query, monAn);

                    if (affectedRows == 0)
                    {
                        return NotFound();
                    }

                    TempData["SuccessMessage"] = "Cập nhật món ăn thành công!";
                    return RedirectToPage("/Admin/MonAn");
                }
            }
            return Page();
        }

    }
}
