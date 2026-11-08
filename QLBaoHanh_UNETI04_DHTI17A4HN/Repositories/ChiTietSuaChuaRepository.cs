using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public interface IChiTietSuaChuaRepository : IRepository<ChiTietSuaChua>
    {
        Task<List<ChiTietSuaChua>> LayDanhSachKemThongTinAsync();
        Task<ChiTietSuaChua?> LayChiTietKemThongTinAsync(int id);
        Task<decimal> TinhTongGiaTriSuaChuaAsync();
        Task<bool> KiemTraPhieuCoChiTietAsync(int maPhieu);
    }

    public class ChiTietSuaChuaRepository : Repository<ChiTietSuaChua>, IChiTietSuaChuaRepository
    {
        public ChiTietSuaChuaRepository(AppDbContext context) : base(context)
        {
        }

        public Task<List<ChiTietSuaChua>> LayDanhSachKemThongTinAsync() =>
            _dbSet.AsNoTracking()
                .Include(c => c.KyThuatVien)
                .Include(c => c.LinhKien)
                .Include(c => c.PhieuSuaChua)
                .ToListAsync();

        public Task<ChiTietSuaChua?> LayChiTietKemThongTinAsync(int id) =>
            _dbSet.AsNoTracking()
                .Include(c => c.PhieuSuaChua)
                .Include(c => c.KyThuatVien)
                .Include(c => c.LinhKien)
                .FirstOrDefaultAsync(c => c.MaChiTiet == id);

        public async Task<decimal> TinhTongGiaTriSuaChuaAsync() =>
            await _dbSet.SumAsync(c => (decimal?)c.ThanhTien) ?? 0m;

        public Task<bool> KiemTraPhieuCoChiTietAsync(int maPhieu) =>
            _dbSet.AnyAsync(c => c.MaPhieu == maPhieu);
    }
}
