// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 1 - nghiệp vụ Loại thiết bị (tách từ LoaiThietBiController).

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class LoaiThietBiService : ILoaiThietBiService
    {
        private const string LoiTrungTen = "Tên loại thiết bị đã tồn tại";
        private readonly AppDbContext _db;
        public LoaiThietBiService(AppDbContext db) => _db = db;

        public async Task<List<LoaiThietBiDongViewModel>> LayDanhSachAsync(string? tuKhoa, bool? trangThai)
        {
            var query = _db.LoaiThietBis.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var k = tuKhoa.Trim();
                query = query.Where(l => l.TenLoai.Contains(k) || l.HangSanXuat.Contains(k));
            }
            if (trangThai.HasValue)
                query = query.Where(l => l.TrangThai == trangThai.Value);

            return await query
                .OrderBy(l => l.TenLoai)
                .Select(l => new LoaiThietBiDongViewModel { Loai = l, SoThietBi = l.ThietBis.Count })
                .ToListAsync();
        }

        public async Task<ChiTietLoaiThietBi?> LayChiTietAsync(int id)
        {
            var loai = await _db.LoaiThietBis.AsNoTracking().FirstOrDefaultAsync(l => l.MaLoai == id);
            if (loai == null) return null;

            var soThietBi = await _db.ThietBis.CountAsync(t => t.MaLoai == id);
            return new ChiTietLoaiThietBi(loai, soThietBi);
        }

        public Task<LoaiThietBi?> LayDeSuaAsync(int id) =>
            _db.LoaiThietBis.AsNoTracking().FirstOrDefaultAsync(l => l.MaLoai == id);

        public async Task<KetQuaDichVu> ThemAsync(LoaiThietBi model, bool modelHopLe)
        {
            ChuanHoa(model);
            var loi = await KiemTraTrungTenAsync(model, 0);
            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            _db.LoaiThietBis.Add(model);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.DuLieuKhongHopLe(new[] { new LoiTruongNhap(nameof(model.TenLoai), LoiTrungTen) });
            }

            return KetQuaDichVu.Ok($"Đã thêm loại thiết bị {model.TenLoai}.");
        }

        public async Task<KetQuaDichVu> SuaAsync(int id, LoaiThietBi model, bool modelHopLe)
        {
            var loai = await _db.LoaiThietBis.FirstOrDefaultAsync(l => l.MaLoai == id);
            if (loai == null) return KetQuaDichVu.KhongTimThay();

            ChuanHoa(model);
            var loi = await KiemTraTrungTenAsync(model, id);
            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            // Chỉ cập nhật các trường cho phép sửa trên bản ghi gốc
            loai.TenLoai = model.TenLoai;
            loai.HangSanXuat = model.HangSanXuat;
            loai.MoTa = model.MoTa;
            loai.ThoiHanBaoHanhMacDinh = model.ThoiHanBaoHanhMacDinh;
            loai.TrangThai = model.TrangThai;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.DuLieuKhongHopLe(new[] { new LoiTruongNhap(nameof(model.TenLoai), LoiTrungTen) });
            }

            return KetQuaDichVu.Ok($"Đã cập nhật loại thiết bị {loai.TenLoai}.");
        }

        // Ngừng sử dụng / sử dụng lại (đảo TrangThai).
        public async Task<KetQuaDichVu> DoiTrangThaiAsync(int id)
        {
            var loai = await _db.LoaiThietBis.FirstOrDefaultAsync(l => l.MaLoai == id);
            if (loai == null) return KetQuaDichVu.KhongTimThay();

            loai.TrangThai = !loai.TrangThai;
            await _db.SaveChangesAsync();

            return KetQuaDichVu.Ok(loai.TrangThai
                ? $"Đã cho phép sử dụng lại loại thiết bị {loai.TenLoai}."
                : $"Đã ngừng sử dụng loại thiết bị {loai.TenLoai}.");
        }

        // Xóa an toàn: đã có thiết bị tham chiếu thì không xóa, gợi ý "Ngừng sử dụng".
        public async Task<KetQuaDichVu> XoaAsync(int id)
        {
            var loai = await _db.LoaiThietBis.FirstOrDefaultAsync(l => l.MaLoai == id);
            if (loai == null) return KetQuaDichVu.KhongTimThay();

            var soThietBi = await _db.ThietBis.CountAsync(t => t.MaLoai == id);
            if (soThietBi > 0)
                return KetQuaDichVu.Loi($"Không thể xóa: loại thiết bị {loai.TenLoai} đang có {soThietBi} thiết bị sử dụng. Hãy chọn Ngừng sử dụng thay vì xóa.");

            _db.LoaiThietBis.Remove(loai);
            try
            {
                await _db.SaveChangesAsync();
                return KetQuaDichVu.Ok($"Đã xóa loại thiết bị {loai.TenLoai}.");
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.Loi("Không thể xóa loại thiết bị này vì đã có dữ liệu liên quan.");
            }
        }

        // ---------------------------------------------------------------------

        private static void ChuanHoa(LoaiThietBi model)
        {
            model.TenLoai = (model.TenLoai ?? string.Empty).Trim();
            model.HangSanXuat = (model.HangSanXuat ?? string.Empty).Trim();
            model.MoTa = string.IsNullOrWhiteSpace(model.MoTa) ? null : model.MoTa.Trim();
        }

        // Báo lỗi thân thiện trước khi chạm unique index. id = 0 khi thêm mới.
        private async Task<List<LoiTruongNhap>> KiemTraTrungTenAsync(LoaiThietBi model, int id)
        {
            var loi = new List<LoiTruongNhap>();
            if (string.IsNullOrEmpty(model.TenLoai)) return loi;
            if (await _db.LoaiThietBis.AnyAsync(l => l.TenLoai == model.TenLoai && l.MaLoai != id))
                loi.Add(new(nameof(model.TenLoai), LoiTrungTen));
            return loi;
        }
    }
}
