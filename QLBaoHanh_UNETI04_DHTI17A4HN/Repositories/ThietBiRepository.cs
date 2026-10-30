using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public record KetQuaPhanTrangThietBi(List<ThietBi> DanhSach, int TongSoLuong, int SoTrang);

    public interface IThietBiRepository : IRepository<ThietBi>
    {
        Task<KetQuaPhanTrangThietBi> LayDanhSachPhanTrangAsync(ThietBiFilter f, int? maKhachHang, int kichThuocTrang);
        Task<ThietBi?> LayChiTietKemThongTinAsync(int id);
        Task<bool> KiemTraTrungSerialAsync(string serialNumber, int? excludeId = null);
        Task<List<ThietBi>> LayDanhSachKhaDungTheoKhachHangAsync(int maKhachHang);
        Task<bool> KiemTraCoPhieuChuaDongAsync(int maThietBi);
        Task<bool> KiemTraCoPhieuSuaChuaAsync(int maThietBi);
    }

    public class ThietBiRepository : Repository<ThietBi>, IThietBiRepository
    {
        public ThietBiRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<KetQuaPhanTrangThietBi> LayDanhSachPhanTrangAsync(ThietBiFilter f, int? maKhachHang, int kichThuocTrang)
        {
            var q = _dbSet.AsNoTracking()
                .Include(t => t.LoaiThietBi)
                .Include(t => t.KhachHang)
                .AsQueryable();

            if (maKhachHang.HasValue)
                q = q.Where(t => t.MaKhachHang == maKhachHang.Value);

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(f.Keyword))
            {
                var kw = f.Keyword.Trim();
                q = q.Where(t => t.TenThietBi.Contains(kw)
                              || t.SerialNumber.Contains(kw)
                              || t.KhachHang!.HoTen.Contains(kw));
            }

            // Lọc
            if (f.MaLoai != null) q = q.Where(t => t.MaLoai == f.MaLoai);
            if (!string.IsNullOrEmpty(f.Hang)) q = q.Where(t => t.LoaiThietBi!.HangSanXuat == f.Hang);
            if (f.TrangThai != null) q = q.Where(t => t.TrangThai == f.TrangThai);
            if (f.GiaTu != null) q = q.Where(t => t.GiaTri >= f.GiaTu);
            if (f.GiaDen != null) q = q.Where(t => t.GiaTri <= f.GiaDen);

            var homNay = DateTime.Today;
            var sapHet = homNay.AddDays(30);
            q = f.BaoHanh switch
            {
                "con" => q.Where(t => t.HanBaoHanh >= homNay),
                "sap" => q.Where(t => t.HanBaoHanh >= homNay && t.HanBaoHanh <= sapHet),
                "het" => q.Where(t => t.HanBaoHanh < homNay),
                _ => q
            };

            // Sắp xếp
            q = f.Sort switch
            {
                "ten" => q.OrderBy(t => t.TenThietBi),
                "ten_desc" => q.OrderByDescending(t => t.TenThietBi),
                "ngaymua" => q.OrderBy(t => t.NgayMua),
                "ngaymua_desc" => q.OrderByDescending(t => t.NgayMua),
                "hanbh" => q.OrderBy(t => t.HanBaoHanh),
                "hanbh_desc" => q.OrderByDescending(t => t.HanBaoHanh),
                "giatri" => q.OrderBy(t => t.GiaTri),
                "giatri_desc" => q.OrderByDescending(t => t.GiaTri),
                _ => q.OrderByDescending(t => t.MaThietBi)
            };

            var tong = await q.CountAsync();
            var soTrang = Math.Max(1, (int)Math.Ceiling(tong / (double)kichThuocTrang));
            f.Page = Math.Clamp(f.Page, 1, soTrang);

            var danhSach = await q.Skip((f.Page - 1) * kichThuocTrang).Take(kichThuocTrang).ToListAsync();
            return new KetQuaPhanTrangThietBi(danhSach, tong, soTrang);
        }

        public Task<ThietBi?> LayChiTietKemThongTinAsync(int id) =>
            _dbSet.AsNoTracking()
                .Include(t => t.LoaiThietBi)
                .Include(t => t.KhachHang)
                .Include(t => t.PhieuSuaChuas)
                .FirstOrDefaultAsync(t => t.MaThietBi == id);

        public Task<bool> KiemTraTrungSerialAsync(string serialNumber, int? excludeId = null) =>
            excludeId.HasValue
                ? _dbSet.AnyAsync(t => t.SerialNumber == serialNumber && t.MaThietBi != excludeId.Value)
                : _dbSet.AnyAsync(t => t.SerialNumber == serialNumber);

        public Task<List<ThietBi>> LayDanhSachKhaDungTheoKhachHangAsync(int maKhachHang) =>
            _dbSet.AsNoTracking()
                .Where(t => t.MaKhachHang == maKhachHang && t.TrangThai == TrangThaiThietBiEnum.KhaDung)
                .OrderBy(t => t.TenThietBi)
                .ToListAsync();

        public Task<bool> KiemTraCoPhieuChuaDongAsync(int maThietBi) =>
            _context.PhieuSuaChuas.AnyAsync(p => p.MaThietBi == maThietBi
                && (p.TrangThai == TrangThaiPhieuEnum.ChoXuLy || p.TrangThai == TrangThaiPhieuEnum.DangXuLy));

        public Task<bool> KiemTraCoPhieuSuaChuaAsync(int maThietBi) =>
            _context.PhieuSuaChuas.AnyAsync(p => p.MaThietBi == maThietBi);
    }
}
