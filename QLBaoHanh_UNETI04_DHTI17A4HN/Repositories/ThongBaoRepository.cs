using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public interface IThongBaoRepository : IRepository<ThongBao>
    {
    }

    public class ThongBaoRepository : Repository<ThongBao>, IThongBaoRepository
    {
        public ThongBaoRepository(AppDbContext context) : base(context)
        {
        }
    }
}
