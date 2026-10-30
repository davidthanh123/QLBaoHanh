using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public interface IKyThuatVienRepository : IRepository<KyThuatVien>
    {
        Task<List<KyThuatVien>> TimKiemAsync(string? tuKhoa, bool? trangThai);
        Task<bool> KiemTraTaiKhoanDaCoHoSoAsync(int maTaiKhoan, int? excludeMaKtv = null);
        Task<List<KyThuatVien>> LayDanhSachHoatDongAsync();
        Task<KyThuatVien?> LayChiTietKemTaiKhoanAsync(int maKyThuatVien);
    }

    public class KyThuatVienRepository : Repository<KyThuatVien>, IKyThuatVienRepository
    {
        public KyThuatVienRepository(AppDbContext context) : base(context)
        {
        }

        public Task<List<KyThuatVien>> TimKiemAsync(string? tuKhoa, bool? trangThai)
        {
            var query = _dbSet.AsNoTracking().Include(k => k.TaiKhoan).AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tuKhoaSach = tuKhoa.Trim();
                query = query.Where(k => k.HoTen.Contains(tuKhoaSach) ||
                    (k.ChuyenMon != null && k.ChuyenMon.Contains(tuKhoaSach)) ||
                    (k.TaiKhoan != null && k.TaiKhoan.TenDangNhap.Contains(tuKhoaSach)));
            }

            if (trangThai.HasValue)
                query = query.Where(k => k.TrangThai == trangThai.Value);

            return query.OrderBy(k => k.HoTen).ToListAsync();
        }

        public Task<bool> KiemTraTaiKhoanDaCoHoSoAsync(int maTaiKhoan, int? excludeMaKtv = null) =>
            excludeMaKtv.HasValue
                ? _dbSet.AnyAsync(k => k.MaTaiKhoan == maTaiKhoan && k.MaKyThuatVien != excludeMaKtv.Value)
                : _dbSet.AnyAsync(k => k.MaTaiKhoan == maTaiKhoan);

        public Task<List<KyThuatVien>> LayDanhSachHoatDongAsync() =>
            _dbSet.AsNoTracking()
                .Include(k => k.TaiKhoan)
                .Where(k => k.TrangThai && k.TaiKhoan != null && k.TaiKhoan.TrangThai)
                .OrderBy(k => k.HoTen)
                .ToListAsync();

        public Task<KyThuatVien?> LayChiTietKemTaiKhoanAsync(int maKyThuatVien) =>
            _dbSet.Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKyThuatVien == maKyThuatVien && k.TrangThai && k.TaiKhoan != null && k.TaiKhoan.TrangThai);
    }
}
