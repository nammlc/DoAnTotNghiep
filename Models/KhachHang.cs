using System.ComponentModel.DataAnnotations.Schema;
namespace DoAnTotNghiep.Models
{
    [Table("khach_hang")]
    public class KhachHangs
    {
        public int? id { get; set; }

        public string? ho_ten { get; set; }

        public string? dia_chi { get; set; }

        public string? so_dien_thoai { get; set; }

        public string? ten_dang_nhap { get; set; }

        public string? mat_khau { get; set; }
        public List<HoaDon>? HoaDons { get; set; }
        public string? gioi_tinh { get; set; }

    }
}

