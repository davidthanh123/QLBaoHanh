// Họ và tên: [điền họ tên của bạn]
// Mã sinh viên: [điền mã sinh viên của bạn]
// Nội dung thực hiện: Thống kê tổng quan cho trang Dashboard (tách từ DashboardController).

using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;
        public DashboardService(AppDbContext db) => _db = db;

        // Các truy vấn chạy tuần tự (không dùng Task.WhenAll) vì một DbContext không dùng song song được.
        public async Task<ThongKeDashboard> LayThongKeAsync()
        {
            var tongKhachHang = await _db.KhachHangs.CountAsync();
            var tongThietBi = await _db.ThietBis.CountAsync();
            var tongPhieu = await _db.PhieuSuaChuas.CountAsync();
            var tongLinhKien = await _db.LinhKiens.CountAsync();
            var tongKyThuatVien = await _db.KyThuatViens.CountAsync();
            var tongChiTiet = await _db.ChiTietSuaChuas.CountAsync();
            var tongGiaTri = await _db.ChiTietSuaChuas.SumAsync(c => (decimal?)c.ThanhTien) ?? 0m;

            return new ThongKeDashboard(tongKhachHang, tongThietBi, tongPhieu,
                tongLinhKien, tongKyThuatVien, tongChiTiet, tongGiaTri);
        }
    }
}
