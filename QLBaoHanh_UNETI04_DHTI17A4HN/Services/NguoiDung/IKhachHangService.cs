// Họ và tên: David Thành
// Mã sinh viên: 23103100239
// Nội dung thực hiện: Module 3 - hợp đồng (interface) quản lý khách hàng: danh sách, thêm/sửa/xóa, đổi trạng thái, hồ sơ cá nhân.

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public interface IKhachHangService
    {
        // ----- Admin / Nhân viên -----
        Task<KhachHangDanhSachViewModel> LayDanhSachAsync(string? tuKhoa, bool? trangThai, LoaiKhachHangEnum? loai, int trang);
        Task<KhachHang?> LayChiTietAsync(int maKhachHang);

        Task<KhachHangFormViewModel> TaoFormMoiAsync();
        Task<KhachHangFormViewModel?> LayFormSuaAsync(int maKhachHang);
        /// <summary>Nạp lại danh sách tài khoản cho dropdown khi form bị trả về do lỗi.</summary>
        Task NapLaiTaiKhoanAsync(KhachHangFormViewModel model);

        Task<KetQuaDichVu> TaoAsync(KhachHangFormViewModel model, bool modelHopLe);
        Task<KetQuaDichVu> CapNhatAsync(int maKhachHang, KhachHangFormViewModel model, bool modelHopLe);
        /// <summary>Đổi Hoạt động / Ngừng hoạt động (xóa mềm). Không cho ngừng khi còn phiếu chưa đóng.</summary>
        Task<KetQuaDichVu> DoiTrangThaiAsync(int maKhachHang);
        /// <summary>Chỉ xóa được khi chưa phát sinh thiết bị/phiếu.</summary>
        Task<KetQuaDichVu> XoaAsync(int maKhachHang);

        // ----- Khách hàng tự quản lý hồ sơ của mình -----
        /// <summary>Trả về null nếu tài khoản chưa có hồ sơ khách hàng.</summary>
        Task<HoSoKhachHangViewModel?> LayHoSoAsync(int maTaiKhoan);

        /// <summary>Thành công thì DuLieu = họ tên mới để Controller ghi lại vào Session.</summary>
        Task<KetQuaDichVu<string>> CapNhatHoSoAsync(int maTaiKhoan, HoSoKhachHangViewModel model, bool modelHopLe);
    }
}
