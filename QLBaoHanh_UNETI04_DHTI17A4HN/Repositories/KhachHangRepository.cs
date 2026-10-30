using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public interface IKhachHangRepository : IRepository<KhachHang>
    {
        Task<List<KhachHang>> TimKiemAsync(string? tuKhoa, bool? trangThai);
        Task<KhachHang?> LayTheoMaTaiKhoanAsync(int maTaiKhoan, bool tracking = false);
        Task<int?> LayMaKhachHangTheoTaiKhoanAsync(int maTaiKhoan);
        Task<bool> KiemTraTrungCccdAsync(string cccd, int excludeMaKhachHang);
        Task<List<KhachHang>> LayDanhSachHoatDongHoacHienTaiAsync(int? maKhachHangHienTai = null);
        Task<bool> KiemTraKhachHangHoatDongAsync(int maKhachHang);
    }

    public class KhachHangRepository : Repository<KhachHang>, IKhachHangRepository
    {
        public KhachHangRepository(AppDbContext context) : base(context)
        {
        }

        public Task<List<KhachHang>> TimKiemAsync(string? tuKhoa, bool? trangThai)
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

            return query.OrderBy(k => k.HoTen).ToListAsync();
        }

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

        public Task<List<KhachHang>> LayDanhSachHoatDongHoacHienTaiAsync(int? maKhachHangHienTai = null) =>
            _dbSet.AsNoTracking()
                .Where(k => k.TrangThai || k.MaKhachHang == maKhachHangHienTai)
                .OrderBy(k => k.HoTen)
                .ToListAsync();

        public Task<bool> KiemTraKhachHangHoatDongAsync(int maKhachHang) =>
            _dbSet.AnyAsync(k => k.MaKhachHang == maKhachHang && k.TrangThai);
    }
}
