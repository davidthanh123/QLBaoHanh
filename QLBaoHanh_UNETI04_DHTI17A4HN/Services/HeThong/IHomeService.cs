// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Dữ liệu cho trang giới thiệu công khai và trang chủ sau đăng nhập (tách khỏi HomeController).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public interface IHomeService
    {
        // Trang giới thiệu (chưa đăng nhập): chỉ trả dữ liệu không nhạy cảm.
        Task<GioiThieuViewModel> LayGioiThieuAsync();

        // Trang chủ sau đăng nhập. Thông tin phiên do Controller lấy từ Session rồi truyền vào
        // (Service không đụng Session - Doc/QUYUOC_SERVICE.md mục 1).
        Task<HomeViewModel> LayTrangChuAsync(int? maTaiKhoan, VaiTroEnum? vaiTro, string? hoTen);
    }
}
