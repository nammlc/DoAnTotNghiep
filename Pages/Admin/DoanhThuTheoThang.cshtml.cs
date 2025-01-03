using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using DoAnTotNghiep.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DoAnTotNghiep.Pages
{
    public class DoanhThuTheoThangModel : PageModel
    {
        private readonly MyDbContext _context;
        private readonly ILogger<DoanhThuTheoThangModel> _logger;

        public DoanhThuTheoThangModel(MyDbContext context, ILogger<DoanhThuTheoThangModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Danh sách năm có dữ liệu
        public List<int> Years { get; set; } = new();
        public int SelectedYear { get; set; }
        public Dictionary<int, long> RevenueData { get; set; } = new();
        public string RevenueJson { get; set; } = "{}";
        public long totalBill{get;set;}

        public async Task<ActionResult> OnGetAsync(int? selectedYear)
        {

            Years = await _context.HoaDon
                .Where(h => h.gio_vao.HasValue)
                .Select(h => h.gio_vao.Value.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            SelectedYear = selectedYear ?? DateTime.Now.Year;

            
            var revenue = await _context.HoaDon
                .Where(h => h.gio_vao.HasValue && h.gio_vao.Value.Year == SelectedYear && h.trang_thai == "Đã hoàn thành")
                .GroupBy(h => h.gio_vao.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalRevenue = g.Sum(h => h.tong_tien)
                })
                .ToListAsync();

            RevenueData = revenue.ToDictionary(r => r.Month, r => r.TotalRevenue);
            foreach(var totalbill in revenue){
                totalBill += totalbill.TotalRevenue;
            }
            RevenueJson = System.Text.Json.JsonSerializer.Serialize(RevenueData);
            return Page();
        }
    }
}
