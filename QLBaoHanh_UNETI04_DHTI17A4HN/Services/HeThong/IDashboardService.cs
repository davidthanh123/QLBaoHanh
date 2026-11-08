
namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public record ThongKeTrangThaiPhieu(
        string TrangThai,
        int SoLuong);

    public record ThongKeLoaiThietBi(
        string TenLoai,
        int SoLuong);

    public record ThongKeKyThuatVien(
        string HoTen,
        int SoPhieu);

    public record ThongKeDashboard(
        int TongKhachHang,
        int TongThietBi,
        int TongPhieuSuaChua,
        int TongLinhKien,
        int TongKyThuatVien,
        int TongChiTietSuaChua,
        decimal TongGiaTriSuaChua,
        List<ThongKeTrangThaiPhieu> TheoTrangThai,
        List<ThongKeLoaiThietBi> TheoLoaiThietBi,
        List<ThongKeKyThuatVien> TheoKyThuatVien,
        decimal ThoiGianSuaChuaTrungBinh,
        decimal TyLeSuaChuaBaoHanh);

    public interface IDashboardService
    {
        Task<ThongKeDashboard> LayThongKeAsync();
    }
}