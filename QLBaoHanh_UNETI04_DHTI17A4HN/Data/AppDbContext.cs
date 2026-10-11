// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: DbContext của hệ thống - khai báo 12 bảng, ràng buộc, index theo Doc/THIETKE.md.

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Data
{
    // Cấu hình ở đây phải khớp Doc/schema.dbml và Doc/RepairPro.sql.
    // Tên bảng lấy từ [Table("...")] trên từng Model (số ít, trùng ERD).
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // 7 bảng bắt buộc (đề mục 10)
        public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
        public DbSet<LoaiThietBi> LoaiThietBis => Set<LoaiThietBi>();
        public DbSet<ThietBi> ThietBis => Set<ThietBi>();
        public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
        public DbSet<PhieuSuaChua> PhieuSuaChuas => Set<PhieuSuaChua>();
        public DbSet<ChiTietSuaChua> ChiTietSuaChuas => Set<ChiTietSuaChua>();
        public DbSet<LinhKien> LinhKiens => Set<LinhKien>();

        // 2 bảng bổ sung nghiệp vụ
        public DbSet<KyThuatVien> KyThuatViens => Set<KyThuatVien>();
        public DbSet<LichSuTrangThaiPhieu> LichSuTrangThaiPhieus => Set<LichSuTrangThaiPhieu>();

        // 3 bảng nâng cao
        public DbSet<NhatKyDangNhap> NhatKyDangNhaps => Set<NhatKyDangNhap>();
        public DbSet<TepDinhKem> TepDinhKems => Set<TepDinhKem>();
        public DbSet<ThongBao> ThongBaos => Set<ThongBao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== 1. Enum lưu dạng chuỗi nvarchar(20) (THIETKE.md mục 2.3) =====
            modelBuilder.Entity<TaiKhoan>().Property(e => e.VaiTro).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<NhatKyDangNhap>().Property(e => e.KetQua).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<KhachHang>().Property(e => e.LoaiKhachHang).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<ThietBi>().Property(e => e.TrangThai).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<PhieuSuaChua>().Property(e => e.TrangThai).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<PhieuSuaChua>().Property(e => e.MucDo).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<LichSuTrangThaiPhieu>().Property(e => e.TrangThaiCu).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<LichSuTrangThaiPhieu>().Property(e => e.TrangThaiMoi).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<TepDinhKem>().Property(e => e.LoaiDoiTuong).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<ThongBao>().Property(e => e.Kenh).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<ThongBao>().Property(e => e.TrangThai).HasConversion<string>().HasMaxLength(20);

            // ===== 2. Các cột không được trùng (đề yêu cầu kiểm tra trùng) =====
            modelBuilder.Entity<TaiKhoan>().HasIndex(t => t.TenDangNhap).IsUnique();
            modelBuilder.Entity<LoaiThietBi>().HasIndex(l => l.TenLoai).IsUnique();
            modelBuilder.Entity<ThietBi>().HasIndex(t => t.SerialNumber).IsUnique();
            modelBuilder.Entity<LinhKien>().HasIndex(l => l.TenLinhKien).IsUnique();
            // CCCD không bắt buộc, nên chỉ unique khi có giá trị
            modelBuilder.Entity<KhachHang>().HasIndex(k => k.CCCD).IsUnique().HasFilter("[CCCD] IS NOT NULL");

            // ===== 3. Quan hệ 1-1 với TaiKhoan =====
            // KhachHang giữ khóa ngoại nullable; EF tự thêm unique index có điều kiện cho MaTaiKhoan
            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.TaiKhoan)
                .WithOne(t => t.KhachHang)
                .HasForeignKey<KhachHang>(k => k.MaTaiKhoan);

            // KyThuatVien giữ khóa ngoại bắt buộc: mỗi tài khoản NhanVien có tối đa 1 hồ sơ kỹ thuật viên
            modelBuilder.Entity<KyThuatVien>()
                .HasOne(k => k.TaiKhoan)
                .WithOne(t => t.KyThuatVien)
                .HasForeignKey<KyThuatVien>(k => k.MaTaiKhoan);

            // ===== 4. Giá trị mặc định ngày giờ (khớp DEFAULT GETDATE() trong RepairPro.sql) =====
            // Không đặt HasDefaultValue cho bool/int/enum: EF coi giá trị mặc định của C# (false/0) là "chưa gán"
            // và sẽ ghi đè bằng default của DB (vd: TrangThai = false bị lưu thành true). Các giá trị đó gán bằng code.
            modelBuilder.Entity<TaiKhoan>().Property(e => e.NgayTaoTaiKhoan).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<NhatKyDangNhap>().Property(e => e.ThoiGian).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<KhachHang>().Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<PhieuSuaChua>().Property(e => e.NgayTiepNhan).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<LichSuTrangThaiPhieu>().Property(e => e.ThoiGian).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<TepDinhKem>().Property(e => e.NgayUpload).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<ThongBao>().Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");

            // ===== 5. Index hỗ trợ tìm kiếm / lọc / thống kê (khớp schema.dbml) =====
            modelBuilder.Entity<ThietBi>().HasIndex(t => t.TrangThai);
            modelBuilder.Entity<ThietBi>().HasIndex(t => new { t.MaLoai, t.MaKhachHang, t.TrangThai });
            modelBuilder.Entity<PhieuSuaChua>().HasIndex(p => new { p.TrangThai, p.NgayTiepNhan });
            modelBuilder.Entity<PhieuSuaChua>().HasIndex(p => p.HanDuKien);
            modelBuilder.Entity<LichSuTrangThaiPhieu>().HasIndex(l => new { l.MaPhieu, l.ThoiGian });
            modelBuilder.Entity<TepDinhKem>().HasIndex(t => new { t.LoaiDoiTuong, t.MaDoiTuong });
            modelBuilder.Entity<ThongBao>().HasIndex(t => new { t.MaTaiKhoanNhan, t.DaDoc });
            modelBuilder.Entity<NhatKyDangNhap>().HasIndex(n => new { n.MaTaiKhoan, n.ThoiGian });

            // ===== 6. CHECK constraint (khớp RepairPro.sql) =====
            modelBuilder.Entity<TaiKhoan>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_TaiKhoan_VaiTro", EnumIn<VaiTroEnum>("VaiTro"));
                t.HasCheckConstraint("CK_TaiKhoan_Email", "[Email] LIKE '%_@_%._%'");
                t.HasCheckConstraint("CK_TaiKhoan_SaiMK", "[SoLanDangNhapSai] >= 0");
            });
            modelBuilder.Entity<NhatKyDangNhap>().ToTable(t =>
                t.HasCheckConstraint("CK_NhatKy_KetQua", EnumIn<KetQuaDangNhapEnum>("KetQua")));
            modelBuilder.Entity<LoaiThietBi>().ToTable(t =>
                t.HasCheckConstraint("CK_LoaiThietBi_BH", "[ThoiHanBaoHanhMacDinh] BETWEEN 0 AND 120"));
            modelBuilder.Entity<KhachHang>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_KhachHang_Loai", EnumIn<LoaiKhachHangEnum>("LoaiKhachHang"));
                t.HasCheckConstraint("CK_KhachHang_SDT", "[SoDienThoai] NOT LIKE '%[^0-9+]%'");
                t.HasCheckConstraint("CK_KhachHang_MST", "[MaSoThue] IS NULL OR [LoaiKhachHang] = N'DoanhNghiep'");
            });
            modelBuilder.Entity<ThietBi>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_ThietBi_TrangThai", EnumIn<TrangThaiThietBiEnum>("TrangThai"));
                t.HasCheckConstraint("CK_ThietBi_HanBH", "[HanBaoHanh] >= [NgayMua]");
                t.HasCheckConstraint("CK_ThietBi_GiaTri", "[GiaTri] >= 0");
            });
            modelBuilder.Entity<LinhKien>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_LinhKien_DonGia", "[DonGia] >= 0");
                t.HasCheckConstraint("CK_LinhKien_Ton", "[SoLuongTon] >= 0");
            });
            modelBuilder.Entity<PhieuSuaChua>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_Phieu_TrangThai", EnumIn<TrangThaiPhieuEnum>("TrangThai"));
                t.HasCheckConstraint("CK_Phieu_MucDo", EnumIn<MucDoEnum>("MucDo"));
                t.HasCheckConstraint("CK_Phieu_LyDo", "[TrangThai] NOT IN (N'Huy', N'TuChoi') OR [LyDoHuyTuChoi] IS NOT NULL");
                t.HasCheckConstraint("CK_Phieu_HoanThanh", "[TrangThai] <> N'HoanThanh' OR [NgayHoanThanh] IS NOT NULL");
                t.HasCheckConstraint("CK_Phieu_HanDuKien", "[HanDuKien] IS NULL OR [HanDuKien] >= CAST([NgayTiepNhan] AS date)");
            });
            modelBuilder.Entity<LichSuTrangThaiPhieu>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_LichSu_Cu", EnumIn<TrangThaiPhieuEnum>("TrangThaiCu", choPhepNull: true));
                t.HasCheckConstraint("CK_LichSu_Moi", EnumIn<TrangThaiPhieuEnum>("TrangThaiMoi"));
            });
            modelBuilder.Entity<ChiTietSuaChua>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_CT_SoLuong", "[SoLuong] > 0");
                t.HasCheckConstraint("CK_CT_DonGia", "[DonGia] >= 0");
                t.HasCheckConstraint("CK_CT_ThanhTien", "[ThanhTien] = [SoLuong] * [DonGia]");
            });
            modelBuilder.Entity<TepDinhKem>().ToTable(t =>
                t.HasCheckConstraint("CK_Tep_Loai", EnumIn<LoaiDoiTuongDinhKemEnum>("LoaiDoiTuong")));
            modelBuilder.Entity<ThongBao>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_TB_Kenh", EnumIn<KenhThongBaoEnum>("Kenh"));
                t.HasCheckConstraint("CK_TB_TrangThai", EnumIn<TrangThaiGuiThongBaoEnum>("TrangThai"));
            });

            // ===== 7. Tắt xóa dây chuyền cho mọi khóa ngoại (THIETKE.md mục 2.7) =====
            // 1) SQL Server báo lỗi "multiple cascade paths" vì KhachHang -> PhieuSuaChua đi 2 đường
            //    (trực tiếp và qua ThietBi).
            // 2) Đề yêu cầu không xóa đối tượng đã phát sinh lịch sử.
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        // Sinh điều kiện CHECK "[Cot] IN (N'A', N'B', ...)" từ chính tên các member của enum,
        // để danh sách giá trị trong DB luôn trùng với enum trong code.
        private static string EnumIn<TEnum>(string cot, bool choPhepNull = false) where TEnum : struct, Enum
        {
            var giaTri = string.Join(", ", Enum.GetNames<TEnum>().Select(ten => $"N'{ten}'"));
            var dieuKien = $"[{cot}] IN ({giaTri})";
            return choPhepNull ? $"[{cot}] IS NULL OR {dieuKien}" : dieuKien;
        }
    }
}
