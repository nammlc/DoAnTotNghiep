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
    public class ThongKeModel : PageModel
    {
        private readonly MyDbContext _context;
        private readonly ILogger<ThongKeModel> _logger;

        // Chỉ cần một constructor duy nhất
        public ThongKeModel(MyDbContext context, ILogger<ThongKeModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<HoaDon> hoaDons { get; set; }
        public List<NhanVien> nhanViens { get; set; }

        public async Task<IActionResult> OnGet()
        {
            hoaDons = await _context.HoaDon.ToListAsync();
            nhanViens = await _context.NhanVien.ToListAsync();

            // Trả dữ liệu về trang dưới dạng JSON
            return Page();
        }


    }



}

