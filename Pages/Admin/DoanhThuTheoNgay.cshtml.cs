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
    public class DoanhThuTheoNgayModel : PageModel
    {
        private readonly MyDbContext _context;
        private readonly ILogger<DoanhThuTheoNgayModel> _logger;

        public DoanhThuTheoNgayModel(MyDbContext context, ILogger<DoanhThuTheoNgayModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Danh sách năm có dữ liệu
        public List<int> Years { get; set; } = new();
        public int SelectedYear { get; set; }
        public List<int> RevenueData { get; set; }
        public string RevenueJson { get; set; } = "{}";
        public int SelectedMonth { get; set; }
        public void OnGet(int month)
        {
            SelectedMonth = month;
            int daysInMonth = DateTime.DaysInMonth(2024, month);
            RevenueData = new List<int>(new int[daysInMonth]);
            var dailyRevenueQuery = _context.HoaDon
                .Where(h => h.gio_vao.HasValue && h.gio_vao.Value.Month == month)
                .GroupBy(h => h.gio_vao.Value.Day) 
                .Select(g => new { Day = g.Key, TotalRevenue = g.Sum(h => h.tong_tien) })
                .ToList();
            foreach (var item in dailyRevenueQuery)
            {
                RevenueData[item.Day - 1] = (int)item.TotalRevenue;
            }
        }

    }
}
