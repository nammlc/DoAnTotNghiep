using System.ComponentModel.DataAnnotations.Schema;
namespace DoAnTotNghiep.Models
{
    [Table("tai_khoan_thanh_toan")]
    public class TaiKhoanThanhToan
    {
        public int id { get; set; }
        public string ten_tai_khoan { get; set; }
        public string so_tai_khoan { get; set; }
        public string ten_ngan_hang { get; set; }
        public string? chi_nhanh_ngan_hang { get; set; }

    }
}

