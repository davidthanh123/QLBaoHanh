// Họ và tên: Bùi Hồng Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - Interface Service LoaiThietBi - định nghĩa nghiệp vụ quản lý loại thiết bị.

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
