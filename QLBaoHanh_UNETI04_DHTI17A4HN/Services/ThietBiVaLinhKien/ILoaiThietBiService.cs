// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 1 - nghiệp vụ Loại thiết bị (tách từ LoaiThietBiController).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public record ChiTietLoaiThietBi(LoaiThietBi Loai, int SoThietBi);

    public interface ILoaiThietBiService
    {
        Task<List<LoaiThietBiDongViewModel>> LayDanhSachAsync(string? tuKhoa, bool? trangThai);
        Task<ChiTietLoaiThietBi?> LayChiTietAsync(int id);
        Task<LoaiThietBi?> LayDeSuaAsync(int id);

        // modelHopLe = ModelState.IsValid của Controller (lỗi DataAnnotations).
        Task<KetQuaDichVu> ThemAsync(LoaiThietBi model, bool modelHopLe);
        Task<KetQuaDichVu> CapNhatAsync(int id, LoaiThietBi model, bool modelHopLe);
        Task<KetQuaDichVu> DoiTrangThaiAsync(int id);
        Task<KetQuaDichVu> XoaAsync(int id);
    }
}
