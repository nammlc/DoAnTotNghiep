// File: Data/MyDbContext.cs
using Microsoft.EntityFrameworkCore;
using DoAnTotNghiep.Models;
using DoAnTotNghiep.Data;

namespace DoAnTotNghiep.Data
{
    public class MyDbContext : DbContext
    {
        public DbSet<MonAn> MonAn {get; set;}
        public DbSet<NhanVien> NhanVien { get; set; }


        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
        }
    }
}

