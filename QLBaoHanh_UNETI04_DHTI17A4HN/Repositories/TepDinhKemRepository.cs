using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public interface ITepDinhKemRepository : IRepository<TepDinhKem>
    {
    }

    public class TepDinhKemRepository : Repository<TepDinhKem>, ITepDinhKemRepository
    {
        public TepDinhKemRepository(AppDbContext context) : base(context)
        {
        }
    }
}
