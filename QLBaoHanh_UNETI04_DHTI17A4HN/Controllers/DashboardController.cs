using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TongKhachHang =
                await _context.KhachHangs.CountAsync();

            ViewBag.TongThietBi =
                await _context.ThietBis.CountAsync();

            ViewBag.TongPhieuSuaChua =
                await _context.PhieuSuaChuas.CountAsync();

            ViewBag.TongLinhKien =
                await _context.LinhKiens.CountAsync();

            ViewBag.TongKyThuatVien =
                await _context.KyThuatViens.CountAsync();

            ViewBag.TongChiTietSuaChua =
                await _context.ChiTietSuaChuas.CountAsync();

            decimal tongGiaTri = await _context.ChiTietSuaChuas
                .SumAsync(c => (decimal?)c.ThanhTien) ?? 0m;

            ViewBag.TongGiaTriSuaChua = tongGiaTri;

            return View();
        }
    }
}