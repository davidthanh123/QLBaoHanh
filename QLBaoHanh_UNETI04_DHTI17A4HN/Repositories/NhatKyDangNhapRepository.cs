using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public interface INhatKyDangNhapRepository : IRepository<NhatKyDangNhap>
    {
        Task<List<NhatKyDangNhap>> LayDanhSachAsync(string? tuKhoa, KetQuaDangNhapEnum? ketQua, int soDongToiDa);
        Task<List<NhatKyDangNhap>> LayTopTheoTaiKhoanAsync(int maTaiKhoan, int soLuong = 10);
    }

    public class NhatKyDangNhapRepository : Repository<NhatKyDangNhap>, INhatKyDangNhapRepository
    {
        public NhatKyDangNhapRepository(AppDbContext context) : base(context)
        {
        }

        public Task<List<NhatKyDangNhap>> LayDanhSachAsync(string? tuKhoa, KetQuaDangNhapEnum? ketQua, int soDongToiDa)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var k = tuKhoa.Trim();
                query = query.Where(n => n.TenDangNhapNhap.Contains(k));
            }
            if (ketQua.HasValue)
                query = query.Where(n => n.KetQua == ketQua.Value);

            return query.OrderByDescending(n => n.ThoiGian).Take(soDongToiDa).ToListAsync();
        }

        public Task<List<NhatKyDangNhap>> LayTopTheoTaiKhoanAsync(int maTaiKhoan, int soLuong = 10) =>
            _dbSet.AsNoTracking()
                .Where(n => n.MaTaiKhoan == maTaiKhoan)
                .OrderByDescending(n => n.ThoiGian)
                .Take(soLuong)
                .ToListAsync();
    }
}
