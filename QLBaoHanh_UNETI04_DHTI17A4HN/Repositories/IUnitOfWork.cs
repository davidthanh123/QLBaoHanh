using Microsoft.EntityFrameworkCore.Storage;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ITaiKhoanRepository TaiKhoans { get; }
        ILoaiThietBiRepository LoaiThietBis { get; }
        IThietBiRepository ThietBis { get; }
        IKhachHangRepository KhachHangs { get; }
        IPhieuSuaChuaRepository PhieuSuaChuas { get; }
        IChiTietSuaChuaRepository ChiTietSuaChuas { get; }
        ILinhKienRepository LinhKiens { get; }
        IKyThuatVienRepository KyThuatViens { get; }
        ILichSuTrangThaiPhieuRepository LichSuTrangThaiPhieus { get; }
        INhatKyDangNhapRepository NhatKyDangNhaps { get; }
        ITepDinhKemRepository TepDinhKems { get; }
        IThongBaoRepository ThongBaos { get; }

        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
