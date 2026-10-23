
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    // Họ và tên: Nguyễn Văn Vũ
    // Mã sinh viên: Điền mã sinh viên của bạn
    // Nội dung thực hiện: Module 5 - Quản lý linh kiện.

    public class LinhKienController : Controller
    {
        private readonly AppDbContext _context;

        public LinhKienController(AppDbContext context)
        {
            _context = context;
        }

        // GET: LinhKien
        public async Task<IActionResult> Index(
            string? searchString,
            string? trangThai,
            string? sapXep)
        {
            var query = _context.LinhKiens
                .AsNoTracking()
                .AsQueryable();

            // Tìm theo tên linh kiện
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(l =>
                    l.TenLinhKien.Contains(searchString));
            }

            // Lọc trạng thái
            if (trangThai == "hoatdong")
            {
                query = query.Where(l => l.TrangThai);
            }
            else if (trangThai == "ngung")
            {
                query = query.Where(l => !l.TrangThai);
            }

            // Sắp xếp
            query = sapXep switch
            {
                "gia_tang" => query.OrderBy(l => l.DonGia),
                "gia_giam" => query.OrderByDescending(l => l.DonGia),
                "ton_tang" => query.OrderBy(l => l.SoLuongTon),
                "ton_giam" => query.OrderByDescending(l => l.SoLuongTon),
                _ => query.OrderBy(l => l.MaLinhKien)
            };

            ViewBag.SearchString = searchString;
            ViewBag.TrangThai = trangThai;
            ViewBag.SapXep = sapXep;

            return View(await query.ToListAsync());
        }

        // GET: LinhKien/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var linhKien = await _context.LinhKiens
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.MaLinhKien == id);

            if (linhKien == null)
                return NotFound();

            return View(linhKien);
        }

        // GET: LinhKien/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LinhKien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LinhKien linhKien)
        {
            if (await _context.LinhKiens.AnyAsync(l =>
                l.TenLinhKien == linhKien.TenLinhKien))
            {
                ModelState.AddModelError(
                    nameof(linhKien.TenLinhKien),
                    "Tên linh kiện đã tồn tại.");
            }

            if (!ModelState.IsValid)
                return View(linhKien);

            _context.LinhKiens.Add(linhKien);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm linh kiện thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: LinhKien/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var linhKien = await _context.LinhKiens.FindAsync(id);

            if (linhKien == null)
                return NotFound();

            return View(linhKien);
        }

        // POST: LinhKien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            LinhKien linhKien)
        {
            if (id != linhKien.MaLinhKien)
                return NotFound();

            if (await _context.LinhKiens.AnyAsync(l =>
                l.MaLinhKien != id &&
                l.TenLinhKien == linhKien.TenLinhKien))
            {
                ModelState.AddModelError(
                    nameof(linhKien.TenLinhKien),
                    "Tên linh kiện đã tồn tại.");
            }

            if (!ModelState.IsValid)
                return View(linhKien);

            // Lấy entity đang được theo dõi để tránh ghi đè
            // các thuộc tính điều hướng do form gửi lên.
            var entity = await _context.LinhKiens.FindAsync(id);

            if (entity == null)
                return NotFound();

            entity.TenLinhKien = linhKien.TenLinhKien;
            entity.DonViTinh = linhKien.DonViTinh;
            entity.DonGia = linhKien.DonGia;
            entity.SoLuongTon = linhKien.SoLuongTon;
            entity.MoTa = linhKien.MoTa;
            entity.TrangThai = linhKien.TrangThai;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể lưu thay đổi. Hãy kiểm tra dữ liệu.");
                return View(linhKien);
            }

            TempData["Success"] = "Cập nhật linh kiện thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: LinhKien/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var linhKien = await _context.LinhKiens
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.MaLinhKien == id);

            if (linhKien == null)
                return NotFound();

            return View(linhKien);
        }

        // POST: LinhKien/Delete/5
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var linhKien = await _context.LinhKiens
                .FirstOrDefaultAsync(l => l.MaLinhKien == id);

            if (linhKien == null)
            {
                return NotFound();
            }

            // Kiểm tra linh kiện đã được sử dụng chưa
            bool daSuDung = await _context.ChiTietSuaChuas
                .AnyAsync(c => c.MaLinhKien == id);

            if (daSuDung)
            {
                TempData["Error"] =
                    "Không thể xóa linh kiện đã được sử dụng trong phiếu sửa chữa.";

                return RedirectToAction(nameof(Index));
            }

            _context.LinhKiens.Remove(linhKien);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa linh kiện thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa linh kiện do dữ liệu đang được tham chiếu.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
