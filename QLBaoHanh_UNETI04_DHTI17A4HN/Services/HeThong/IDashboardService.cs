// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Thống kê tổng quan cho trang Dashboard (tách từ DashboardController).

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public record ThongKeDashboard(
        int TongKhachHang,
        int TongThietBi,
        int TongPhieuSuaChua,
        int TongLinhKien,
        int TongKyThuatVien,
        int TongChiTietSuaChua,
        decimal TongGiaTriSuaChua);

    public interface IDashboardService
    {
        Task<ThongKeDashboard> LayThongKeAsync();
    }
}
