using System.ComponentModel.DataAnnotations.Schema;
namespace DoAnTotNghiep.Models
{
    [Table("mon_an")]
    public class MonAn {
        public int id{get; set;}

        public string ma_mon_an{get; set;}

        public string ten_mon_an{get; set;}

        public string gia_mon_an{get; set;}

        public string mo_ta{get; set;}

        public string loai_mon_an{get; set;}

        public string phan_loai_thuc_an{get; set;}

        public string anh_minh_hoa{get; set;}
    }
}

