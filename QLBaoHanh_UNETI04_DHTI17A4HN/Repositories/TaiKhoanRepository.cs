using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public record ThongTinXacThucTaiKhoan(bool TrangThai, VaiTroEnum VaiTro, string HoTen);

    public interface ITaiKhoanRepository : IRepository<TaiKhoan>
    {
        Task<TaiKhoan?> LayTheoTenDangNhapAsync(string tenDangNhap);
        Task<List<TaiKhoan>> TimKiemAsync(string? tuKhoa, VaiTroEnum? vaiTro, bool? trangThai);
        Task<TaiKhoan?> LayChiTietKemHoSoAsync(int id);
        Task<bool> KiemTraTrungTenDangNhapAsync(string tenDangNhap, int? excludeId = null);
        Task<bool> LaAdminHoatDongCuoiCungAsync(int id);
        Task<bool> CoDuLieuLienQuanAsync(int id);
        Task<List<TaiKhoan>> LayDanhSachNhanVienChuaCoHoSoKtvAsync(int? taiKhoanHienTai = null);
        Task<bool> KiemTraNhanVienHoatDongHopLeAsync(int maTaiKhoan);
        Task<ThongTinXacThucTaiKhoan?> LayThongTinXacThucAsync(int maTaiKhoan);
    }

    public class TaiKhoanRepository : Repository<TaiKhoan>, ITaiKhoanRepository
    {
        public TaiKhoanRepository(AppDbContext context) : base(context)
        {
        }

        public Task<TaiKhoan?> LayTheoTenDangNhapAsync(string tenDangNhap) =>
            _dbSet.FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);

        public Task<List<TaiKhoan>> TimKiemAsync(string? tuKhoa, VaiTroEnum? vaiTro, bool? trangThai)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var k = tuKhoa.Trim();
                query = query.Where(t => t.TenDangNhap.Contains(k) || t.HoTen.Contains(k) || t.Email.Contains(k));
            }
            if (vaiTro.HasValue)
                query = query.Where(t => t.VaiTro == vaiTro.Value);
            if (trangThai.HasValue)
                query = query.Where(t => t.TrangThai == trangThai.Value);

            return query.OrderBy(t => t.TenDangNhap).ToListAsync();
        }

        public Task<TaiKhoan?> LayChiTietKemHoSoAsync(int id) =>
            _dbSet.AsNoTracking()
                .Include(t => t.KhachHang)
                .Include(t => t.KyThuatVien)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

        public Task<bool> KiemTraTrungTenDangNhapAsync(string tenDangNhap, int? excludeId = null) =>
            excludeId.HasValue
                ? _dbSet.AnyAsync(t => t.TenDangNhap == tenDangNhap && t.MaTaiKhoan != excludeId.Value)
                : _dbSet.AnyAsync(t => t.TenDangNhap == tenDangNhap);

        public Task<bool> LaAdminHoatDongCuoiCungAsync(int id) =>
            _dbSet.AnyAsync(t => t.MaTaiKhoan != id && t.VaiTro == VaiTroEnum.Admin && t.TrangThai)
                .ContinueWith(task => !task.Result);

        public async Task<bool> CoDuLieuLienQuanAsync(int id)
        {
            return await _context.KhachHangs.AnyAsync(k => k.MaTaiKhoan == id)
                || await _context.KyThuatViens.AnyAsync(k => k.MaTaiKhoan == id)
                || await _context.PhieuSuaChuas.AnyAsync(p => p.MaTaiKhoanTiepNhan == id)
                || await _context.LichSuTrangThaiPhieus.AnyAsync(l => l.MaTaiKhoanThucHien == id)
                || await _context.TepDinhKems.AnyAsync(t => t.MaTaiKhoanUpload == id)
                || await _context.ThongBaos.AnyAsync(t => t.MaTaiKhoanNhan == id)
                || await _context.NhatKyDangNhaps.AnyAsync(n => n.MaTaiKhoan == id);
        }

        public Task<List<TaiKhoan>> LayDanhSachNhanVienChuaCoHoSoKtvAsync(int? taiKhoanHienTai = null) =>
            _dbSet.AsNoTracking()
                .Where(t => t.VaiTro == VaiTroEnum.NhanVien && t.TrangThai &&
                    (t.KyThuatVien == null || t.MaTaiKhoan == taiKhoanHienTai))
                .OrderBy(t => t.HoTen).ToListAsync();

        public Task<bool> KiemTraNhanVienHoatDongHopLeAsync(int maTaiKhoan) =>
            _dbSet.AnyAsync(t => t.MaTaiKhoan == maTaiKhoan && t.VaiTro == VaiTroEnum.NhanVien && t.TrangThai);

        public Task<ThongTinXacThucTaiKhoan?> LayThongTinXacThucAsync(int maTaiKhoan) =>
            _dbSet.AsNoTracking()
                .Where(t => t.MaTaiKhoan == maTaiKhoan)
                .Select(t => new ThongTinXacThucTaiKhoan(t.TrangThai, t.VaiTro, t.HoTen))
                .FirstOrDefaultAsync();
    }
}
