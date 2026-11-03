using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public interface ILoaiThietBiRepository : IRepository<LoaiThietBi>
    {
        Task<List<LoaiThietBiDongViewModel>> LayDanhSachDongAsync(string? tuKhoa, bool? trangThai);
        Task<int> DemSoThietBiTheoLoaiAsync(int maLoai);
        Task<bool> KiemTraTrungTenAsync(string tenLoai, int excludeId = 0);
        Task<List<LoaiThietBi>> LayDanhSachHoatDongHoacHienTaiAsync(int? maLoaiHienTai = null);
        Task<List<string>> LayDanhSachHangSanXuatAsync();
        Task<bool> KiemTraLoaiThietBiHoatDongAsync(int maLoai);
    }

    public class LoaiThietBiRepository : Repository<LoaiThietBi>, ILoaiThietBiRepository
    {
        public LoaiThietBiRepository(AppDbContext context) : base(context)
        {
        }

        public Task<List<LoaiThietBiDongViewModel>> LayDanhSachDongAsync(string? tuKhoa, bool? trangThai)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var k = tuKhoa.Trim();
                query = query.Where(l => l.TenLoai.Contains(k) || l.HangSanXuat.Contains(k));
            }
            if (trangThai.HasValue)
                query = query.Where(l => l.TrangThai == trangThai.Value);

            return query
                .OrderBy(l => l.TenLoai)
                .Select(l => new LoaiThietBiDongViewModel { Loai = l, SoThietBi = l.ThietBis.Count })
                .ToListAsync();
        }

        public Task<int> DemSoThietBiTheoLoaiAsync(int maLoai) =>
            _context.ThietBis.CountAsync(t => t.MaLoai == maLoai);

        public Task<bool> KiemTraTrungTenAsync(string tenLoai, int excludeId = 0) =>
            _dbSet.AnyAsync(l => l.TenLoai == tenLoai && l.MaLoai != excludeId);

        public Task<List<LoaiThietBi>> LayDanhSachHoatDongHoacHienTaiAsync(int? maLoaiHienTai = null) =>
            _dbSet.AsNoTracking()
                .Where(l => l.TrangThai || l.MaLoai == maLoaiHienTai)
                .OrderBy(l => l.TenLoai)
                .ToListAsync();

        public Task<List<string>> LayDanhSachHangSanXuatAsync() =>
            _dbSet.AsNoTracking()
                .Select(l => l.HangSanXuat)
                .Distinct()
                .OrderBy(h => h)
                .ToListAsync();

        public Task<bool> KiemTraLoaiThietBiHoatDongAsync(int maLoai) =>
            _dbSet.AnyAsync(l => l.MaLoai == maLoai && l.TrangThai);
    }
}
