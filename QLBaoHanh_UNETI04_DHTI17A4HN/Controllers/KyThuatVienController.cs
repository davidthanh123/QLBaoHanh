using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    public class KyThuatVienController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KyThuatVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Buổi 1: Chỉ lấy danh sách phiếu phân công cho KTV
        public async Task<IActionResult> DanhSachPhieu(int? maKTV)
        {
            var query = _context.PhieuSuaChuas
                .Include(p => p.KhachHang)
                .Include(p => p.ThietBi)
                .AsQueryable();

            if (maKTV.HasValue)
            {
                query = query.Where(p => p.MaKTV == maKTV.Value);
            }

            var dsPhieu = await query.OrderByDescending(p => p.NgayTiepNhan).ToListAsync();
            return View(dsPhieu);
        }
    }
}