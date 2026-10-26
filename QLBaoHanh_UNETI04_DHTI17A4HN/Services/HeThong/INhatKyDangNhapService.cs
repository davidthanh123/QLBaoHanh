// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 1 - Xem nhật ký đăng nhập (chỉ đọc), tách từ NhatKyDangNhapController.

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public record DanhSachNhatKy(List<NhatKyDangNhap> DanhSach, int SoDongToiDa);

    public interface INhatKyDangNhapService
    {
        Task<DanhSachNhatKy> LayDanhSachAsync(string? tuKhoa, KetQuaDangNhapEnum? ketQua);
    }
}
