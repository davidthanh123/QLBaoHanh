// Họ và tên: Vân
// Mã sinh viên: 23103100203
// Nội dung thực hiện: Module 1 - nghiệp vụ CRUD Tài khoản (Admin): thêm, sửa, khóa/mở khóa, xóa an toàn (tách từ TaiKhoanController).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public record ChiTietTaiKhoan(TaiKhoan TaiKhoan, List<NhatKyDangNhap> NhatKy);

    // maDangNhap = mã tài khoản đang đăng nhập (lấy từ Session ở Controller).
    public interface ITaiKhoanService
    {
        Task<List<TaiKhoan>> TimKiemAsync(string? tuKhoa, VaiTroEnum? vaiTro, bool? trangThai);
        Task<ChiTietTaiKhoan?> LayChiTietAsync(int id);
        Task<TaiKhoanFormViewModel?> LayDeSuaAsync(int id);

        Task<KetQuaDichVu> ThemAsync(TaiKhoanFormViewModel model, bool modelHopLe);
        Task<KetQuaDichVu> CapNhatAsync(int id, TaiKhoanFormViewModel model, int? maDangNhap, bool modelHopLe);
        Task<KetQuaDichVu> DoiTrangThaiAsync(int id, int? maDangNhap);
        Task<KetQuaDichVu> XoaAsync(int id, int? maDangNhap);
    }
}
