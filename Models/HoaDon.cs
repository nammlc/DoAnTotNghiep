using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnTotNghiep.Models
{
    [Table("hoa_don")]
    public class HoaDon
    {
        public int id { get; set; }
        public long tong_tien { get; set; }
        public DateTime? gio_vao { get; set; }
        public DateTime? gio_ra { get; set; }
        public string list_mon_an { get; set; }
        public string? ten_ban { get; set; }
        public string ten_nhan_vien { get; set; }
        public string trang_thai { get; set; }
        public string ten_kh {get;set;}

        public int? ban_an_id { get; set; }
        public int? khach_hang_id { get; set; }

        [ForeignKey("ban_an_id")]
        public BanAn BanAn { get; set; }

        [ForeignKey("khach_hang_id")]
        public KhachHangs KhachHang{get;set;}

    }
}
