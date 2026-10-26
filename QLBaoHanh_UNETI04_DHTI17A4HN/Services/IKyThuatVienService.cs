// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Module 4 - quản lý hồ sơ kỹ thuật viên (tách từ KyThuatVienController).

using QLBaoHanh_UNETI04_DHTI17A4HN.Models;
using QLBaoHanh_UNETI04_DHTI17A4HN.ViewModels;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public interface IKyThuatVienService
    {
        Task<List<KyThuatVien>> TimKiemAsync(string? tuKhoa, bool? trangThai);

        /// <summary>Tài khoản Nhân viên đang hoạt động, chưa có hồ sơ kỹ thuật viên (giữ lại tài khoản hiện tại khi sửa).</summary>
        Task<List<TaiKhoan>> LayTaiKhoanNhanVienAsync(int? taiKhoanHienTai = null);

        Task<KyThuatVienViewModel?> LayDeSuaAsync(int id);

        // Chỉ gọi khi ModelState đã hợp lệ (code cũ trả về View ngay nếu ModelState lỗi).
        Task<KetQuaDichVu> ThemAsync(KyThuatVienViewModel model);

        // DuLieu = MaTaiKhoan hiện đang gắn với hồ sơ (trước khi sửa), để Controller nạp lại dropdown khi form lỗi.
        Task<KetQuaDichVu<int>> CapNhatAsync(int id, KyThuatVienViewModel model, bool modelHopLe);
    }
}
