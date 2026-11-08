using Microsoft.AspNetCore.Mvc;
using QLBaoHanh_UNETI04_DHTI17A4HN.Services;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service) => _service = service;

        public async Task<IActionResult> Index()
        {
            var tk = await _service.LayThongKeAsync();

            ViewBag.TongKhachHang = tk.TongKhachHang;
            ViewBag.TongThietBi = tk.TongThietBi;
            ViewBag.TongPhieuSuaChua = tk.TongPhieuSuaChua;
            ViewBag.TongLinhKien = tk.TongLinhKien;
            ViewBag.TongKyThuatVien = tk.TongKyThuatVien;
            ViewBag.TongChiTietSuaChua = tk.TongChiTietSuaChua;
            ViewBag.TongGiaTriSuaChua = tk.TongGiaTriSuaChua;

            return View();
        }
    }
}
