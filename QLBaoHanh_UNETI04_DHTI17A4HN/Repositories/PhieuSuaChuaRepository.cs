using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public record KetQuaPhanTrangPhieu(List<PhieuSuaChua> DanhSach, int TongSoPhieu, int SoTrang, int TrangHienTai, DateTime? TuNgay, DateTime? DenNgay);

    public interface IPhieuSuaChuaRepository : IRepository<PhieuSuaChua>
    {
        Task<KetQuaPhanTrangPhieu> LayDanhSachPhanTrangAsync(PhieuSuaChuaBoLoc boLoc, int? maKhachHang, int kichThuocTrang);
        Task<PhieuSuaChua?> LayChiTietKemTatCaThongTinAsync(int maPhieu);
        Task<PhieuSuaChua?> LayPhieuKemThietBiAsync(int maPhieu);
        Task<PhieuSuaChua?> LayPhieuTheoNguoiDungAsync(int maPhieu, int? maKhachHang);
        Task<bool> KiemTraThietBiCoPhieuDangMoAsync(int maThietBi, int? excludeMaPhieu = null);
        Task<bool> KiemTraThietBiCoPhieuMoHoacDangSuaAsync(int maThietBi);
    }

    public class PhieuSuaChuaRepository : Repository<PhieuSuaChua>, IPhieuSuaChuaRepository
    {
        public PhieuSuaChuaRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<KetQuaPhanTrangPhieu> LayDanhSachPhanTrangAsync(PhieuSuaChuaBoLoc boLoc, int? maKhachHang, int kichThuocTrang)
        {
            var trang = boLoc.Trang < 1 ? 1 : boLoc.Trang;
            var tuNgay = boLoc.TuNgay;
            var denNgay = boLoc.DenNgay;
            if (tuNgay.HasValue && denNgay.HasValue && tuNgay.Value.Date > denNgay.Value.Date)
                (tuNgay, denNgay) = (denNgay, tuNgay);

            var query = _dbSet.AsNoTracking()
                .Include(p => p.KhachHang)
                .Include(p => p.ThietBi).ThenInclude(t => t!.LoaiThietBi)
                .AsQueryable();

            if (maKhachHang.HasValue)
            {
                query = query.Where(p => p.MaKhachHang == maKhachHang.Value);
            }

            if (!string.IsNullOrWhiteSpace(boLoc.TuKhoa))
            {
                var tuKhoaSach = boLoc.TuKhoa.Trim();
                query = query.Where(p =>
                    p.KhachHang!.HoTen.Contains(tuKhoaSach) ||
                    p.KhachHang.SoDienThoai.Contains(tuKhoaSach) ||
                    p.ThietBi!.TenThietBi.Contains(tuKhoaSach) ||
                    p.ThietBi.SerialNumber.Contains(tuKhoaSach));
            }

            if (boLoc.MaLoai.HasValue)
                query = query.Where(p => p.ThietBi != null && p.ThietBi.MaLoai == boLoc.MaLoai.Value);
            if (boLoc.TrangThai.HasValue)
                query = query.Where(p => p.TrangThai == boLoc.TrangThai.Value);
            if (tuNgay.HasValue)
                query = query.Where(p => p.NgayTiepNhan >= tuNgay.Value.Date);
            if (denNgay.HasValue)
            {
                var mocKetThuc = denNgay.Value.Date.AddDays(1);
                query = query.Where(p => p.NgayTiepNhan < mocKetThuc);
            }

            query = boLoc.SapXep switch
            {
                "cu-nhat" => query.OrderBy(p => p.NgayTiepNhan),
                "ten-khach" => query.OrderBy(p => p.KhachHang!.HoTen).ThenByDescending(p => p.NgayTiepNhan),
                "ten-thiet-bi" => query.OrderBy(p => p.ThietBi!.TenThietBi).ThenByDescending(p => p.NgayTiepNhan),
                _ => query.OrderByDescending(p => p.NgayTiepNhan)
            };

            var tongSoPhieu = await query.CountAsync();
            var soTrang = Math.Max(1, (int)Math.Ceiling(tongSoPhieu / (double)kichThuocTrang));
            if (trang > soTrang) trang = soTrang;

            var danhSach = await query.Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang).ToListAsync();

            return new KetQuaPhanTrangPhieu(danhSach, tongSoPhieu, soTrang, trang, tuNgay, denNgay);
        }

        public Task<PhieuSuaChua?> LayChiTietKemTatCaThongTinAsync(int maPhieu) =>
            _dbSet.AsNoTracking()
                .Include(p => p.KhachHang)
                .Include(p => p.ThietBi).ThenInclude(t => t!.LoaiThietBi)
                .Include(p => p.TaiKhoanTiepNhan)
                .Include(p => p.ChiTietSuaChuas).ThenInclude(c => c.KyThuatVien)
                    .ThenInclude(k => k!.TaiKhoan)
                .Include(p => p.ChiTietSuaChuas).ThenInclude(c => c.LinhKien)
                .Include(p => p.LichSus).ThenInclude(l => l.TaiKhoanThucHien)
                .FirstOrDefaultAsync(p => p.MaPhieu == maPhieu);

        public Task<PhieuSuaChua?> LayPhieuKemThietBiAsync(int maPhieu) =>
            _dbSet.Include(p => p.ThietBi).FirstOrDefaultAsync(p => p.MaPhieu == maPhieu);

        public Task<PhieuSuaChua?> LayPhieuTheoNguoiDungAsync(int maPhieu, int? maKhachHang)
        {
            var query = _dbSet.Where(p => p.MaPhieu == maPhieu);
            if (maKhachHang.HasValue)
                query = query.Where(p => p.MaKhachHang == maKhachHang.Value);
            return query.FirstOrDefaultAsync();
        }

        public Task<bool> KiemTraThietBiCoPhieuDangMoAsync(int maThietBi, int? excludeMaPhieu = null) =>
            excludeMaPhieu.HasValue
                ? _dbSet.AnyAsync(p => p.MaThietBi == maThietBi && p.MaPhieu != excludeMaPhieu.Value
                    && (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy))
                : _dbSet.AnyAsync(p => p.MaThietBi == maThietBi
                    && (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy));

        public Task<bool> KiemTraThietBiCoPhieuMoHoacDangSuaAsync(int maThietBi) =>
            _dbSet.AnyAsync(p => p.MaThietBi == maThietBi &&
                (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy));
    }
}
