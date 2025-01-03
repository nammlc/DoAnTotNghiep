using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using DoAnTotNghiep.Data;
using DoAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoAnTotNghiep.Pages
{
    public class BangLuongNhanVienModel : PageModel
    {
        private readonly MyDbContext _context;
        private readonly ILogger<BangLuongNhanVienModel> _logger;

        // Chỉ cần một constructor duy nhất
        public BangLuongNhanVienModel(MyDbContext context, ILogger<BangLuongNhanVienModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<HoaDon> hoaDons { get; set; }
        public List<NhanVien> nhanViens { get; set; }
        public int SelectedYear { get; set; }

        public async Task<IActionResult> OnGet(int? selectedYear)
        {
            SelectedYear = selectedYear ?? DateTime.Now.Year;
            hoaDons = await _context.HoaDon
            .Where(h => h.gio_vao.HasValue && h.gio_vao.Value.Year == SelectedYear && h.trang_thai == "Đã hoàn thành")
            .ToListAsync();
            nhanViens = await _context.NhanVien.ToListAsync();
            return Page();
        }
    }
}

