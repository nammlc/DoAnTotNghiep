using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using DoAnTotNghiep.Data;
using DoAnTotNghiep.Models;
using System.Collections.Generic;
using System.Linq;

namespace DoAnTotNghiep.Pages
{
    public class LuongChiTietModel : PageModel
    {
        private readonly MyDbContext _context;
        private readonly ILogger<LuongChiTietModel> _logger;

        public LuongChiTietModel(MyDbContext context, ILogger<LuongChiTietModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<EmployeeSalary> EmployeeSalaries { get; set; } = new List<EmployeeSalary>();
        public int SelectedMonth { get; set; }

        public void OnGet(int month)
        {
            SelectedMonth = month;
            var hoaDonThang = _context.HoaDon
                .Where(h => h.gio_vao.HasValue 
                            && h.gio_vao.Value.Month == month 
                            && h.trang_thai == "Đã hoàn thành"
                            && h.ten_kh == "Client")
                .ToList();

            var nhanVienList = _context.NhanVien.ToList();
            EmployeeSalaries = nhanVienList
                .Select(nv =>
                {
                   
                    var hoaDonNhanVien = hoaDonThang
                        .Where(hd => hd.ten_nhan_vien == nv.ma_nhan_vien)
                        .GroupBy(hd => hd.gio_vao.Value.Date) 
                        .Count();

                    return new EmployeeSalary
                    {
                        EmployeeCode = nv.ma_nhan_vien,
                        EmployeeName = nv.ten_nhan_vien,
                        Position = nv.vi_tri,
                        TotalSalary = hoaDonNhanVien * 300_000 
                    };
                })
                .Where(es => es.TotalSalary > 0) 
                .OrderBy(es => es.EmployeeCode) 
                .ToList();
        }

        public class EmployeeSalary
        {
            public string EmployeeName { get; set; } 
            public string EmployeeCode { get; set; } 
            public string Position { get; set; }   
            public decimal TotalSalary { get; set; } 
        }
    }
}
