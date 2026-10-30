using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public interface ILinhKienRepository : IRepository<LinhKien>
    {
        Task<List<LinhKien>> TimKiemAsync(string? tuKhoa, string? trangThai, string? sapXep);
        Task<bool> KiemTraTrungTenAsync(string tenLinhKien, int? excludeId = null);
        Task<List<LinhKien>> LayDanhSachTheoLocAsync(LocLinhKien loc, int? maLinhKienDangChon = null);
        Task<List<GiaLinhKien>> LayDanhSachGiaLinhKienAsync();
        Task<List<LinhKien>> LayDanhSachKhaDungAsync();
        Task<LinhKien?> LayTheoMaKemTrangThaiAsync(int maLinhKien, bool chiKhaDung = false);
    }

    public class LinhKienRepository : Repository<LinhKien>, ILinhKienRepository
    {
        public LinhKienRepository(AppDbContext context) : base(context)
        {
        }

        public Task<List<LinhKien>> TimKiemAsync(string? tuKhoa, string? trangThai, string? sapXep)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
                query = query.Where(l => l.TenLinhKien.Contains(tuKhoa));

            if (trangThai == "hoatdong")
                query = query.Where(l => l.TrangThai);
            else if (trangThai == "ngung")
                query = query.Where(l => !l.TrangThai);

            query = sapXep switch
            {
                "gia_tang" => query.OrderBy(l => l.DonGia),
                "gia_giam" => query.OrderByDescending(l => l.DonGia),
                "ton_tang" => query.OrderBy(l => l.SoLuongTon),
                "ton_giam" => query.OrderByDescending(l => l.SoLuongTon),
                _ => query.OrderBy(l => l.MaLinhKien)
            };

            return query.ToListAsync();
        }

        public Task<bool> KiemTraTrungTenAsync(string tenLinhKien, int? excludeId = null) =>
            excludeId.HasValue
                ? _dbSet.AnyAsync(l => l.TenLinhKien == tenLinhKien && l.MaLinhKien != excludeId.Value)
                : _dbSet.AnyAsync(l => l.TenLinhKien == tenLinhKien);

        public Task<List<LinhKien>> LayDanhSachTheoLocAsync(LocLinhKien loc, int? maLinhKienDangChon = null)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();
            query = loc switch
            {
                LocLinhKien.DangHoatDong => query.Where(l => l.TrangThai),
                LocLinhKien.DangHoatDongVaConHang => query.Where(l => l.TrangThai && l.SoLuongTon > 0),
                LocLinhKien.DangHoatDongConHangHoacDangChon => query.Where(l =>
                    l.TrangThai && (l.SoLuongTon > 0 || l.MaLinhKien == maLinhKienDangChon)),
                _ => query
            };
            return query.ToListAsync();
        }

        public Task<List<GiaLinhKien>> LayDanhSachGiaLinhKienAsync() =>
            _dbSet.AsNoTracking()
                .Select(l => new GiaLinhKien(l.MaLinhKien, l.DonGia))
                .ToListAsync();

        public Task<List<LinhKien>> LayDanhSachKhaDungAsync() =>
            _dbSet.AsNoTracking()
                .Where(l => l.TrangThai && l.SoLuongTon > 0)
                .OrderBy(l => l.TenLinhKien)
                .ToListAsync();

        public Task<LinhKien?> LayTheoMaKemTrangThaiAsync(int maLinhKien, bool chiKhaDung = false) =>
            chiKhaDung
                ? _dbSet.FirstOrDefaultAsync(l => l.MaLinhKien == maLinhKien && l.TrangThai)
                : _dbSet.FirstOrDefaultAsync(l => l.MaLinhKien == maLinhKien);
    }
}
