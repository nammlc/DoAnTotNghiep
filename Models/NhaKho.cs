using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnTotNghiep.Models
{
    [Table("nha_kho")]
    public class NhaKho
    {
        public int id { get; set; }
        public string ten_nguyen_lieu { get; set; }
        public string gia_nguyen_lieu { get; set; }
        public string don_vi { get; set; }
        public string chu_thich { get; set; }
        public string nha_cung_cap { get; set; }
        public string so_luong_trong_kho { get; set; }


    }
}

/* create table nha_kho (
//	id INT AUTO_INCREMENT PRIMARY KEY,
 //   ten_nguyen_lieu varchar(255),
    gia_nguyen_lieu varchar(255),
    don_vi varchar(255),
    chu_thich text,
    nha_cung_cap varchar(255),
    so_luong_trong_kho varchar(255)
);*/