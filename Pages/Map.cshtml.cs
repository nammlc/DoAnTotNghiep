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


namespace DoAnTotNghiep.Pages
{
    public class MapModel : PageModel
    {
        public string SearchQuery { get; set; }
        public int TotalPages { get; set; }
        private readonly ILogger<IndexModel> _logger;
        private readonly MyDbContext _context;
        private readonly string _connectionString;

        public MapModel(ILogger<IndexModel> logger, MyDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IList<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

        public async Task<IActionResult> OnGet(int? page, string searchQuery)
        {
            SearchQuery = searchQuery;

            var allBill = _context.HoaDon
                .Select(hd => new HoaDon
                {
                    id = hd.id,
                    tong_tien = hd.tong_tien,
                    gio_vao = hd.gio_vao ?? DateTime.Now,
                    gio_ra = hd.gio_ra ?? DateTime.Now,
                    list_mon_an = hd.list_mon_an ?? "",
                    ten_ban = hd.ten_ban ?? "",
                    ten_nhan_vien = hd.ten_nhan_vien ?? "",
                    trang_thai = hd.trang_thai ?? "",
                    ten_kh =hd.ten_kh
                })
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                allBill = allBill.Where(e => e.ten_ban.Contains(searchQuery));
            }

            allBill = allBill.OrderByDescending(m => m.gio_vao);
            int pageSize = 12;
            int pageNumber = page ?? 1;

            TotalPages = (int)Math.Ceiling(allBill.Count() / (double)pageSize);
            ViewData["TotalPages"] = TotalPages;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TongHoaDon"] = allBill.Count();
            
            
            HoaDons = allBill.ToList();
            long tongTienTrongCa = 0;
            foreach(var hoadon in HoaDons){
                tongTienTrongCa = tongTienTrongCa + hoadon.tong_tien;
            }
            
            return Page();
        }
    }
}
