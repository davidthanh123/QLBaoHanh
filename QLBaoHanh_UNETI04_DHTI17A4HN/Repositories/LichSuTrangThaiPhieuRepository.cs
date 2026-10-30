using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public interface ILichSuTrangThaiPhieuRepository : IRepository<LichSuTrangThaiPhieu>
    {
    }

    public class LichSuTrangThaiPhieuRepository : Repository<LichSuTrangThaiPhieu>, ILichSuTrangThaiPhieuRepository
    {
        public LichSuTrangThaiPhieuRepository(AppDbContext context) : base(context)
        {
        }
    }
}
