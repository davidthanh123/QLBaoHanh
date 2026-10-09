using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
        public DbSet<LoaiThietBi> LoaiThietBis => Set<LoaiThietBi>();
        public DbSet<ThietBi> ThietBis => Set<ThietBi>();
        public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
        public DbSet<PhieuSuaChua> PhieuSuaChuas => Set<PhieuSuaChua>();
        public DbSet<ChiTietSuaChua> ChiTietSuaChuas => Set<ChiTietSuaChua>();
        public DbSet<LinhKien> LinhKiens => Set<LinhKien>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Các cột không được trùng (đề yêu cầu kiểm tra trùng)
            modelBuilder.Entity<TaiKhoan>().HasIndex(t => t.TenDangNhap).IsUnique();
            modelBuilder.Entity<LoaiThietBi>().HasIndex(l => l.TenLoai).IsUnique();
            modelBuilder.Entity<ThietBi>().HasIndex(t => t.SerialNumber).IsUnique();

            // KhachHang - TaiKhoan: quan hệ 1-1, KhachHang giữ khóa ngoại (nullable)
            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.TaiKhoan)
                .WithOne(t => t.KhachHang)
                .HasForeignKey<KhachHang>(k => k.MaTaiKhoan);

            // Tắt xóa dây chuyền cho mọi khóa ngoại:
            // 1) SQL Server báo lỗi "multiple cascade paths" vì KhachHang -> PhieuSuaChua đi 2 đường
            //    (trực tiếp và qua ThietBi).
            // 2) Đề yêu cầu không xóa đối tượng đã phát sinh lịch sử.
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
