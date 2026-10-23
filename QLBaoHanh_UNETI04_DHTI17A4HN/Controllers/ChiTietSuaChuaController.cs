using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;
using QLBaoHanh_UNETI04_DHTI17A4HN.Models;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Controllers
{
    public class ChiTietSuaChuaController : Controller
    {
        private readonly AppDbContext _context;

        public ChiTietSuaChuaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ChiTietSuaChua
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ChiTietSuaChuas.Include(c => c.KyThuatVien).Include(c => c.LinhKien).Include(c => c.PhieuSuaChua);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ChiTietSuaChua/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTietSuaChua = await _context.ChiTietSuaChuas
            .Include(c => c.PhieuSuaChua)
            .Include(c => c.KyThuatVien)
            .Include(c => c.LinhKien)
            .FirstOrDefaultAsync(c => c.MaChiTiet == id);
            if (chiTietSuaChua == null)
            {
                return NotFound();
            }

            return View(chiTietSuaChua);
        }

        // GET: ChiTietSuaChua/Create
        
        public IActionResult Create()
        {
            ViewData["MaPhieu"] = new SelectList(
                _context.PhieuSuaChuas, "MaPhieu", "MaPhieu");

            ViewData["MaKyThuatVien"] = new SelectList(
                _context.KyThuatViens, "MaKyThuatVien", "HoTen");

            ViewData["MaLinhKien"] = new SelectList(
                _context.LinhKiens, "MaLinhKien", "TenLinhKien");

            return View(new ChiTietSuaChua { SoLuong = 1 });
        }


        // POST: ChiTietSuaChua/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaChiTiet,MaPhieu,MaKyThuatVien,MaLinhKien,SoLuong,DonGia,KetQuaChanDoan,GhiChu")]
            ChiTietSuaChua chiTietSuaChua)
        {
            // Kiểm tra linh kiện nếu có sử dụng
            LinhKien? linhKien = null;

            if (chiTietSuaChua.MaLinhKien.HasValue)
            {
                linhKien = await _context.LinhKiens
                    .FirstOrDefaultAsync(l =>
                        l.MaLinhKien == chiTietSuaChua.MaLinhKien.Value);

                if (linhKien == null)
                {
                    ModelState.AddModelError(
                        nameof(chiTietSuaChua.MaLinhKien),
                        "Linh kiện không tồn tại.");
                }
                else
                {
                    if (!linhKien.TrangThai)
                    {
                        ModelState.AddModelError(
                            nameof(chiTietSuaChua.MaLinhKien),
                            "Linh kiện đã ngừng hoạt động.");
                    }

                    if (chiTietSuaChua.SoLuong > linhKien.SoLuongTon)
                    {
                        ModelState.AddModelError(
                            nameof(chiTietSuaChua.SoLuong),
                            "Số lượng sử dụng vượt quá số lượng tồn kho.");
                    }
                }
            }

            // Lấy đơn giá từ dữ liệu linh kiện, không tin giá gửi từ form
            if (linhKien != null)
            {
                chiTietSuaChua.DonGia = linhKien.DonGia;
            }

            // Tự tính thành tiền
            chiTietSuaChua.ThanhTien =
                chiTietSuaChua.SoLuong * chiTietSuaChua.DonGia;

            if (ModelState.IsValid)
            {
                if (linhKien != null)
                {
                    linhKien.SoLuongTon -= chiTietSuaChua.SoLuong;
                }

                _context.ChiTietSuaChuas.Add(chiTietSuaChua);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm chi tiết sửa chữa thành công.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaKyThuatVien"] = new SelectList(
                _context.KyThuatViens,
                "MaKyThuatVien", "HoTen",
                chiTietSuaChua.MaKyThuatVien);

            ViewData["MaLinhKien"] = new SelectList(
                _context.LinhKiens.Where(l => l.TrangThai && l.SoLuongTon > 0),
                "MaLinhKien", "TenLinhKien",
                chiTietSuaChua.MaLinhKien);

            ViewData["MaPhieu"] = new SelectList(
                _context.PhieuSuaChuas,
                "MaPhieu", "NoiDungLoi",
                chiTietSuaChua.MaPhieu);

            return View(chiTietSuaChua);
        }


        // GET: ChiTietSuaChua/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var chiTietSuaChua = await _context.ChiTietSuaChuas
                .FindAsync(id);

            if (chiTietSuaChua == null)
                return NotFound();

            ViewData["MaKyThuatVien"] = new SelectList(
                _context.KyThuatViens,
                "MaKyThuatVien", "HoTen",
                chiTietSuaChua.MaKyThuatVien);

            ViewData["MaLinhKien"] = new SelectList(
                _context.LinhKiens.Where(l => l.TrangThai),
                "MaLinhKien", "TenLinhKien",
                chiTietSuaChua.MaLinhKien);

            ViewData["MaPhieu"] = new SelectList(
                _context.PhieuSuaChuas,
                "MaPhieu", "NoiDungLoi",
                chiTietSuaChua.MaPhieu);

            ViewBag.GiaLinhKien = await _context.LinhKiens
                .Select(l => new
                {
                    l.MaLinhKien,
                    l.DonGia
                })
                .ToListAsync();

            return View(chiTietSuaChua);
        }

        // POST: ChiTietSuaChua/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaChiTiet,MaPhieu,MaKyThuatVien,MaLinhKien,SoLuong,DonGia,KetQuaChanDoan,GhiChu")]
            ChiTietSuaChua chiTietSuaChua)
        {
            if (id != chiTietSuaChua.MaChiTiet)
                return NotFound();

            // Lấy chi tiết cũ từ database
            var chiTietCu = await _context.ChiTietSuaChuas
                .FirstOrDefaultAsync(c => c.MaChiTiet == id);

            if (chiTietCu == null)
                return NotFound();

            // Lấy linh kiện mới nếu có sử dụng
            LinhKien? linhKienMoi = null;

            if (chiTietSuaChua.MaLinhKien.HasValue)
            {
                linhKienMoi = await _context.LinhKiens
                    .FirstOrDefaultAsync(l =>
                        l.MaLinhKien == chiTietSuaChua.MaLinhKien.Value);

                if (linhKienMoi == null)
                {
                    ModelState.AddModelError(
                        nameof(chiTietSuaChua.MaLinhKien),
                        "Linh kiện không tồn tại.");
                }
                else if (!linhKienMoi.TrangThai)
                {
                    ModelState.AddModelError(
                        nameof(chiTietSuaChua.MaLinhKien),
                        "Linh kiện đã ngừng hoạt động.");
                }
                else
                {
                    // Nếu vẫn dùng linh kiện cũ, cộng lại số lượng cũ
                    // để tính lượng tồn có thể sử dụng.
                    int tonCoTheDung = linhKienMoi.SoLuongTon;

                    if (chiTietCu.MaLinhKien == linhKienMoi.MaLinhKien)
                        tonCoTheDung += chiTietCu.SoLuong;

                    if (chiTietSuaChua.SoLuong > tonCoTheDung)
                    {
                        ModelState.AddModelError(
                            nameof(chiTietSuaChua.SoLuong),
                            "Số lượng sử dụng vượt quá tồn kho.");
                    }
                }
            }

            // Lấy đơn giá chính thức từ linh kiện
            if (chiTietSuaChua.MaLinhKien.HasValue)
            {
                if (linhKienMoi != null)
                {
                    chiTietSuaChua.DonGia = linhKienMoi.DonGia;
                }
            }
            else
            {
                chiTietSuaChua.DonGia = 0;
            }

            // Tự động tính thành tiền
            chiTietSuaChua.ThanhTien = chiTietSuaChua.SoLuong * chiTietSuaChua.DonGia;

            if (ModelState.IsValid)
            {
                // Hoàn lại số lượng của linh kiện cũ
                if (chiTietCu.MaLinhKien.HasValue)
                {
                    var linhKienCu = await _context.LinhKiens
                        .FirstOrDefaultAsync(l =>
                            l.MaLinhKien == chiTietCu.MaLinhKien.Value);

                    if (linhKienCu != null)
                        linhKienCu.SoLuongTon += chiTietCu.SoLuong;
                }

                // Trừ số lượng linh kiện mới
                if (linhKienMoi != null)
                    linhKienMoi.SoLuongTon -= chiTietSuaChua.SoLuong;

                // Cập nhật bản ghi đang được EF Core theo dõi
                chiTietCu.MaPhieu = chiTietSuaChua.MaPhieu;
                chiTietCu.MaKyThuatVien = chiTietSuaChua.MaKyThuatVien;
                chiTietCu.MaLinhKien = chiTietSuaChua.MaLinhKien;
                chiTietCu.SoLuong = chiTietSuaChua.SoLuong;
                chiTietCu.DonGia = chiTietSuaChua.DonGia;
                chiTietCu.ThanhTien = chiTietSuaChua.ThanhTien;
                chiTietCu.KetQuaChanDoan = chiTietSuaChua.KetQuaChanDoan;
                chiTietCu.GhiChu = chiTietSuaChua.GhiChu;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật chi tiết sửa chữa thành công.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaKyThuatVien"] = new SelectList(
                _context.KyThuatViens,
                "MaKyThuatVien", "HoTen",
                chiTietSuaChua.MaKyThuatVien);

            ViewData["MaLinhKien"] = new SelectList(
                _context.LinhKiens.Where(l =>
                    l.TrangThai &&
                    (l.SoLuongTon > 0 ||
                    l.MaLinhKien == chiTietSuaChua.MaLinhKien)),
                "MaLinhKien", "TenLinhKien",
                chiTietSuaChua.MaLinhKien);

            ViewData["MaPhieu"] = new SelectList(
                _context.PhieuSuaChuas,
                "MaPhieu", "NoiDungLoi",
                chiTietSuaChua.MaPhieu);

            ViewBag.GiaLinhKien = await _context.LinhKiens
            .Select(l => new
            {
                l.MaLinhKien,
                l.DonGia
            })
            .ToListAsync();
            return View(chiTietSuaChua);
        }


        // GET: ChiTietSuaChua/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTietSuaChua = await _context.ChiTietSuaChuas
                .Include(c => c.KyThuatVien)
                .Include(c => c.LinhKien)
                .Include(c => c.PhieuSuaChua)
                .FirstOrDefaultAsync(m => m.MaChiTiet == id);
            if (chiTietSuaChua == null)
            {
                return NotFound();
            }

            return View(chiTietSuaChua);
        }

        // POST: ChiTietSuaChua/Delete/5
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Tìm chi tiết sửa chữa cần xóa
            var chiTietSuaChua = await _context.ChiTietSuaChuas
                .FirstOrDefaultAsync(c => c.MaChiTiet == id);

            if (chiTietSuaChua == null)
            {
                return NotFound();
            }

            // Hoàn lại số lượng linh kiện vào kho
            if (chiTietSuaChua.MaLinhKien.HasValue)
            {
                var linhKien = await _context.LinhKiens
                    .FirstOrDefaultAsync(l =>
                        l.MaLinhKien == chiTietSuaChua.MaLinhKien.Value);

                if (linhKien != null)
                {
                    linhKien.SoLuongTon += chiTietSuaChua.SoLuong;
                }
            }

            // Xóa chi tiết sửa chữa
            _context.ChiTietSuaChuas.Remove(chiTietSuaChua);

            // Lưu cả thay đổi tồn kho và thao tác xóa
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa chi tiết sửa chữa và hoàn lại linh kiện thành công.";

            return RedirectToAction(nameof(Index));
        }


        private bool ChiTietSuaChuaExists(int id)
        {
            return _context.ChiTietSuaChuas.Any(e => e.MaChiTiet == id);
        }
    }
}
