// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 1 - nghiệp vụ Loại thiết bị (tách từ LoaiThietBiController).

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Repositories;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class LoaiThietBiService : ILoaiThietBiService
    {
        private const string LoiTrungTen = "Tên loại thiết bị đã tồn tại";
        private readonly IUnitOfWork _uow;

        public LoaiThietBiService(IUnitOfWork uow) => _uow = uow;

        public Task<List<LoaiThietBiDongViewModel>> LayDanhSachAsync(string? tuKhoa, bool? trangThai) =>
            _uow.LoaiThietBis.LayDanhSachDongAsync(tuKhoa, trangThai);

        public async Task<ChiTietLoaiThietBi?> LayChiTietAsync(int id)
        {
            var loai = await _uow.LoaiThietBis.GetByIdAsync(id);
            if (loai == null) return null;

            var soThietBi = await _uow.LoaiThietBis.DemSoThietBiTheoLoaiAsync(id);
            return new ChiTietLoaiThietBi(loai, soThietBi);
        }

        public Task<LoaiThietBi?> LayDeSuaAsync(int id) =>
            _uow.LoaiThietBis.GetByIdAsync(id);

        public async Task<KetQuaDichVu> ThemAsync(LoaiThietBi model, bool modelHopLe)
        {
            ChuanHoa(model);
            var loi = await KiemTraTrungTenAsync(model, 0);
            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            await _uow.LoaiThietBis.AddAsync(model);
            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.DuLieuKhongHopLe(new[] { new LoiTruongNhap(nameof(model.TenLoai), LoiTrungTen) });
            }

            return KetQuaDichVu.Ok($"Đã thêm loại thiết bị {model.TenLoai}.");
        }

        public async Task<KetQuaDichVu> CapNhatAsync(int id, LoaiThietBi model, bool modelHopLe)
        {
            var loai = await _uow.LoaiThietBis.GetByIdAsync(id);
            if (loai == null) return KetQuaDichVu.KhongTimThay();

            ChuanHoa(model);
            var loi = await KiemTraTrungTenAsync(model, id);
            if (!modelHopLe || loi.Count > 0)
                return KetQuaDichVu.DuLieuKhongHopLe(loi);

            loai.TenLoai = model.TenLoai;
            loai.HangSanXuat = model.HangSanXuat;
            loai.MoTa = model.MoTa;
            loai.ThoiHanBaoHanhMacDinh = model.ThoiHanBaoHanhMacDinh;
            loai.TrangThai = model.TrangThai;

            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return KetQuaDichVu.DuLieuKhongHopLe(new[] { new LoiTruongNhap(nameof(model.TenLoai), LoiTrungTen) });
            }

            return KetQuaDichVu.Ok($"Đã cập nhật loại thiết bị {loai.TenLoai}.");
        }

        public async Task<KetQuaDichVu> DoiTrangThaiAsync(int id)
        {
            var loai = await _uow.LoaiThietBis.GetByIdAsync(id);
            if (loai == null) return KetQuaDichVu.KhongTimThay();

            loai.TrangThai = !loai.TrangThai;
            await _uow.SaveChangesAsync();

            return KetQuaDichVu.Ok(loai.TrangThai
                ? $"Đã cho phép sử dụng lại loại thiết bị {loai.TenLoai}."
                : $"Đã ngừng sử dụng loại thiết bị {loai.TenLoai}.");
        }

        public async Task<KetQuaDichVu> XoaAsync(int id)
        {
            var loai = await _uow.LoaiThietBis.GetByIdAsync(id);
            if (loai == null) return KetQuaDichVu.KhongTimThay();

            var soThietBi = await _uow.LoaiThietBis.DemSoThietBiTheoLoaiAsync(id);
            if (soThietBi > 0)
                return KetQuaDichVu.Loi($"Không thể xóa: loại thiết bị {loai.TenLoai} đang có {soThietBi} thiết bị sử dụng. Hãy chọn Ngừng sử dụng thay vì xóa.");

            _uow.LoaiThietBis.Remove(loai);
            try
            {
                await _uow.SaveChangesAsync();
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

        private async Task<List<LoiTruongNhap>> KiemTraTrungTenAsync(LoaiThietBi model, int id)
        {
            var loi = new List<LoiTruongNhap>();
            if (string.IsNullOrEmpty(model.TenLoai)) return loi;
            if (await _uow.LoaiThietBis.KiemTraTrungTenAsync(model.TenLoai, id))
                loi.Add(new(nameof(model.TenLoai), LoiTrungTen));
            return loi;
        }
    }
}
