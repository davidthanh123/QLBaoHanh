// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Module 3 - truy vấn khách hàng: tìm kiếm, lọc, phân trang, kiểm tra trùng CCCD/tài khoản, kiểm tra dữ liệu phát sinh trước khi ngừng/xóa.

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public record KetQuaPhanTrangKhachHang(List<KhachHang> DanhSach, int TongSo, int SoTrang, int TrangHienTai);

    public interface IKhachHangRepository : IRepository<KhachHang>
    {
        Task<KetQuaPhanTrangKhachHang> TimKiemPhanTrangAsync(
            string? tuKhoa, bool? trangThai, LoaiKhachHangEnum? loai, int trang, int kichThuocTrang);
        Task<KhachHang?> LayChiTietAsync(int maKhachHang);
        Task<KhachHang?> LayTheoMaKemTaiKhoanAsync(int maKhachHang);
        Task<KhachHang?> LayTheoMaTaiKhoanAsync(int maTaiKhoan, bool tracking = false);
        Task<int?> LayMaKhachHangTheoTaiKhoanAsync(int maTaiKhoan);
        Task<bool> KiemTraTrungCccdAsync(string cccd, int excludeMaKhachHang);
        Task<bool> KiemTraTaiKhoanDaLienKetAsync(int maTaiKhoan, int excludeMaKhachHang);
        Task<List<TaiKhoan>> LayTaiKhoanKhachChuaLienKetAsync(int? giuLaiMaTaiKhoan = null);
        Task<List<KhachHang>> LayDanhSachHoatDongHoacHienTaiAsync(int? maKhachHangHienTai = null);
        Task<bool> KiemTraKhachHangHoatDongAsync(int maKhachHang);
        Task<bool> KiemTraCoPhieuChuaDongAsync(int maKhachHang);
        Task<bool> KiemTraCoDuLieuPhatSinhAsync(int maKhachHang);
    }

    public class KhachHangRepository : Repository<KhachHang>, IKhachHangRepository
    {
        public KhachHangRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<KetQuaPhanTrangKhachHang> TimKiemPhanTrangAsync(
            string? tuKhoa, bool? trangThai, LoaiKhachHangEnum? loai, int trang, int kichThuocTrang)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(k => k.TaiKhoan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tuKhoaSach = tuKhoa.Trim();
                query = query.Where(k =>
                    k.HoTen.Contains(tuKhoaSach) ||
                    k.SoDienThoai.Contains(tuKhoaSach) ||
                    (k.Email != null && k.Email.Contains(tuKhoaSach)) ||
                    (k.CCCD != null && k.CCCD.Contains(tuKhoaSach)));
            }

            if (trangThai.HasValue)
                query = query.Where(k => k.TrangThai == trangThai.Value);
            if (loai.HasValue)
                query = query.Where(k => k.LoaiKhachHang == loai.Value);

            var tong = await query.CountAsync();
            var soTrang = Math.Max(1, (int)Math.Ceiling(tong / (double)kichThuocTrang));
            trang = Math.Clamp(trang, 1, soTrang);

            var danhSach = await query
                .OrderBy(k => k.HoTen)
                .ThenBy(k => k.MaKhachHang)
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

            return new KetQuaPhanTrangKhachHang(danhSach, tong, soTrang, trang);
        }

        // Dùng cho trang chi tiết: kèm tài khoản, thiết bị và các phiếu (mới nhất trước).
        public Task<KhachHang?> LayChiTietAsync(int maKhachHang) =>
            _dbSet.AsNoTracking()
                .Include(k => k.TaiKhoan)
                .Include(k => k.ThietBis)
                .Include(k => k.PhieuSuaChuas.OrderByDescending(p => p.NgayTiepNhan))
                    .ThenInclude(p => p.ThietBi)
                .FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang);

        public Task<KhachHang?> LayTheoMaKemTaiKhoanAsync(int maKhachHang) =>
            _dbSet.Include(k => k.TaiKhoan).FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang);

        public Task<KhachHang?> LayTheoMaTaiKhoanAsync(int maTaiKhoan, bool tracking = false)
        {
            var query = tracking ? _dbSet.Include(k => k.TaiKhoan).AsQueryable() : _dbSet.AsNoTracking();
            return query.FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan);
        }

        public Task<int?> LayMaKhachHangTheoTaiKhoanAsync(int maTaiKhoan) =>
            _dbSet.AsNoTracking()
                .Where(k => k.MaTaiKhoan == maTaiKhoan && k.TrangThai)
                .Select(k => (int?)k.MaKhachHang)
                .FirstOrDefaultAsync();

        public Task<bool> KiemTraTrungCccdAsync(string cccd, int excludeMaKhachHang) =>
            _dbSet.AnyAsync(k => k.CCCD == cccd && k.MaKhachHang != excludeMaKhachHang);

        public Task<bool> KiemTraTaiKhoanDaLienKetAsync(int maTaiKhoan, int excludeMaKhachHang) =>
            _dbSet.AnyAsync(k => k.MaTaiKhoan == maTaiKhoan && k.MaKhachHang != excludeMaKhachHang);

        // Tài khoản vai trò Khách hàng, đang hoạt động và chưa gắn hồ sơ nào (giữ lại tài khoản đang gắn khi sửa).
        public Task<List<TaiKhoan>> LayTaiKhoanKhachChuaLienKetAsync(int? giuLaiMaTaiKhoan = null) =>
            _context.TaiKhoans.AsNoTracking()
                .Where(t => t.VaiTro == VaiTroEnum.KhachHang
                    && ((t.TrangThai && t.KhachHang == null) || t.MaTaiKhoan == giuLaiMaTaiKhoan))
                .OrderBy(t => t.TenDangNhap)
                .ToListAsync();

        public Task<List<KhachHang>> LayDanhSachHoatDongHoacHienTaiAsync(int? maKhachHangHienTai = null) =>
            _dbSet.AsNoTracking()
                .Where(k => k.TrangThai || k.MaKhachHang == maKhachHangHienTai)
                .OrderBy(k => k.HoTen)
                .ToListAsync();

        public Task<bool> KiemTraKhachHangHoatDongAsync(int maKhachHang) =>
            _dbSet.AnyAsync(k => k.MaKhachHang == maKhachHang && k.TrangThai);

        // Phiếu còn mở (Chờ xử lý / Đang xử lý) thì chưa được ngừng hoạt động khách hàng.
        public Task<bool> KiemTraCoPhieuChuaDongAsync(int maKhachHang) =>
            _context.PhieuSuaChuas.AnyAsync(p => p.MaKhachHang == maKhachHang
                && (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy));

        // Đã có thiết bị hoặc phiếu thì không xóa cứng (đề mục 6.2, THIETKE 2.7).
        public async Task<bool> KiemTraCoDuLieuPhatSinhAsync(int maKhachHang) =>
            await _context.ThietBis.AnyAsync(t => t.MaKhachHang == maKhachHang)
            || await _context.PhieuSuaChuas.AnyAsync(p => p.MaKhachHang == maKhachHang);
    }
}
