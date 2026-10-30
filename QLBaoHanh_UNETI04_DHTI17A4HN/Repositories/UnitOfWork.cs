using Microsoft.EntityFrameworkCore.Storage;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        private ITaiKhoanRepository? _taiKhoans;
        private ILoaiThietBiRepository? _loaiThietBis;
        private IThietBiRepository? _thietBis;
        private IKhachHangRepository? _khachHangs;
        private IPhieuSuaChuaRepository? _phieuSuaChuas;
        private IChiTietSuaChuaRepository? _chiTietSuaChuas;
        private ILinhKienRepository? _linhKiens;
        private IKyThuatVienRepository? _kyThuatViens;
        private ILichSuTrangThaiPhieuRepository? _lichSuTrangThaiPhieus;
        private INhatKyDangNhapRepository? _nhatKyDangNhaps;
        private ITepDinhKemRepository? _tepDinhKems;
        private IThongBaoRepository? _thongBaos;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public ITaiKhoanRepository TaiKhoans => _taiKhoans ??= new TaiKhoanRepository(_context);
        public ILoaiThietBiRepository LoaiThietBis => _loaiThietBis ??= new LoaiThietBiRepository(_context);
        public IThietBiRepository ThietBis => _thietBis ??= new ThietBiRepository(_context);
        public IKhachHangRepository KhachHangs => _khachHangs ??= new KhachHangRepository(_context);
        public IPhieuSuaChuaRepository PhieuSuaChuas => _phieuSuaChuas ??= new PhieuSuaChuaRepository(_context);
        public IChiTietSuaChuaRepository ChiTietSuaChuas => _chiTietSuaChuas ??= new ChiTietSuaChuaRepository(_context);
        public ILinhKienRepository LinhKiens => _linhKiens ??= new LinhKienRepository(_context);
        public IKyThuatVienRepository KyThuatViens => _kyThuatViens ??= new KyThuatVienRepository(_context);
        public ILichSuTrangThaiPhieuRepository LichSuTrangThaiPhieus => _lichSuTrangThaiPhieus ??= new LichSuTrangThaiPhieuRepository(_context);
        public INhatKyDangNhapRepository NhatKyDangNhaps => _nhatKyDangNhaps ??= new NhatKyDangNhapRepository(_context);
        public ITepDinhKemRepository TepDinhKems => _tepDinhKems ??= new TepDinhKemRepository(_context);
        public IThongBaoRepository ThongBaos => _thongBaos ??= new ThongBaoRepository(_context);

        public IRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.TryGetValue(type, out var repo))
            {
                repo = new Repository<T>(_context);
                _repositories[type] = repo;
            }
            return (IRepository<T>)repo;
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

        public Task<IDbContextTransaction> BeginTransactionAsync() => _context.Database.BeginTransactionAsync();

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
