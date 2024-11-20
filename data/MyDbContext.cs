// File: Data/MyDbContext.cs
using Microsoft.EntityFrameworkCore;
using DoAnTotNghiep.Models;
using DoAnTotNghiep.Data;

namespace DoAnTotNghiep.Data
{
    public class MyDbContext : DbContext
    {
        public DbSet<NhaKho> NhaKho { get; set; }
        public DbSet<MonAn> MonAn { get; set; }
        public DbSet<NhanVien> NhanVien { get; set; }
        public DbSet<HoaDon> HoaDon { get; set; }
        public DbSet<BanAn> BanAn{get; set;}


        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           modelBuilder.Entity<BanAn>()
                .HasOne(b => b.HoaDon)
                .WithOne(h => h.BanAn)
                .HasForeignKey<HoaDon>(h => h.ban_an_id)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

