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
using Microsoft.AspNetCore.Antiforgery;

namespace DoAnTotNghiep.Pages
{
    public class MapModel : PageModel
    {
        private readonly ILogger<MapModel> _logger;
        private readonly MyDbContext _context;
        private readonly string _connectionString;

        public MapModel(ILogger<MapModel> logger, MyDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IList<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
        public IList<BanAn> BanAns { get; set; } = new List<BanAn>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Lấy thông tin tất cả các bàn và đơn hàng
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Lấy danh sách bàn
                var banAns = await connection.QueryAsync<BanAn>("SELECT * FROM ban_an");
                BanAns = new List<BanAn>(banAns);

                // Lấy danh sách đơn hàng
                var hoaDons = await connection.QueryAsync<HoaDon>("SELECT * FROM hoa_don");
                hoaDons = hoaDons.OrderByDescending(m => m.gio_vao);
                HoaDons = new List<HoaDon>(hoaDons);
            }

            return Page();
        }

        // Hàm xử lý yêu cầu gán đơn hàng vào bàn
        [HttpPost]
        public async Task<IActionResult> OnPostTable([FromBody] AssignOrderRequest request)
        {
            if (request == null || request.TableId <= 0 || request.OrderId <= 0)
            {
                return BadRequest("Thông tin không hợp lệ.");
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    // Bắt đầu transaction
                    await connection.OpenAsync();
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        // Kiểm tra ID bàn có tồn tại không
                        var tableExists = await connection.QueryFirstOrDefaultAsync<int>(
                            "SELECT COUNT(1) FROM ban_an WHERE id = @TableId",
                            new { request.TableId }, transaction);

                        if (tableExists == 0)
                        {
                            return BadRequest("Bàn không tồn tại.");
                        }

                        // Kiểm tra ID hóa đơn có tồn tại không
                        var orderExists = await connection.QueryFirstOrDefaultAsync<int>(
                            "SELECT COUNT(1) FROM hoa_don WHERE id = @OrderId",
                            new { request.OrderId }, transaction);

                        if (orderExists == 0)
                        {
                            return BadRequest("Hóa đơn không tồn tại.");
                        }

                        // Cập nhật trạng thái bàn
                        var updateTableQuery = @"
                            UPDATE ban_an 
                            SET trang_thai = 'Có đơn hàng', hoa_don_id = @OrderId 
                            WHERE id = @TableId;";
                        await connection.ExecuteAsync(updateTableQuery, new { request.TableId, request.OrderId }, transaction);

                        // Cập nhật ban_an_id trong hóa đơn
                        var updateOrderQuery = @"
                            UPDATE hoa_don 
                            SET ban_an_id = @TableId 
                            WHERE id = @OrderId;";
                        await connection.ExecuteAsync(updateOrderQuery, new { request.TableId, request.OrderId }, transaction);

                        // Commit transaction
                        await transaction.CommitAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi gán đơn hàng vào bàn");
                    return BadRequest(new { success = false, message = "Không thể gán đơn hàng vào bàn." });
                }
            }

            return new OkResult();
        }

        //kết thúc đơn hàng
        public HoaDon hoaDon { get; set; }
        public async Task<IActionResult> OnPostFinishOrder([FromBody] AssignOrderRequest request)
        {
            if (request == null || request.TableId <= 0 || request.OrderId <= 0)
            {
                return BadRequest("Thông tin không hợp lệ.");
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    // Bắt đầu transaction
                    await connection.OpenAsync();
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        // Cập nhật trạng thái bàn
                        var updateTableQuery = @"
                    UPDATE ban_an 
                    SET trang_thai = NULL, hoa_don_id = NULL 
                    WHERE id = @TableId;";
                        await connection.ExecuteAsync(updateTableQuery, new { request.TableId }, transaction);

                        // Lấy đối tượng HoaDon
                        var hoaDon = await connection.QueryFirstOrDefaultAsync<HoaDon>(
                            "SELECT * FROM hoa_don WHERE id = @OrderId",
                            new { request.OrderId },
                            transaction);

                        if (hoaDon == null)
                        {
                            return NotFound("Hóa đơn không tồn tại.");
                        }

                        hoaDon.gio_ra = DateTime.Now;

                        // Cập nhật ban_an_id trong hóa đơn
                        var updateOrderQuery = @"
                    UPDATE hoa_don 
                    SET ban_an_id = NULL , trang_thai = 'Đã hoàn thành', gio_ra = @gio_ra
                    WHERE id = @OrderId;";
                        await connection.ExecuteAsync(updateOrderQuery, new { hoaDon.gio_ra, request.OrderId }, transaction);

                        // Commit transaction
                        await transaction.CommitAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi kết thúc đơn hàng");
                    return BadRequest(new { success = false, message = "Không thể kết thúc đơn hàng." });
                }
            }

            return new OkResult();
        }


    }
    public class AssignOrderRequest
    {
        public int TableId { get; set; }
        public int OrderId { get; set; }
    }
}

