// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - nghiệp vụ đăng nhập: kiểm tra mật khẩu, khóa tạm khi sai nhiều lần, ghi nhật ký (tách từ TaiKhoanController).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public record ThongTinDangNhap(int MaTaiKhoan, string HoTen, VaiTroEnum VaiTro);

    public interface IDangNhapService
    {
        /// <summary>
        /// Thành công: DuLieu là thông tin để Controller ghi vào Session.
        /// Thất bại: Loai = LoiNghiepVu và ThongBao là câu hiển thị cho người dùng.
        /// Service không đụng Session; việc Clear/ghi Session là của Controller.
        /// </summary>
        Task<KetQuaDichVu<ThongTinDangNhap?>> DangNhapAsync(DangNhapViewModel model, string? diaChiIp);
    }
}
