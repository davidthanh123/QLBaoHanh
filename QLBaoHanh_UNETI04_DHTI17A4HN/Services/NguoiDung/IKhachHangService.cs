// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Quản lý khách hàng và hồ sơ cá nhân (tách từ KhachHangController).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public interface IKhachHangService
    {
        Task<List<KhachHang>> TimKiemAsync(string? tuKhoa, bool? trangThai);

        /// <summary>Trả về null nếu tài khoản chưa có hồ sơ khách hàng.</summary>
        Task<HoSoKhachHangViewModel?> LayHoSoAsync(int maTaiKhoan);

        /// <summary>Thành công thì DuLieu = họ tên mới để Controller ghi lại vào Session.</summary>
        Task<KetQuaDichVu<string>> CapNhatHoSoAsync(int maTaiKhoan, HoSoKhachHangViewModel model, bool modelHopLe);
    }
}
