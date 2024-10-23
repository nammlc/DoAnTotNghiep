namespace DoAnTotNghiep.Models
{
   public class NhanVien
{
    public int id { get; set; }
    public string ten_nhan_vien { get; set; }
    public string ma_nhan_vien { get; set; }
    public DateTime ngay_sinh { get; set; }
    public string gioi_tinh { get; set; }
    public string vi_tri { get; set; }
    public string so_cmnd { get; set; }
    public DateTime ngay_cap_cmnd { get; set; }
    public string noi_cap_cmnd { get; set; }
    public string dia_chi { get; set; }
    public string so_dien_thoai { get; set; }
    public string? so_dien_thoai_co_dinh { get; set; } 
    public string email { get; set; }
    public string so_tai_khoan_ngan_hang { get; set; }
    public string ten_ngan_hang { get; set; }
    public string chi_nhanh_ngan_hang { get; set; }
}
}