// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Thống kê tổng quan cho trang Dashboard (tách từ DashboardController).

using QLBaoHanh_UNETI04_DHTI17A4HN.Repositories;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;
        public DashboardService(IUnitOfWork uow) => _uow = uow;

        public async Task<ThongKeDashboard> LayThongKeAsync()
        {
            var tongKhachHang = await _uow.KhachHangs.CountAsync();
            var tongThietBi = await _uow.ThietBis.CountAsync();
            var tongPhieu = await _uow.PhieuSuaChuas.CountAsync();
            var tongLinhKien = await _uow.LinhKiens.CountAsync();
            var tongKyThuatVien = await _uow.KyThuatViens.CountAsync();
            var tongChiTiet = await _uow.ChiTietSuaChuas.CountAsync();
            var tongGiaTri = await _uow.ChiTietSuaChuas.TinhTongGiaTriSuaChuaAsync();

            return new ThongKeDashboard(tongKhachHang, tongThietBi, tongPhieu,
                tongLinhKien, tongKyThuatVien, tongChiTiet, tongGiaTri);
        }
    }
}
