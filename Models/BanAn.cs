using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnTotNghiep.Models
{
    [Table("ban_an")]

    public class BanAn
    {
        public int id { get; set; }
        public string ten_ban { get; set; }
        public string trang_thai { get; set; }
        public HoaDon HoaDon { get; set; }
        public int? hoa_don_id { get; set; }

    }
}