// Họ và tên: Bùi Hồng Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - Interface Service NhatKyDangNhap - định nghĩa nghiệp vụ tra cứu nhật ký đăng nhập.

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
