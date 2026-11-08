
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models.Enums;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;

        public DashboardService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ThongKeDashboard> LayThongKeAsync()
        {
            // 1. Thống kê tổng quan
            var tongKhachHang = await _db.KhachHangs.CountAsync();
            var tongThietBi = await _db.ThietBis.CountAsync();
            var tongPhieu = await _db.PhieuSuaChuas.CountAsync();
            var tongLinhKien = await _db.LinhKiens.CountAsync();
            var tongKyThuatVien = await _db.KyThuatViens.CountAsync();
            var tongChiTiet = await _db.ChiTietSuaChuas.CountAsync();

            // 2. Tổng giá trị sửa chữa
            var tongGiaTri = await _db.ChiTietSuaChuas
                .SumAsync(c => (decimal?)c.ThanhTien) ?? 0m;

            // 3. Thống kê phiếu theo trạng thái
            var trangThaiData = await _db.PhieuSuaChuas
                .GroupBy(p => p.TrangThai)
                .Select(g => new
                {
                    TrangThai = g.Key,
                    SoLuong = g.Count()
                })
                .ToListAsync();

            var theoTrangThai = trangThaiData
                .Select(x => new ThongKeTrangThaiPhieu(
                    x.TrangThai.ToString(),
                    x.SoLuong))
                .ToList();

            // 4. Thống kê thiết bị theo loại
            var theoLoaiThietBi = await _db.ThietBis
                .GroupBy(t => t.LoaiThietBi!.TenLoai)
                .Select(g => new ThongKeLoaiThietBi(
                    g.Key,
                    g.Count()))
                .ToListAsync();

            // 5. Thống kê số phiếu của từng kỹ thuật viên
            // Một kỹ thuật viên chỉ được tính một lần cho mỗi phiếu.
            var theoKyThuatVien = await _db.ChiTietSuaChuas
                .GroupBy(c => new
                {
                    c.MaKyThuatVien,
                    HoTen = c.KyThuatVien!.HoTen
                })
                .Select(g => new
                {
                    g.Key.HoTen,
                    SoPhieu = g.Select(c => c.MaPhieu).Distinct().Count()
                })
                .OrderByDescending(x => x.SoPhieu)
                .Select(x => new ThongKeKyThuatVien(
                    x.HoTen,
                    x.SoPhieu))
                .ToListAsync();

            // 6. Thời gian sửa chữa trung bình (ngày)
            var thoiGianTrungBinh = await _db.PhieuSuaChuas
                .Where(p =>
                    p.TrangThai == TrangThaiPhieuEnum.HoanThanh
                    && p.NgayHoanThanh != null)
                .Select(p => (double?)EF.Functions.DateDiffDay(
                    p.NgayTiepNhan,
                    p.NgayHoanThanh!.Value))
                .AverageAsync() ?? 0;

            // 7. Tỷ lệ sửa chữa còn bảo hành (%)
            var tongPhieuCoThietBi = await _db.PhieuSuaChuas
                .CountAsync(p => p.ThietBi != null);

            var phieuConBaoHanh = await _db.PhieuSuaChuas
                .CountAsync(p =>
                    p.ThietBi != null &&
                    p.ThietBi.HanBaoHanh >= p.NgayTiepNhan);

            decimal tyLeBaoHanh = tongPhieuCoThietBi > 0
                ? (decimal)phieuConBaoHanh / tongPhieuCoThietBi * 100
                : 0;

            return new ThongKeDashboard(
                tongKhachHang,
                tongThietBi,
                tongPhieu,
                tongLinhKien,
                tongKyThuatVien,
                tongChiTiet,
                tongGiaTri,
                theoTrangThai,
                theoLoaiThietBi,
                theoKyThuatVien,
                (decimal)thoiGianTrungBinh,
                tyLeBaoHanh
            );
        }
    }
}
